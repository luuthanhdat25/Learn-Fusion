using Fusion;
using UnityEngine;

public struct PlayerNetworkInput : INetworkInput
{
    public float HorizontalInput;
    public Quaternion GunPivotRotation;
    public NetworkButtons NetworkButtons;
}

public enum PlayerInputButtons
{
    None,
    Jump,
    Shoot
}