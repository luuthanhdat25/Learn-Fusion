using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Config/Game")]
public class GameConfig : ScriptableObject
{
    public float StartDelay;
    public int NumberPlayerToStart;
}
