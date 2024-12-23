using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;

public class ObjectPoolingManager : MonoBehaviour, INetworkObjectPool
{
    private Dictionary<NetworkObject, List<NetworkObject>> prefabsThatHadBeenInstantiated = new();

    private void Start()
    {
        if(GlobalManagers.Instance != null)
        {
            GlobalManagers.Instance.ObjectPoolingManager = this;
        }
    }

    public NetworkObject AcquireInstance(NetworkRunner runner, NetworkPrefabInfo info)
    {
        NetworkProjectConfig.Global.PrefabTable.TryGetPrefab(info.Prefab, out var prefab);

        if (prefabsThatHadBeenInstantiated.TryGetValue(prefab, out var networkObjects))
        {
            var inactiveObject = networkObjects?.FirstOrDefault(item => item != null && !item.gameObject.activeSelf);
            if (inactiveObject != null) return inactiveObject;
        }
        else
        {
            networkObjects = new List<NetworkObject>();
            prefabsThatHadBeenInstantiated[prefab] = networkObjects;
        }

        var newObject = Instantiate(prefab);
        networkObjects.Add(newObject);

        return newObject;
    }

    public void ReleaseInstance(NetworkRunner runner, NetworkObject instance, bool isSceneObject)
    {
        instance.gameObject.SetActive(false);
    }

    public void RemoveNetworkObjectFromDic(NetworkObject obj)
    {
        if(prefabsThatHadBeenInstantiated.Count > 0)
        {
            foreach (var item in prefabsThatHadBeenInstantiated)
            {
                var target = item.Value.FirstOrDefault(i => i == obj);
                if (target != null)
                {
                    item.Value.Remove(target);
                    break;
                }
            }
        }
    }
}
