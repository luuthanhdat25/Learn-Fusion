using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.SceneManagement;

public class NetworkRunnerController : MonoBehaviour, INetworkRunnerCallbacks
{
    public event Action OnStartedRunnerConnection;
    public event Action OnPlayerJoinedSuccessfully;

    [SerializeField] private NetworkRunner networkRunnerPrefab;

    private NetworkRunner networkRunnerInstance;

    public string LocalPlayerNickName { get; private set; }

    public void SetPlayerNickName(string str)
    {
        LocalPlayerNickName = str;
    }

    public void ShutDownRunner()
    {
        networkRunnerInstance.Shutdown();
    }

    public async void StartGame(GameMode gameMode, string roomName)
    {
        OnStartedRunnerConnection?.Invoke();

        if(networkRunnerInstance == null)
        {
            networkRunnerInstance = Instantiate(networkRunnerPrefab);
        }

        networkRunnerInstance.AddCallbacks(this);

        networkRunnerInstance.ProvideInput = true;

        var startGameArgs = new StartGameArgs()
        {
            GameMode = gameMode,
            SessionName = roomName,
            PlayerCount = 4,
            SceneManager = networkRunnerInstance.GetComponent<INetworkSceneManager>()
        };

        var result = await networkRunnerInstance.StartGame(startGameArgs);
        if (result.Ok)
        {
            networkRunnerInstance.SetActiveScene("MainGame");
        }
        else
        {
            Debug.LogError($"Failed to start: {result.ShutdownReason}");
        }
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        OnPlayerJoinedSuccessfully?.Invoke();
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        SceneManager.LoadScene("Lobby");
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }
}
