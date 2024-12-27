using Fusion;
using TMPro;
using UnityEngine;

public class InGameUI : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI numberOfPlayerText;
    [SerializeField] private TextMeshProUGUI countDownStartGameText;

    public override void Spawned()
    {
        Runner.SetIsSimulated(Object, true);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void Rpc_ShowCountDownStarting()
    {
        numberOfPlayerText.gameObject.SetActive(false);
        countDownStartGameText.gameObject.SetActive(true);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void Rpc_HideCountDownStarting()
    {
        countDownStartGameText.gameObject.SetActive(false);
    }

    public void SetWaitingPlayers(int count, int numberPlayerToStart)
    {
        numberOfPlayerText.text = $"{count}/{numberPlayerToStart}";
    }

    public void SetCountdownTimer(float? remainingTime)
    {
        countDownStartGameText.text = $"Game Starts In {Mathf.RoundToInt(remainingTime ?? 0)}";
    }
}