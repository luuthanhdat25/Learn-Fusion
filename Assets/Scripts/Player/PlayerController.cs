using Fusion;
using TMPro;
using UnityEngine;

public class PlayerController : NetworkBehaviour, IBeforeUpdate
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private float moveSpeed = 6;
    [SerializeField] private float jumpForce = 6;
    [SerializeField] private float respawnTime = 3;
    [SerializeField] private GameObject cam;
    [SerializeField] private PlayerHealthController playerHealthController;

    private float horizontal;
    private Rigidbody2D rigidbody2D;
    private PlayerWeaponController playerWeaponController;
    private PlayerVisualController playerVisualController;

    [Networked, HideInInspector] public NetworkBool IsPlayerAlive { get; set; }
    [Networked] public TickTimer RespawnTimer { get; set; }
    [Networked] private NetworkButtons buttonPrev { get; set; }
    [Networked] private Vector2 serverNextSpawnPoint { get; set; }
    [Networked(OnChanged = nameof(OnNickNameChange))] private NetworkString<_8> playerName { get; set; }

    private static void OnNickNameChange(Changed<PlayerController> changed)
    {
        var nickName = changed.Behaviour.playerName;
        changed.Behaviour.SetPlayerNickName(nickName);
    }

    private void SetPlayerNickName(NetworkString<_8> nickName)
    {
        playerNameText.text = nickName + " " + Object.InputAuthority.PlayerId;
    }

    public enum PlayerInputButtons
    {
        None,
        Jump,
        Shoot
    }

    public override void Spawned()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        playerWeaponController = GetComponent<PlayerWeaponController>();
        playerVisualController = GetComponent<PlayerVisualController>();
        IsPlayerAlive = true;
        SetLocalObjects();
    }

    private void SetLocalObjects()
    {
        if (Runner.LocalPlayer == Object.InputAuthority)
        {
            cam.SetActive(true);

            //Update new join player nickname to all client
            var nickName = GlobalManagers.Instance.NetworkRunnerController.LocalPlayerNickName;
            RpcSetNickName(nickName);
        }
        else
        {
            //Make sure all proxy in screen is snapshot, not be predicted, easier to caculate Lag Compensation
            GetComponent<NetworkRigidbody2D>().InterpolationDataSource = InterpolationDataSources.Snapshots;
        }
    }

    //Send RPC to Host
    //RpcTargets defines on which it is executed!
    [Rpc(sources:RpcSources.InputAuthority, RpcTargets.StateAuthority)] //Client send to Server
    private void RpcSetNickName(NetworkString<_8> nickName)
    {
        playerName = nickName;
    }

    public void BeforeUpdate()
    {
        if(Runner.LocalPlayer == Object.InputAuthority && IsPlayerAlive)
        {
            horizontal = Input.GetAxisRaw("Horizontal");
        }
    }

    public override void FixedUpdateNetwork()
    {
        CheckSpawnTimer();

        if (Runner.TryGetInputForPlayer<PlayerData>(Object.InputAuthority, out PlayerData input) && IsPlayerAlive)
        {
            rigidbody2D.velocity = new Vector2(input.HorizontalInput * moveSpeed, rigidbody2D.velocity.y);

            CheckJumpInput(input);
        }

        playerVisualController.UpdateScaleTransform(rigidbody2D.velocity);
    }

    private void CheckJumpInput(PlayerData input)
    {
        var pressed = input.NetworkButtons.GetPressed(buttonPrev);
        if(pressed.WasPressed(buttonPrev, PlayerInputButtons.Jump))
        {
            rigidbody2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Force);
        }

        buttonPrev = input.NetworkButtons;
    }

    private void CheckSpawnTimer()
    {
        if (IsPlayerAlive) return;

        if(RespawnTimer.ExpiredOrNotRunning(Runner))
        {
            RespawnTimer = TickTimer.None;
            RespawnPlayer();
        }
    }

    private void RespawnPlayer()
    {
        rigidbody2D.simulated = true;
        rigidbody2D.position = serverNextSpawnPoint;
        IsPlayerAlive = true;
        playerVisualController.TriggerRespawnAnimation();
        playerHealthController.RestoreAllHealth();
    }

    public override void Render()
    {
        playerVisualController.RendererVisuals(rigidbody2D.velocity, playerWeaponController.IsHoldingShootingKey);
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
        }
        rigidbody2D.simulated = false;
        IsPlayerAlive = false;
        playerVisualController.TriggerDieAnimation();
        RespawnTimer = TickTimer.CreateFromSeconds(Runner, respawnTime);
    }
}
