using System.Collections.Generic;
using UnityEngine;

public class ExplosiveProjectile : MonoBehaviour
{
    [Header("Detonation")]
    public bool explodeOnImpact = true;
    public float fuseTime = 3f;
    public float armTime = 0.15f;

    private float spawnTime;
    private bool Armed => Time.time - spawnTime >= armTime;

    [Header("Explosion")]
    public float radius = 5f;
    public float force = 15f;
    public float upwardsModifier = 1f;
    public float damage = 25f;
    public LayerMask affectedLayers = ~0;

    [Header("Effects")]
    public GameObject explosionEffect;
    public float effectLifetime = 3f;
    public AudioClip explosionSound;

    private bool exploded;

    private void Start()
    {
        spawnTime = Time.time;

        if (!explodeOnImpact)
            Invoke(nameof(Explode), fuseTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (explodeOnImpact && Armed)
            Explode();
    }

    private void OnCollisionStay(Collision collision)
    {
        if (explodeOnImpact && Armed)
            Explode();
    }

    public void Explode()
    {
        if (exploded) return;
        exploded = true;

        Vector3 center = transform.position;

        if (explosionEffect != null)
            Destroy(Instantiate(explosionEffect, center, Quaternion.identity), effectLifetime);

        if (explosionSound != null)
            AudioSource.PlayClipAtPoint(explosionSound, center);

        // objects can have several colliders, so track what's been hit to push and damage each one once
        HashSet<Rigidbody> pushedBodies = new HashSet<Rigidbody>();
        HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();

        foreach (Collider col in Physics.OverlapSphere(center, radius, affectedLayers, QueryTriggerInteraction.Ignore))
        {
            Rigidbody body = col.attachedRigidbody;

            if (body != null && body.gameObject != gameObject && pushedBodies.Add(body))
                body.AddExplosionForce(force, center, radius, upwardsModifier, ForceMode.Impulse);

            if (damage > 0f && col.GetComponentInParent<IDamageable>() is IDamageable target && damagedTargets.Add(target))
            {
                float distance = Vector3.Distance(center, col.ClosestPoint(center));
                float falloff = 1f - Mathf.Clamp01(distance / radius);
                target.TakeDamage(damage * falloff);
            }
        }

        Destroy(gameObject);
    }
}