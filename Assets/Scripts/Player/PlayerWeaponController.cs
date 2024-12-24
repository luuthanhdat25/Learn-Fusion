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
    [Networked] private NetworkBool playMuzzleEffect { get; set; }
    [Networked, HideInInspector] public NetworkBool IsHoldingShootingKey { get; set; }
    private ChangeDetector _changes;

    public override void Spawned()
    {
        _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    public void BeforeUpdate()
    {
        if(Runner.LocalPlayer == Object.InputAuthority && playerController.AcceptAnyInput)
        {
            var direction = localCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            LocalQuaternionPivotRot = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if(Runner.TryGetInputForPlayer<PlayerData>(Object.InputAuthority, out var input))
        {
            if (playerController.AcceptAnyInput)
            {
                CheckShootInput(input);

                // Just one player can change currentPlayerPivotRotation
                // When it change, it will update currentPlayerPivotRotation for all client
                currentPlayerPivotRotation = input.GunPivotRotation;

            }
            else
            {
                IsHoldingShootingKey = false;
                playMuzzleEffect = false;
                buttonPrev = default;
            }

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
            if (Runner.IsServer)
            {
                Runner.Spawn(bulletPrefab, firePointPos.position, firePointPos.rotation, Object.InputAuthority);
            }
        }
        else
        {
            playMuzzleEffect = false;
        }

        buttonPrev = input.NetworkButtons;
    }

    public override void Render()
    {
        foreach (var change in _changes.DetectChanges(this, out var previousBuffer, out var currentBuffer))
        {
            switch (change)
            {
                case nameof(playMuzzleEffect):
                    var reader = GetPropertyReader<NetworkBool>(nameof(playMuzzleEffect));
                    var (oldValue, newValue) = reader.Read(previousBuffer, currentBuffer);
                    PlayOrStopMuzzleEffect(newValue);
                    break;
            }
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
