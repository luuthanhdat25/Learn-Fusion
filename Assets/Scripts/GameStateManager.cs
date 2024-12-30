using Fusion;
using System;
using System.Linq;
using UnityEngine;

public class GameStateManager : NetworkBehaviour
{
    public static bool IsMatchOver;

    [SerializeField] private new Camera camera;
    [field:SerializeField] public Collider2D CameraBound { get; private set; }
    [SerializeField] private InGameUI inGameUI;

    [SerializeField] private GameConfig gameConfig;
    [Networked] private TickTimer timer { get; set; }
    [Networked, HideInInspector] public GameState State { get; private set; }

    public enum GameState
    {
        Waiting,
        Starting,
        Running,
        Ending
    }

    private void Awake()
    {
        if(GlobalManagers.Instance != null)
        {
            GlobalManagers.Instance.GameManager = this;
        }

        inGameUI.SetWaitingPlayers(1, gameConfig.NumberPlayerToStart);
    }

    public override void Spawned()
    {
        Runner.SetIsSimulated(Object, true);
        IsMatchOver = false;
        camera.gameObject.SetActive(false);
        timer = TickTimer.CreateFromSeconds(Runner, gameConfig.StartDelay);
    }

    public override void FixedUpdateNetwork()
    {
        switch (State)
        {
            case GameState.Waiting:
                int playerCount = Runner.ActivePlayers.Count();
                int numberPlayerToStart = gameConfig.NumberPlayerToStart;
                inGameUI.SetWaitingPlayers(playerCount, numberPlayerToStart);
                
                if(playerCount >= numberPlayerToStart && Runner.IsServer)
                {
                    Runner.SessionInfo.IsOpen = false;

                    State = GameState.Starting;
                    timer = TickTimer.CreateFromSeconds(Runner, gameConfig.StartDelay);
                    inGameUI.Rpc_ShowCountDownStarting();
                }
                break;

            case GameState.Starting:
                inGameUI.SetCountdownTimer(timer.RemainingTime(Runner));

                if (Runner.IsServer && timer.Expired(Runner))
                {
                    State = GameState.Running;
                    inGameUI.Rpc_HideCountDownStarting();
                    GlobalManagers.Instance.PlayerSpawnerController.RespawnAllPlayerToStartGame();
                }
                break;

            case GameState.Running:
                break;

            case GameState.Ending:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void EndGame()
    {
        if (!Runner.IsServer) return;
        var finalPlayer = GlobalManagers.Instance.PlayerSpawnerController.GetFinalPlayerAlive();
        if(finalPlayer != null)
        {
            finalPlayer.Rpc_ShowGameOverPanel(1);
        }
        else
        {
            Debug.LogError($"[{nameof(GameStateManager)}] Final Player is NULL");
        }
        State = GameState.Ending;
    }
}
