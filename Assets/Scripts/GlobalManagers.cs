using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalManagers : MonoBehaviour
{
    public static GlobalManagers Instance { get; private set; }
    
    [field: SerializeField] public NetworkRunnerController NetworkRunnerController;
    [SerializeField] private GameObject parentObject;

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
