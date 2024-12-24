using UnityEngine;
using Fusion;
using TMPro;

public class RespawnPanel : SimulationBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private TextMeshProUGUI respawnAmountText;
    [SerializeField] private GameObject childObject;

    public override void FixedUpdateNetwork()
    {
        if (playerController.Object.IsLocalPlayer())
        {
            bool isTimerRunning = playerController.RespawnTimer.IsRunning;

            childObject.SetActive(isTimerRunning);

            if (isTimerRunning && playerController.RespawnTimer.RemainingTime(Runner).HasValue)
            {
                float timerFloat = playerController.RespawnTimer.RemainingTime(Runner).Value;
                respawnAmountText.text = Mathf.RoundToInt(timerFloat).ToString();
            }
        }
    }
}
