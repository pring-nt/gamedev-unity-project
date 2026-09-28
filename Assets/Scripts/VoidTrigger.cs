using UnityEngine;

public class VoidTrigger : MonoBehaviour
{
    public Transform checkpoint;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;

        rb.linearVelocity = Vector3.zero;

        rb.transform.position = checkpoint.position;
        Physics.SyncTransforms();

        Debug.Log("Teleported player to checkpoint");
    }
}