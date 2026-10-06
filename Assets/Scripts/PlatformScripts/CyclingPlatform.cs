using UnityEngine;

public class CyclingPlatform : MonoBehaviour
{
    public Rigidbody platformRb;        // kinematic Rigidbody on the empty parent

    public float moveSpeed = 2f;        // units per second
    public float distance = 10f;        // how far it travels from its placed position
    public float pauseAtEnds = 1f;      // seconds to wait at each end
    public float startDelay = 0f;       // offsets this platform from others
    public bool reverse = false;        // false = goes up first, true = goes down first

    float startY;
    float endY;
    bool movingToEnd = true;
    float pauseTimer;

    private void Start()
    {
        startY = platformRb.position.y;
        endY = reverse ? startY - distance : startY + distance;
        pauseTimer = startDelay;
    }

    private void FixedUpdate()
    {
        // waiting (at an end, or during the initial delay)
        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.fixedDeltaTime;
            return;
        }

        float target = movingToEnd ? endY : startY;

        Vector3 pos = platformRb.position;
        pos.y = Mathf.MoveTowards(pos.y, target, moveSpeed * Time.fixedDeltaTime);
        platformRb.MovePosition(pos);

        // reached the target: flip direction and wait
        if (Mathf.Approximately(pos.y, target))
        {
            movingToEnd = !movingToEnd;
            pauseTimer = pauseAtEnds;
        }
    }
}