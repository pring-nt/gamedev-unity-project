using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class ThrowObject : MonoBehaviour
{
    public enum MouseButton { Left, Right, Middle }

    [Header("References")]
    public Transform cam;
    public Transform attackPoint;
    public GameObject objectToThrow;

    [Header("Charge Zoom")]
    public PlayerCamera playerCamera;
    public float maxChargeZoom = 10f;

    [Header("Settings")]
    public int totalThrows;
    public float throwCooldown;

    [Header("Throwing")]
    public MouseButton throwButton = MouseButton.Left;
    public float throwForce;
    public float throwUpwardForce;
    private bool readyToThrow;

    [Header("Throw Mode")]
    public bool holdToCharge;
    public float minChargeMultiplier = 0.5f;
    public float maxChargeMultiplier = 2f;
    public float maxChargeTime = 1f;
    private bool charging;
    private float chargeTimer;

    [Header("Aiming")]
    public LayerMask aimMask = ~0;
    public float minAimDistance = 2f;

    public float ChargePercent => charging ? Mathf.Clamp01(chargeTimer / maxChargeTime) : 0f;

    private void Start()
    {
        readyToThrow = true;
    }

    private void Update()
    {
        ButtonControl button = GetButton(throwButton);
        bool canThrow = readyToThrow && totalThrows > 0;

        if (!holdToCharge)
        {
            charging = false;

            if (button.wasPressedThisFrame && canThrow)
                Throw(1f);

            return;
        }

        if (button.wasPressedThisFrame && canThrow)
        {
            charging = true;
            chargeTimer = 0f;
        }

        if (!charging) return;

        chargeTimer += Time.deltaTime;

        if (button.wasReleasedThisFrame)
        {
            charging = false;
            Throw(Mathf.Lerp(minChargeMultiplier, maxChargeMultiplier, Mathf.Clamp01(chargeTimer / maxChargeTime)));
        }
    }

    private void LateUpdate()
    {
        playerCamera.SetZoom(ChargePercent * maxChargeZoom);
    }

    private ButtonControl GetButton(MouseButton button)
    {
        return button switch
        {
            MouseButton.Right => Mouse.current.rightButton,
            MouseButton.Middle => Mouse.current.middleButton,
            _ => Mouse.current.leftButton,
        };
    }

    private void Throw(float forceMultiplier)
    {
        readyToThrow = false;

        // fall back to a far point straight ahead when nothing valid is under the crosshair
        Vector3 aimPoint = cam.position + cam.forward * 500f;

        if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, 500f, aimMask, QueryTriggerInteraction.Ignore)
            && hit.distance > minAimDistance)
            aimPoint = hit.point;

        Vector3 forceDirection = (aimPoint - attackPoint.position).normalized;

        GameObject projectile = Instantiate(objectToThrow, attackPoint.position, cam.rotation);
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();

        foreach (Collider projectileCol in projectile.GetComponentsInChildren<Collider>())
            foreach (Collider playerCol in GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(projectileCol, playerCol);

        Vector3 forceToAdd = (forceDirection * throwForce + transform.up * throwUpwardForce) * forceMultiplier;
        projectileRb.AddForce(forceToAdd, ForceMode.Impulse);

        totalThrows--;

        Invoke(nameof(ResetThrow), throwCooldown);
    }

    private void ResetThrow()
    {
        readyToThrow = true;
    }
}