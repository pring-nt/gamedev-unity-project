using UnityEngine;

public static class LightCone
{
    public static bool IsLit(Light light, Vector3 point, LayerMask obstacleMask)
    {
        if (light == null || !light.enabled || !light.gameObject.activeInHierarchy) return false;

        Transform origin = light.transform;
        Vector3 toPoint = point - origin.position;
        float distance = toPoint.magnitude;

        if (distance < 0.001f || distance > light.range) return false;
        if (Vector3.Angle(origin.forward, toPoint) > light.spotAngle * 0.5f) return false;

        // stop just short of the point so the target's own collider doesn't count as blocking
        return !Physics.Raycast(origin.position, toPoint / distance, distance - 0.05f,
            obstacleMask, QueryTriggerInteraction.Ignore);
    }
}