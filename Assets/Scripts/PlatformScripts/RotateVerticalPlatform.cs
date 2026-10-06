using UnityEngine;

public class RotateVerticalPlatform : MonoBehaviour
{
    public Transform platform;

    public float rotateSpeed = 60f;   // degrees per second
    public float maxAngle = 360f;
    public bool clampAtMax = false;   // true = stop at maxAngle, false = wrap back to 0

    private float currentAngle;
    private Vector3 startEuler;
    private bool playerInside;

    private void Start()
    {
        startEuler = platform.eulerAngles;
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

        float newAngle = currentAngle + rotateSpeed * Time.deltaTime;

        if (clampAtMax)
            currentAngle = Mathf.Clamp(newAngle, 0f, maxAngle);
        else
            currentAngle = Mathf.Repeat(newAngle, maxAngle);

        platform.eulerAngles = new Vector3(startEuler.x + currentAngle, startEuler.y, startEuler.z);
    }
}