using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private Camera camera;

    public override void Spawned()
    {
        camera.gameObject.SetActive(false);
    }
}
