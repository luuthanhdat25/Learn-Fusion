using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LocalInputPoller : NetworkBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Camera localCamera;

    private const string AXIS_HORIZONTAL = "Horizontal";
    private const string BUTTON_FIRE1 = "Fire1";

    public override void Spawned()
    {
        if (Object.IsLocalPlayer())
        {
            Runner.AddCallbacks(this);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (runner == null || !runner.IsRunning || !playerController.AcceptAnyInput) return;

        PlayerNetworkInput localInput = new PlayerNetworkInput();
        localInput.HorizontalInput = Input.GetAxisRaw(AXIS_HORIZONTAL);
        localInput.GunPivotRotation = CalculateQuaternionPivotRotateFromMouse();
        localInput.NetworkButtons.Set(PlayerInputButtons.Jump, Input.GetKey(KeyCode.Space));
        localInput.NetworkButtons.Set(PlayerInputButtons.Shoot, Input.GetButton(BUTTON_FIRE1));
        input.Set(localInput);
    }

    private Quaternion CalculateQuaternionPivotRotateFromMouse()
    {
        var direction = localCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return Quaternion.AngleAxis(angle, Vector3.forward);
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

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
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
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }
}
