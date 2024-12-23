using Fusion;
using System;
using TMPro;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static bool IsMatchOver;
    public Action OnGameOver;

    [SerializeField] private Camera camera;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float matchTime;

    [Networked] private TickTimer matchTimer { get; set; }

    private void Awake()
    {
        if(GlobalManagers.Instance != null)
        {
            GlobalManagers.Instance.GameManager = this;
        }
    }

    public override void Spawned()
    {
        IsMatchOver = false;
        camera.gameObject.SetActive(false);
        matchTimer = TickTimer.CreateFromSeconds(Runner, matchTime);
    }

    public override void FixedUpdateNetwork()
    {
        if(!matchTimer.Expired(Runner) && matchTimer.RemainingTime(Runner).HasValue)
        {
            var timeSpan = TimeSpan.FromSeconds(matchTimer.RemainingTime(Runner).Value);
            timerText.text = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
        else if (matchTimer.Expired(Runner))
        {
            IsMatchOver = true;
            matchTimer = TickTimer.None;
            OnGameOver?.Invoke();
            Debug.Log("[GameManager] Match end");
        }
    }
}
