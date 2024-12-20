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

    [Networked(OnChanged = nameof(HealthValueChanged))] private int currentHealthAmount { get; set; }
    private const int MAX_HEALTH_AMOUNT = 100;

    public override void Spawned()
    {
        currentHealthAmount = MAX_HEALTH_AMOUNT;
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

    private static void HealthValueChanged(Changed<PlayerHealthController> changed)
    {
        var currentHealth = changed.Behaviour.currentHealthAmount;

        changed.LoadOld();
        var oldHealth = changed.Behaviour.currentHealthAmount;

        if (currentHealth != oldHealth)
        {
            changed.Behaviour.UpdateUIVisuals(currentHealth);

            //If the player did not spawn OR respawned
            //Only then do damage
            if (currentHealth != MAX_HEALTH_AMOUNT)
            {
                changed.Behaviour.GotHit(currentHealth);
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
        var isLocalPlayer = Runner.LocalPlayer == Object.HasInputAuthority;
        if (isLocalPlayer)
        {
            Debug.Log("local player got hit!");
            cameraController.ShakeCamera(new Vector3(0.2f, 0.1f));
            bloodScreenHitAnimator.Play("HeartIdle");
        }

        if (healthAmount <= 0)
        {
            //todo kill player
            Debug.Log("Player is dead");
        }
    }
}
