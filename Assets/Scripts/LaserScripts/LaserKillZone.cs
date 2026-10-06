using UnityEngine;

public class LaserKillZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Laser laser = other.GetComponentInParent<Laser>();

        // only kill moving lasers
        if (laser != null && laser.isMoving)
            Destroy(laser.gameObject);
    }
}