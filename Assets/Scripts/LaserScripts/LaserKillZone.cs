using UnityEngine;

public class LaserKillZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Laser laser = other.GetComponentInParent<Laser>();

        // only moving lasers, so hand-placed static ones overlapping the zone survive
        if (laser != null && laser.isMoving)
            Destroy(laser.gameObject);
    }
}