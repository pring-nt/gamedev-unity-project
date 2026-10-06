using UnityEngine;

[CreateAssetMenu(fileName = "NewLaserData", menuName = "Game/Laser Data")]
public class LaserData : ScriptableObject
{
    public float damage = 10f;
    // seconds between hits while touching; 0 = only hit once on contact
    public float tickInterval = 0.5f;
    public float speed = 8f;
}