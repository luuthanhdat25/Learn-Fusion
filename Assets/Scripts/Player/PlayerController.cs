using Fusion;
using Fusion.Addons.Physics;
using System;
using TMPro;
using UnityEngine;
using static Unity.Collections.Unicode;

public class PlayerController : NetworkBehaviour, IBeforeUpdate
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private float moveSpeed = 6;
    [SerializeField] private float jumpForce = 6;
    [SerializeField] private float respawnTime = 4;
    [SerializeField] private GameObject cam;
    [SerializeField] private PlayerHealthController playerHealthController;
    
    [Header("Check Ground")]
    [SerializeField] private LayerMask groundLayerMark;
    [SerializeField] private Transform groundDefectTransform;

    private float horizontal;
    private new Rigidbody2D rigidbody2D;
    private PlayerWeaponController playerWeaponController;
    private PlayerVisualController playerVisualController;
    private NetworkRigidbody2D networkRigidbody2D;
    private ChangeDetector _changes;

    public bool AcceptAnyInput => IsPlayerAlive && !GameManager.IsMatchOver;

    [Networked, HideInInspector] public NetworkBool IsPlayerAlive { get; set; }
    [Networked] public TickTimer RespawnTimer { get; set; }
    [Networked] private TickTimer respawnToNewSpawnPointTimer { get; set; }
    [Networked] private NetworkButtons buttonPrev { get; set; }
    [Networked] private Vector2 serverNextSpawnPoint { get; set; }
    [Networked] private NetworkString<_8> playerName { get; set; }

    public enum PlayerInputButtons
    {
        None,
        Jump,
        Shoot
    }

    public override void Spawned()
    {
        Runner.SetIsSimulated(Object, true);

        _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);
        rigidbody2D = GetComponent<Rigidbody2D>();
        playerWeaponController = GetComponent<PlayerWeaponController>();
        playerVisualController = GetComponent<PlayerVisualController>();
        networkRigidbody2D = GetComponent<NetworkRigidbody2D>();

        GlobalManagers.Instance.PlayerSpawnerController.AddToEntry(base.Object.InputAuthority, base.Object);

        IsPlayerAlive = true;
        SetLocalObjects();
    }

    private void SetLocalObjects()
    {
        if (Object.IsLocalPlayer())
        {
            cam.transform.SetParent(null);
            cam.SetActive(true);

            //Update new join player nickname to all client
            var nickName = GlobalManagers.Instance.NetworkRunnerController.LocalPlayerNickName;
            Rpc_SetNickName(nickName);
        }
    }

    //Send RPC to Host
    //RpcTargets defines on which it is executed!
    [Rpc(sources:RpcSources.InputAuthority, RpcTargets.StateAuthority)] //Client send to Server
    private void Rpc_SetNickName(NetworkString<_8> nickName)
    {
        playerName = nickName;
    }

    public void BeforeUpdate()
    {
        if(Object.IsLocalPlayer() && AcceptAnyInput)
        {
            horizontal = Input.GetAxisRaw("Horizontal");
        }
    }

    public override void FixedUpdateNetwork()
    {
        CheckRespawnTimer();

        if (Runner.TryGetInputForPlayer<PlayerData>(Object.InputAuthority, out PlayerData input))
        {
            if (AcceptAnyInput)
            {
                rigidbody2D.velocity = new Vector2(input.HorizontalInput * moveSpeed, rigidbody2D.velocity.y);

                CheckJumpInput(input);
                buttonPrev = input.NetworkButtons;
            }
            else
            {
                rigidbody2D.velocity = Vector2.zero;
            }
        }

        playerVisualController.UpdateScaleTransform(rigidbody2D.velocity);
    }

    private void CheckJumpInput(PlayerData input)
    {
        var pressed = input.NetworkButtons.GetPressed(buttonPrev);
        if(pressed.WasPressed(buttonPrev, PlayerInputButtons.Jump) && IsGround())
        {
            rigidbody2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Force);
        }
    }

    public bool IsGround()
    {
        return Runner.GetPhysicsScene2D().OverlapBox(
            groundDefectTransform.position, 
            groundDefectTransform.localScale, 
            0, 
            groundLayerMark);
    }

    private void CheckRespawnTimer()
    {
        if (IsPlayerAlive) return;

        if (respawnToNewSpawnPointTimer.Expired(Runner))
        {
            GetComponent<NetworkRigidbody2D>().Teleport(serverNextSpawnPoint);
            respawnToNewSpawnPointTimer = TickTimer.None;
        }

        if (RespawnTimer.ExpiredOrNotRunning(Runner))
        {
            RespawnTimer = TickTimer.None;
            RespawnPlayer();
        }
    }

    private void RespawnPlayer()
    {
        rigidbody2D.simulated = true;
        IsPlayerAlive = true;
        playerVisualController.TriggerRespawnAnimation();
        playerHealthController.RestoreAllHealth();
    }

    public override void Render()
    {
        foreach (var change in _changes.DetectChanges(this, out var previousBuffer, out var currentBuffer))
        {
            switch (change)
            {
                case nameof(playerName):
                    var reader = GetPropertyReader<NetworkString<_8>>(nameof(playerName));
                    var (oldName, newName) = reader.Read(previousBuffer, currentBuffer);
                    SetPlayerNickName(newName);
                    break;
            }
        }
        playerVisualController.RendererVisuals(rigidbody2D.velocity, playerWeaponController.IsHoldingShootingKey);
    }

    private void SetPlayerNickName(NetworkString<_8> nickName)
    {
        playerNameText.text = nickName + " " + Object.InputAuthority.PlayerId;
    }

    public PlayerData GetPlayerDataInput()
    {
        PlayerData playerData = new PlayerData();
        playerData.HorizontalInput = horizontal;
        playerData.GunPivotRotation = playerWeaponController.LocalQuaternionPivotRot;
        playerData.NetworkButtons.Set(PlayerInputButtons.Jump, Input.GetKey(KeyCode.Space));
        playerData.NetworkButtons.Set(PlayerInputButtons.Shoot, Input.GetButton("Fire1"));
        return playerData;
    }

    public void KillPlayer()
    {
        if (Runner.IsServer)
        {
            serverNextSpawnPoint = GlobalManagers.Instance.PlayerSpawnerController.GetRandomSpawnPointPosition();
            respawnToNewSpawnPointTimer = TickTimer.CreateFromSeconds(Runner, respawnTime - 1);
        }

        rigidbody2D.simulated = false;
        IsPlayerAlive = false;
        playerVisualController.TriggerDieAnimation();
        RespawnTimer = TickTimer.CreateFromSeconds(Runner, respawnTime);
    }

    // Destroy completed player, not set active false
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        GlobalManagers.Instance.ObjectPoolingManager.RemoveNetworkObjectFromDic(Object);
        Destroy(gameObject);
    }
}
