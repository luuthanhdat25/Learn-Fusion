using Fusion;
using UnityEngine;

public class PlayerWeaponController : NetworkBehaviour, IBeforeUpdate
{
    public Quaternion LocalQuaternionPivotRot { get; private set; }
    [SerializeField] private NetworkPrefabRef bulletPrefab = NetworkPrefabRef.Empty;
    [SerializeField] private Transform firePointPos;
    [SerializeField] private float delayBetweenShots = 0.18f;
    [SerializeField] private ParticleSystem muzzleEffect;
    [SerializeField] private Camera localCamera;
    [SerializeField] private Transform pivotToRotate;
    [SerializeField] private PlayerController playerController;

    // Networked help currentPlayerPivotRotation synchronized in all client
    [Networked] private Quaternion currentPlayerPivotRotation { get; set; }
    [Networked] private NetworkButtons buttonPrev { get; set; }
    [Networked] private TickTimer shootCoolDown { get; set; }
    [Networked(OnChanged = nameof(OnMuzzleEffectStateChanged))] private NetworkBool playMuzzleEffect { get; set; }
    [Networked, HideInInspector] public NetworkBool IsHoldingShootingKey { get; set; }

    public void BeforeUpdate()
    {
        if(Runner.LocalPlayer == Object.InputAuthority && playerController.IsPlayerAlive)
        {
            var direction = localCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            LocalQuaternionPivotRot = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if(Runner.TryGetInputForPlayer<PlayerData>(Object.InputAuthority, out var input) && playerController.IsPlayerAlive)
        {
            CheckShootInput(input);

            // Just one player can change currentPlayerPivotRotation
            // When it change, it will update currentPlayerPivotRotation for all client
            currentPlayerPivotRotation = input.GunPivotRotation;
        }

        pivotToRotate.rotation = currentPlayerPivotRotation;
    }

    private void CheckShootInput(PlayerData input)
    {
        var currentBtns = input.NetworkButtons.GetPressed(buttonPrev);
        
        IsHoldingShootingKey = currentBtns.WasReleased(buttonPrev, PlayerController.PlayerInputButtons.Shoot);

        if (currentBtns.WasReleased(buttonPrev, PlayerController.PlayerInputButtons.Shoot) && shootCoolDown.ExpiredOrNotRunning(Runner))
        {
            shootCoolDown = TickTimer.CreateFromSeconds(Runner, delayBetweenShots);
            playMuzzleEffect = true;
            Runner.Spawn(bulletPrefab, firePointPos.position, firePointPos.rotation, Object.InputAuthority);
        }
        else
        {
            playMuzzleEffect = false;
        }

        buttonPrev = input.NetworkButtons;
    }

    private static void OnMuzzleEffectStateChanged(Changed<PlayerWeaponController> changed)
    {
        var currentState = changed.Behaviour.playMuzzleEffect;
        changed.LoadOld();
        var oldState = changed.Behaviour.playMuzzleEffect;

        if (currentState != oldState)
        {
            changed.Behaviour.PlayOrStopMuzzleEffect(currentState);
        }
    }

    private void PlayOrStopMuzzleEffect(bool isPlay)
    {
        if (isPlay)
        {
            muzzleEffect.Play();
        }
        else
        {
            muzzleEffect.Stop();
        }
    }
}
