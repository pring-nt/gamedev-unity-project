using UnityEngine;

public class MovePlatformPosX : MonoBehaviour
{
    public Transform platform;

    public float moveSpeed = 5f;          // units per second
    public float maxUnitsRight = 11.5f;   // how far right of the start it can shift

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

        Vector3 pos = platform.position;
        pos.x = Mathf.MoveTowards(pos.x, startX + maxUnitsRight, moveSpeed * Time.deltaTime);
        platform.position = pos;
    }
}