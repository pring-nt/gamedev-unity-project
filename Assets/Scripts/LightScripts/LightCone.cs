using UnityEngine;

public static class LightCone
{
    public static bool IsLit(Light light, Vector3 point, LayerMask obstacleMask, Transform target = null)
    {
        if (light == null || !light.enabled || !light.gameObject.activeInHierarchy) return false;

        Transform origin = light.transform;
        Vector3 toPoint = point - origin.position;
        float distance = toPoint.magnitude;

        if (distance < 0.001f || distance > light.range) return false;
        if (Vector3.Angle(origin.forward, toPoint) > light.spotAngle * 0.5f) return false;

        if (!Physics.Raycast(origin.position, toPoint / distance, out RaycastHit hit, distance,
                obstacleMask, QueryTriggerInteraction.Ignore))
            return true;

        // the first thing in the way is the target itself, so nothing blocks it
        return target != null && hit.transform.IsChildOf(target);
    }
}