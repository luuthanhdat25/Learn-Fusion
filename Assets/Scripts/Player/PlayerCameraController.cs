using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource impulseSource;
    
    public void ShakeCamera(Vector3 skakeAmount)
    {
        impulseSource.GenerateImpulse(skakeAmount);
    }
}
