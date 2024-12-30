using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "Config/Player")]
public class PlayerConfig : ScriptableObject
{
    public float MoveSpeed;
    public float JumpForce;
    public float RespawnTime;
}
