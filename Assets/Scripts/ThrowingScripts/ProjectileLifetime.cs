using DG.Tweening;
using UnityEngine;

public class ProjectileLifetime : MonoBehaviour
{
    public float lifetime = 10f;
    public float shrinkTime = 0.3f;

    [Tooltip("Explosives go off when their time runs out instead of just disappearing")]
    public bool explodeOnExpire = true;

    private bool expiring;

    private void Start()
    {
        Invoke(nameof(Expire), lifetime);
    }

    private void Expire()
    {
        if (expiring) return;
        expiring = true;

        if (explodeOnExpire && TryGetComponent(out ExplosiveProjectile explosive))
        {
            explosive.Explode();
            return;
        }

        DetachTrail();

        transform.DOScale(Vector3.zero, shrinkTime)
            .SetEase(Ease.InBack)
            .OnComplete(() => Destroy(gameObject));
    }

    private void DetachTrail()
    {
        TrailRenderer trail = GetComponentInChildren<TrailRenderer>();
        if (trail == null) return;

        trail.transform.SetParent(null);
        trail.emitting = false;
        Destroy(trail.gameObject, trail.time);
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}