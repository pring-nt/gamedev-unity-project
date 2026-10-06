using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    public LaserData data;
    public bool isMoving;

    private readonly Dictionary<IDamageable, float> nextTickTime = new Dictionary<IDamageable, float>();

    private void Update()
    {
        if (isMoving)
            transform.Translate(Vector3.back * data.speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<IDamageable>() is not IDamageable target) return;
        if (nextTickTime.ContainsKey(target)) return;

        target.TakeDamage(data.damage);
        nextTickTime[target] = Time.time + data.tickInterval;
    }

    private void OnTriggerStay(Collider other)
    {
        if (data.tickInterval <= 0f) return;
        if (other.GetComponentInParent<IDamageable>() is not IDamageable target) return;
        if (!nextTickTime.TryGetValue(target, out float next) || Time.time < next) return;

        target.TakeDamage(data.damage);
        nextTickTime[target] = Time.time + data.tickInterval;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<IDamageable>() is IDamageable target)
            nextTickTime.Remove(target);
    }
}