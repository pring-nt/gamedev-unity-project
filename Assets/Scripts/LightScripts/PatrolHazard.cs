using DG.Tweening;
using UnityEngine;

public class PatrolHazard : MonoBehaviour
{
    public enum Shape { Sphere, Box, Capsule }

    [Header("Detection")]
    public Shape shape = Shape.Sphere;
    public LayerMask playerMask;
    public float radius = 0.5f;
    public Vector3 boxHalfExtents = new Vector3(0.5f, 0.5f, 0.5f);
    public float capsuleHeight = 2f;

    [Header("Damage")]
    public bool instantKill;
    public float damage = 25f;
    public float hitCooldown = 1f;

    [Header("Patrol")]
    public Vector3 moveOffset = new Vector3(4f, 0f, 0f);
    public float moveDuration = 2f;
    public Ease moveEase = Ease.InOutSine;

    private PlayerStats playerStats;
    private float nextHitTime;
    private Tween moveTween;

    private void Start()
    {
        playerStats = GameManager.Instance.player;

        moveTween = transform.DOLocalMove(transform.localPosition + moveOffset, moveDuration)
            .SetEase(moveEase)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void FixedUpdate()
    {
        if (playerStats == null) { Debug.LogWarning($"{name}: no player reference"); return; }
        if (Time.time < nextHitTime) return;

        bool inside = PlayerInside();
        if (inside) Debug.Log($"{name}: hit player");

        if (!inside) return;

        nextHitTime = Time.time + hitCooldown;
        playerStats.TakeDamage(instantKill ? playerStats.CurrentHealth : damage);
    }

    private bool PlayerInside()
    {
        Vector3 center = transform.position;

        switch (shape)
        {
            case Shape.Box:
                return Physics.CheckBox(center, boxHalfExtents, transform.rotation, playerMask, QueryTriggerInteraction.Ignore);

            case Shape.Capsule:
                Vector3 offset = transform.up * Mathf.Max(0f, capsuleHeight * 0.5f - radius);
                return Physics.CheckCapsule(center - offset, center + offset, radius, playerMask, QueryTriggerInteraction.Ignore);

            default:
                return Physics.CheckSphere(center, radius, playerMask, QueryTriggerInteraction.Ignore);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        switch (shape)
        {
            case Shape.Box:
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, boxHalfExtents * 2f);
                break;

            case Shape.Capsule:
                Vector3 offset = transform.up * Mathf.Max(0f, capsuleHeight * 0.5f - radius);
                Gizmos.DrawWireSphere(transform.position + offset, radius);
                Gizmos.DrawWireSphere(transform.position - offset, radius);
                break;

            default:
                Gizmos.DrawWireSphere(transform.position, radius);
                break;
        }
    }

    private void OnDestroy()
    {
        moveTween?.Kill();
    }
}