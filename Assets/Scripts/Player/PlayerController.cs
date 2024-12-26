using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 6;
    [SerializeField] private float jumpForce = 6;
    [SerializeField] private float respawnTime = 4;
    [SerializeField] private GameObject cam;
    [SerializeField] private PlayerHealthController playerHealthController;
    [field: SerializeField] public PlayerWeaponController PlayerWeaponController { get; private set; }
    
    [Header("Check Ground")]
    [SerializeField] private LayerMask groundLayerMark;
    [SerializeField] private Transform groundDefectTransform;

    private new Rigidbody2D rigidbody2D;
    private PlayerVisualController playerVisualController;
    private NetworkRigidbody2D networkRigidbody2D;
    private ChangeDetector _changes;

    public bool AcceptAnyInput => IsPlayerAlive && !GameManager.IsMatchOver;

    [Networked, HideInInspector] public NetworkBool IsPlayerAlive { get; set; }
    [Networked] public TickTimer RespawnTimer { get; set; }
    [Networked] private TickTimer respawnToNewSpawnPointTimer { get; set; }
    [Networked] private NetworkButtons buttonPrev { get; set; }
    [Networked] private Vector2 serverNextSpawnPoint { get; set; }

    public override void Spawned()
    {
        Runner.SetIsSimulated(Object, true);

        _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);
        rigidbody2D = GetComponent<Rigidbody2D>();
        playerVisualController = GetComponent<PlayerVisualController>();
        networkRigidbody2D = GetComponent<NetworkRigidbody2D>();
        IsPlayerAlive = true;
        SetLocalObjects();
    }

    private void SetLocalObjects()
    {
        if (Object.IsLocalPlayer())
        {
            cam.transform.SetParent(null);
            cam.SetActive(true);
        }
    }

    public override void FixedUpdateNetwork()
    {
        CheckRespawnTimer();

        if (Runner.TryGetInputForPlayer<PlayerNetworkInput>(Object.InputAuthority, out PlayerNetworkInput input))
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

    private void CheckJumpInput(PlayerNetworkInput input)
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
        playerVisualController.RendererVisuals(rigidbody2D.velocity, PlayerWeaponController.IsHoldingShootingKey);
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
