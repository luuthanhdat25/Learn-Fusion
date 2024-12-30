using Fusion;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerSpawnerController : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    [SerializeField] private NetworkPrefabRef playerNetworkPrefab = NetworkPrefabRef.Empty;
    [SerializeField] private Transform[] spawnPoints;

    private Dictionary<PlayerRef, NetworkObject> spawnedPlayers = new Dictionary<PlayerRef, NetworkObject>();

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
                spawnedPlayers.Remove(playerRef);
                Runner.Despawn(playerObject);
            }
            Runner.SetPlayerObject(playerRef, null);
        }
    }

    public void AddEntry(PlayerRef playerRef, NetworkObject networkObject)
    {
        if (spawnedPlayers.ContainsKey(playerRef) 
            || networkObject == null) return;

        spawnedPlayers.Add(playerRef, networkObject);
    }

    /*public void HideAndTeleportAllPlayerToStartPosition()
    {
        if (!Object.HasStateAuthority) return;
        foreach (var item in spawnedPlayers)
        {
            PlayerController playerController = item.Value.GetComponent<PlayerController>();
            if (playerController != null) 
            {
                playerController.HidePlayer();
            }
        }
    }*/

    public void RespawnAllPlayerToStartGame()
    {
        if (!Object.HasStateAuthority) return;
        foreach (var item in spawnedPlayers)
        {
            PlayerController playerController = item.Value.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TeleportToPosition(GetSpawnPointByPlayerIndex(item.Key));
                playerController.PlayerWeaponController.Rpc_ActiveWeapon();
            }
        }
    }
}
