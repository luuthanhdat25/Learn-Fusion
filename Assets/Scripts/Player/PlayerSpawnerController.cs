using Fusion;
using System;
using UnityEngine;

public class PlayerSpawnerController : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    [SerializeField] private NetworkPrefabRef playerNetworkPrefab = NetworkPrefabRef.Empty;
    [SerializeField] private Transform[] spawnPoints;

    private void Awake()
    {
        if(GlobalManagers.Instance != null)
        {
            GlobalManagers.Instance.PlayerSpawnerController = this;
        }
    }

    /// <summary>
    /// When this object spawn, it will spawn all active player
    /// </summary>
    public override void Spawned()
    {
        if(Runner.IsServer)
        {
            foreach (var item in Runner.ActivePlayers)
            {
                SpawnPlayer(item);
            }
        }
    }

    private void SpawnPlayer(PlayerRef playerRef)
    {
        if(Runner.IsServer) //Just only server can spawn
        {
            int index = playerRef % spawnPoints.Length;
            var spawnPoint = spawnPoints[index].transform.position; 
            var playerObject = Runner.Spawn(playerNetworkPrefab, spawnPoint, Quaternion.identity, playerRef);

            Runner.SetPlayerObject(playerRef, playerObject);
        }
    }

    public Vector2 GetRandomSpawnPointPosition()
    {
        return spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)].transform.position;
    }

    /// <summary>
    /// Any player joined -> spawn new player
    /// </summary>
    /// <param name="player"></param>
    public void PlayerJoined(PlayerRef player)
    {
        Debug.Log($"[PlayerSpawnerController] player {player.PlayerId} Joined");
        SpawnPlayer(player);
    }

    public void PlayerLeft(PlayerRef player)
    {
        DespawnPlayer(player);
    }

    private void DespawnPlayer(PlayerRef playerRef)
    {
        if (Runner.IsServer)
        {
            if(Runner.TryGetPlayerObject(playerRef, out NetworkObject playerObject))
            {
                Runner.Despawn(playerObject);
            }
            Runner.SetPlayerObject(playerRef, null);
        }
    }
}
