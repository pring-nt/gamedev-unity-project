using UnityEngine;

public class SpeedPad : MonoBehaviour
{
    public float multiplier = 1.5f;
    public float duration = 3f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerStats>() is PlayerStats stats)
            stats.ApplySpeedModifier(multiplier, duration);
    }
}