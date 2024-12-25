using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnerController : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    [SerializeField] private NetworkPrefabRef playerNetworkPrefab = NetworkPrefabRef.Empty;
    [SerializeField] private Transform[] spawnPoints;

    private Dictionary<PlayerRef, NetworkObject> currentSpawnedPlayers = new Dictionary<PlayerRef, NetworkObject>();

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
        if (Runner.IsServer && !Runner.TryGetPlayerObject(playerRef, out _))
        {
            int index = playerRef.AsIndex % spawnPoints.Length;
            var spawnPoint = spawnPoints[index].position;
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
            if(currentSpawnedPlayers.TryGetValue(playerRef, out NetworkObject playerObject))
            {
                currentSpawnedPlayers.Remove(playerRef);
                Runner.Despawn(playerObject);
            }
            Runner.SetPlayerObject(playerRef, null);
        }
    }

    public void AddToEntry(PlayerRef player, NetworkObject obj)
    {
        currentSpawnedPlayers.TryAdd(player, obj);
    }
}
