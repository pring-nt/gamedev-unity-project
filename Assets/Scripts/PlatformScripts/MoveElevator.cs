using UnityEngine;

public class MovePlatformUp : MonoBehaviour
{
    public Rigidbody platformRb;      // the kinematic Rigidbody on the empty parent

    public float moveSpeed = 2f;      // units per second
    public float maxHeight = 10f;     // how far above the start it can rise
    public float returnDelay = 2f;    // seconds to wait after the player leaves

    float startY;
    float timeSinceLeft;
    bool playerInside;

    private void Start()
    {
        startY = platformRb.position.y;
        timeSinceLeft = returnDelay;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;
        timeSinceLeft = 0f;
    }

    private void FixedUpdate()
    {
        if (!playerInside)
            timeSinceLeft += Time.fixedDeltaTime;

        float topY = startY + maxHeight;

        Vector3 pos = platformRb.position;
        bool atTop = topY - pos.y <= 0.01f;
        float target = pos.y;

        if (playerInside) target = startY + maxHeight;
        else if (timeSinceLeft >= returnDelay && pos.y == topY) target = startY;
        else if (pos.y < topY) target = startY;

        pos.y = Mathf.MoveTowards(pos.y, target, moveSpeed * Time.fixedDeltaTime);
        platformRb.MovePosition(pos);
    }
}