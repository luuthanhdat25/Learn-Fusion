using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private PlayerConfig playerConfig;
    [SerializeField] private GameObject cam;
    [SerializeField] private PlayerHealthController playerHealthController;
    [field: SerializeField] public PlayerWeaponController PlayerWeaponController { get; private set; }
    
    [Header("Check Ground")]
    [SerializeField] private LayerMask groundLayerMark;
    [SerializeField] private Transform groundDefectTransform;
    [SerializeField] private new Rigidbody2D rigidbody2D;
    [SerializeField] private PlayerVisualController playerVisualController;
    [SerializeField] private NetworkRigidbody2D networkRigidbody2D;

    private GameStateManager gameManager;

    public bool AcceptAnyInput => IsPlayerAlive && !GameStateManager.IsMatchOver;

    [Networked, HideInInspector] public NetworkBool IsPlayerAlive { get; set; }
    //[Networked] public TickTimer RespawnTimer { get; set; }
    //[Networked] private TickTimer respawnToNewSpawnPointTimer { get; set; }
    [Networked] private NetworkButtons buttonPrev { get; set; }
    [Networked] private Vector2 serverNextSpawnPoint { get; set; }

    public override void Spawned()
    {
        Runner.SetIsSimulated(Object, true);

        rigidbody2D = GetComponent<Rigidbody2D>();
        playerVisualController = GetComponent<PlayerVisualController>();
        gameManager = GlobalManagers.Instance.GameManager;
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
        //if(gameManager.State != GameStateManager.GameState.Running) CheckRespawnTimer();

        if (Runner.TryGetInputForPlayer<PlayerNetworkInput>(Object.InputAuthority, out PlayerNetworkInput input))
        {
            if (AcceptAnyInput)
            {
                rigidbody2D.velocity = new Vector2(input.HorizontalInput * playerConfig.MoveSpeed, rigidbody2D.velocity.y);

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
            rigidbody2D.AddForce(Vector2.up *  playerConfig.JumpForce, ForceMode2D.Force);
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

    //private void CheckRespawnTimer()
    //{
    //    if (IsPlayerAlive) return;

    //    if (respawnToNewSpawnPointTimer.Expired(Runner))
    //    {
    //        TeleportToPosition(serverNextSpawnPoint);
    //        respawnToNewSpawnPointTimer = TickTimer.None;
    //    }

    //    if (RespawnTimer.ExpiredOrNotRunning(Runner))
    //    {
    //        RespawnTimer = TickTimer.None;
    //        RespawnPlayer();
    //    }
    //}

    public void TeleportToPosition(Vector3 position) 
        => networkRigidbody2D.Teleport(position);

    public void RespawnWhenWaiting(Vector3 respawnPosition)
    {
        playerHealthController.RestoreAllHealth();
        TeleportToPosition(respawnPosition);
    }

    public void RespawnPlayer()
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
        //float respawnTime = playerConfig.RespawnTime;

        //if (Runner.IsServer)
        //{
            //serverNextSpawnPoint = GlobalManagers.Instance.PlayerSpawnerController.GetRandomSpawnPointPosition();
            //respawnToNewSpawnPointTimer = TickTimer.CreateFromSeconds(Runner, respawnTime/2);
        //}

        rigidbody2D.simulated = false;
        IsPlayerAlive = false;
        playerVisualController.TriggerDieAnimation();
        //RespawnTimer = TickTimer.CreateFromSeconds(Runner, respawnTime);
    }

    // Destroy completed player, not set active false
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        GlobalManagers.Instance.ObjectPoolingManager.RemoveNetworkObjectFromDic(Object);
        Destroy(gameObject);
    }
}
