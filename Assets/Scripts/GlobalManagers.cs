using UnityEngine;

public class GlobalManagers : MonoBehaviour
{
    public static GlobalManagers Instance { get; private set; }
    
    [field: SerializeField] public NetworkRunnerController NetworkRunnerController;
    [SerializeField] private GameObject parentObject;
    public PlayerSpawnerController PlayerSpawnerController { get; set; }
    public ObjectPoolingManager ObjectPoolingManager { get; set; }
    public GameManager GameManager { get; set; }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(parentObject);
        }
    }
}
