using UnityEngine;

public class MovePlatformNegX : MonoBehaviour
{
    public Transform platform;

    public float moveSpeed = 5f;          // units per second
    public float maxUnitsLeft = 11.5f;    // how far left of the start it can shift

    float startX;
    bool playerInside;

    private void Start()
    {
        startX = platform.position.x;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) playerInside = false;
    }

    private void Update()
    {
        if (!playerInside) return;

        float leftX = startX - maxUnitsLeft;

        Vector3 pos = platform.position;
        pos.x = Mathf.MoveTowards(pos.x, leftX, moveSpeed * Time.deltaTime);
        platform.position = pos;
    }
}