using System.Collections.Generic;
using Fusion;
using System.Linq;

public class ObjectPoolingManager : NetworkObjectProviderDefault
{
    private Dictionary<NetworkObject, List<NetworkObject>> prefabsThatHadBeenInstantiated = new();

    private void Start()
    {
        if(GlobalManagers.Instance != null)
        {
            GlobalManagers.Instance.ObjectPoolingManager = this;
        }
    }

    protected override NetworkObject InstantiatePrefab(NetworkRunner runner, NetworkObject prefab)
     {
         if (prefabsThatHadBeenInstantiated.TryGetValue(prefab, out var networkObjects))
         {
             var inactiveObject = networkObjects?.FirstOrDefault(item => item != null && !item.gameObject.activeSelf);
             if (inactiveObject != null)
                 return inactiveObject;
         }
         else
         {
             networkObjects = new List<NetworkObject>();
             prefabsThatHadBeenInstantiated[prefab] = networkObjects;
         }

         var newObj = Instantiate(prefab);
         networkObjects.Add(newObj);
         return newObj;
     }

     protected override void DestroyPrefabInstance(NetworkRunner runner, NetworkPrefabId prefabId, NetworkObject instance)
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
