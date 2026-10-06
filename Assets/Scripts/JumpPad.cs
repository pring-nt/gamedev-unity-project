using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public float launchForce = 30f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerMovement>() is PlayerMovement pm)
            pm.Launch(launchForce);
    }
}