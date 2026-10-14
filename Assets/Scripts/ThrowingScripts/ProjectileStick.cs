using Unity.VisualScripting;
using UnityEngine;

public class ProjectileStick : MonoBehaviour
{
    private Rigidbody rb;
    private Collider col;
    private bool targetHit;

    [Header("Effects")]
    public AudioClip hitSound;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponentInChildren<Collider>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (targetHit) return;
        targetHit = true;

        if (collision.rigidbody != null)
            StickToRigidbody(collision.rigidbody);
        else
            StickToStatic(collision.transform);

        if (hitSound != null)
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
    }

    private void StickToRigidbody(Rigidbody target)
    {
        col.enabled = false;
        Destroy(rb);

        transform.SetParent(target.transform, true);
    }

    private void StickToStatic(Transform target)
    {
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.isKinematic = true;

        Vector3 scale = target.lossyScale;
        if (Mathf.Approximately(scale.x, scale.y) && Mathf.Approximately(scale.y, scale.z))
            transform.SetParent(target);
    }
}