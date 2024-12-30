using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnerController : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    [SerializeField] private NetworkPrefabRef playerNetworkPrefab = NetworkPrefabRef.Empty;
    [SerializeField] private Transform[] spawnPoints;

    private Dictionary<PlayerRef, NetworkObject> spawnedPlayers = new Dictionary<PlayerRef, NetworkObject>();
    [Networked, HideInInspector, OnChangedRender(nameof(OnTotalPlayerAliveChange))] public int TotalPlayerAlive { get; private set; }

    private void OnTotalPlayerAliveChange()
    {
        if(TotalPlayerAlive == 1 && Runner.IsServer)
        {
            GlobalManagers.Instance.GameManager.EndGame();
        }
    }

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
            var playerObject = Runner.Spawn(playerNetworkPrefab, GetSpawnPointByPlayerIndex(playerRef), Quaternion.identity, playerRef);
            Runner.SetPlayerObject(playerRef, playerObject);
        }
    }

    public Vector2 GetRandomSpawnPointPosition()
    {
        return spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)].transform.position;
    }

    public Vector3 GetSpawnPointByPlayerIndex(PlayerRef playerRef)
    {
        int index = playerRef.AsIndex % spawnPoints.Length;
        return spawnPoints[index].position;
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
            if(spawnedPlayers.TryGetValue(playerRef, out NetworkObject playerObject))
            {
                if(playerObject.TryGetComponent<PlayerController>(out var playerController))
                {
                    if (playerController.IsPlayerAlive) //When player still alive and out room
                    {
                        Debug.Log($"[{nameof(PlayerSpawnerController)}] Player [{GlobalManagers.Instance.PlayerData.GetNickName()}] is out Room");
                        TotalPlayerAlive--;
                    } 
                }
                spawnedPlayers.Remove(playerRef);
                Runner.Despawn(playerObject);
            }
            Runner.SetPlayerObject(playerRef, null);
        }
    }

    public void PlayerDead(PlayerRef playerRef)
    {
        if (spawnedPlayers.TryGetValue(playerRef, out NetworkObject playerObject))
        {
            TotalPlayerAlive--;
            Debug.Log($"[{nameof(PlayerSpawnerController)}] Player [{GlobalManagers.Instance.PlayerData.GetNickName()}] is dead");
        }
    }

    public void AddEntry(PlayerRef playerRef, NetworkObject networkObject)
    {
        if (spawnedPlayers.ContainsKey(playerRef) 
            || networkObject == null) return;

        spawnedPlayers.Add(playerRef, networkObject);
    }

    public void RespawnAllPlayerToStartGame()
    {
        if (!Object.HasStateAuthority) return;

        int numberOfPlayer = 0;
        foreach (var item in spawnedPlayers)
        {
            PlayerController playerController = item.Value.GetComponent<PlayerController>();
            if (playerController != null)
            {
                numberOfPlayer++;
                playerController.TeleportToPosition(GetSpawnPointByPlayerIndex(item.Key));
                playerController.PlayerWeaponController.Rpc_ActiveWeapon();
            }
        }

        TotalPlayerAlive = numberOfPlayer;
    }

    public PlayerController GetFinalPlayerAlive()
    {
        foreach (var item in spawnedPlayers)
        {
            PlayerController playerController = item.Value.GetComponent<PlayerController>();
            if (playerController != null && playerController.IsPlayerAlive)
            {
                return playerController;
            }
        }
        return null;
    }
}
