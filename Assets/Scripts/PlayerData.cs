using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerData", menuName = "Game/Player Data")]
public class PlayerData : ScriptableObject
{
    public float maxHealth = 100f;
    public float minSpeedMultiplier = 0.25f;
    public float maxSpeedMultiplier = 2f;
}