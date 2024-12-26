using Fusion;
using UnityEngine;

public class PlayerWeaponController : NetworkBehaviour
{
    [SerializeField] private NetworkPrefabRef bulletPrefab = NetworkPrefabRef.Empty;
    [SerializeField] private Transform firePointPos;
    [SerializeField] private float delayBetweenShots = 0.18f;
    [SerializeField] private ParticleSystem muzzleEffect;
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
        Runner.SetIsSimulated(Object, true);

        _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    public override void FixedUpdateNetwork()
    {
        if(Runner.TryGetInputForPlayer<PlayerNetworkInput>(Object.InputAuthority, out var input))
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

    private void CheckShootInput(PlayerNetworkInput input)
    {
        var currentBtns = input.NetworkButtons.GetPressed(buttonPrev);
        
        IsHoldingShootingKey = currentBtns.WasReleased(buttonPrev, PlayerInputButtons.Shoot);

        if (currentBtns.WasReleased(buttonPrev, PlayerInputButtons.Shoot) && shootCoolDown.ExpiredOrNotRunning(Runner))
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
