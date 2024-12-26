using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private LayerMask playerLayerMask;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float moveSpeed = 20;
    [SerializeField] private float lifeTimeAmount = 0.8f;
    [SerializeField] private int damage = 10;

    [Networked] private NetworkBool didHitSomething { get; set; }
    [Networked] private TickTimer lifeTimeTimer { get; set; }
    private Collider2D coll;

    public override void Spawned()
    {
        Runner.SetIsSimulated(Object, true);

        coll = GetComponent<Collider2D>();
        lifeTimeTimer = TickTimer.CreateFromSeconds(Runner, lifeTimeAmount);
    }

    public override void FixedUpdateNetwork()
    {
        if (!didHitSomething)
        {
            CheckIfHitGround();
            CheckIfWeHitAPlayer();
        }

        if (lifeTimeTimer.ExpiredOrNotRunning(Runner) == false && !didHitSomething)
        {
            transform.Translate(transform.right * moveSpeed * Runner.DeltaTime, Space.World);
        }
        else
        {
            lifeTimeTimer = TickTimer.None;
            Runner.Despawn(Object);
        }
    }

    private void CheckIfHitGround()
    {
        var groundCollider = Runner.GetPhysicsScene2D()
            .OverlapBox(transform.position, coll.bounds.size, 0, groundLayerMask);

        if (groundCollider != default)
        {
            didHitSomething = true;
        }
    }

    private List<LagCompensatedHit> hits = new List<LagCompensatedHit>();
    private void CheckIfWeHitAPlayer()
    {
        Runner.LagCompensation.OverlapBox(transform.position, coll.bounds.size, Quaternion.identity,
            Object.InputAuthority, hits, playerLayerMask);

        if (hits.Count > 0)
        {
            foreach (var item in hits)
            {
                if (item.Hitbox != null)
                {
                    var player = item.Hitbox.GetComponentInParent<PlayerController>();

                    if (player.Object.InputAuthority.PlayerId != Object.InputAuthority.PlayerId
                        && player.IsPlayerAlive)
                    {
                        // Because the bullet can call multiple time difference by FUN
                        // Use rpc to make sure it just reduce 1 time from server
                        if (Runner.IsServer)
                        {
                            player.GetComponent<PlayerHealthController>().Rpc_DeductPlayerHealth(damage);
                        }

                        didHitSomething = true;
                        break;
                    }
                }
            }
        }
    }
}
