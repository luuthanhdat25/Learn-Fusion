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

    [SerializeField] private float startDelay = 4.0f;
    [SerializeField] private int numberPlayerToStart = 2;
    [Networked] private TickTimer timer { get; set; }
    [Networked] public GameState State { get; private set; }

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
    }

    public override void Spawned()
    {
        Runner.SetIsSimulated(Object, true);
        IsMatchOver = false;
        camera.gameObject.SetActive(false);
        timer = TickTimer.CreateFromSeconds(Runner, startDelay);
    }

    public override void FixedUpdateNetwork()
    {
        switch (State)
        {
            case GameState.Waiting:
                int playerCount = Runner.ActivePlayers.Count();
                inGameUI.SetWaitingPlayers(playerCount, numberPlayerToStart);
                if(playerCount >= numberPlayerToStart && Runner.IsServer)
                {
                    Runner.SessionInfo.IsOpen = false;

                    State = GameState.Starting;
                    timer = TickTimer.CreateFromSeconds(Runner, startDelay);
                    inGameUI.Rpc_ShowCountDownStarting();
                    //GlobalManagers.Instance.PlayerSpawnerController.HideAndTeleportAllPlayerToStartPosition();
                }
                break;

            case GameState.Starting:
                inGameUI.SetCountdownTimer(timer.RemainingTime(Runner));

                if (Object.HasStateAuthority && timer.Expired(Runner))
                {
                    State = GameState.Running;
                    inGameUI.Rpc_HideCountDownStarting();
                    //GlobalManagers.Instance.PlayerSpawnerController.RespawnAllPlayerToStartGame();
                }
                break;

            case GameState.Running:
                /*if (timer.Expired(Runner))
                    EndGame();*/
                //When hvae 1 player left
                break;

            case GameState.Ending:
                /*UI?.SetWinner(_winner, timer.RemainingTime(Runner));

                if (timer.Expired(Runner))
                    Runner.Shutdown();*/
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
