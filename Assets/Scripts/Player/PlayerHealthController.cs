using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthController : NetworkBehaviour
{
    [SerializeField] private Image healthFillAmount;
    [SerializeField] private Animator bloodScreenHitAnimator;
    [SerializeField] private TextMeshProUGUI healthAmountText;
    [SerializeField] private PlayerCameraController cameraController;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private LayerMask groundLayerMark;

    private const int MAX_HEALTH_AMOUNT = 100;
    private new Collider2D collider2D;

    [Networked] private int currentHealthAmount { get; set; }
    private ChangeDetector _changes;
    private GameStateManager gameManager;

    public override void Spawned()
    {
        Runner.SetIsSimulated(Object, true);
        currentHealthAmount = MAX_HEALTH_AMOUNT;
        collider2D = GetComponent<Collider2D>();
        _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);
        gameManager = GlobalManagers.Instance.GameManager;
    }

    // Call from server to server to deduct health
    [Rpc(RpcSources.StateAuthority, RpcTargets.StateAuthority)] 
    public void Rpc_DeductPlayerHealth(int damage)
    {
        DamagePlayer(damage);
    }

    private void DamagePlayer(int dmg)
    {
        currentHealthAmount -= dmg;
    }

    public override void FixedUpdateNetwork()
    {
        if (Runner.IsServer && playerController.IsPlayerAlive)
        {
            var isHitDeathGround = Runner.GetPhysicsScene2D().OverlapBox(transform.position, collider2D.bounds.size, 0, groundLayerMark);
            if (isHitDeathGround)
            {
                Rpc_DeductPlayerHealth(MAX_HEALTH_AMOUNT);
            }        
        }
    }

    public override void Render()
    {
        foreach (var change in _changes.DetectChanges(this, out var previousBuffer, out var currentBuffer))
        {
            switch (change)
            {
                case nameof(currentHealthAmount):
                    var reader = GetPropertyReader<int>(nameof(currentHealthAmount));
                    var (oldHealth, currentHealth) = reader.Read(previousBuffer, currentBuffer);
                    UpdateUIVisuals(currentHealth);

                    //If the player did not spawn OR respawned
                    //Only then do damage
                    if (currentHealth != MAX_HEALTH_AMOUNT)
                    {
                        GotHit(currentHealth);
                    }
                    break;
            }
        }
    }

    private void UpdateUIVisuals(int healthAmount)
    {
        var num = (float)healthAmount / MAX_HEALTH_AMOUNT;
        healthFillAmount.fillAmount = num;
        healthAmountText.text = $"{healthAmount}/{MAX_HEALTH_AMOUNT}";
    }

    private void GotHit(int healthAmount)
    {
        if (Object.IsLocalPlayer())
        {
            Debug.Log("local player got hit!");
            cameraController.ShakeCamera(new Vector3(0.2f, 0.1f));
            bloodScreenHitAnimator.Play("HeartIdle");
        }

        if (healthAmount <= 0)
        {
            if(gameManager.State == GameStateManager.GameState.Running)
            {
                playerController.KillPlayer();
                Debug.Log("Player is dead");
            }
            else
            {
                playerController.KillPlayer();
            }
        }
    }

    public void RestoreAllHealth()
    {
        currentHealthAmount = MAX_HEALTH_AMOUNT;
    }
}
