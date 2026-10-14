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
    // presses this long before the cooldown ends still count
    public float inputBufferTime = 0.2f;

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

    [Header("Audio")]
    public AudioSource chargeAudio;

    private ThrowableAudio throwableAudio;
    private float lastPressTime = float.NegativeInfinity;

    public float ChargePercent => charging ? Mathf.Clamp01(chargeTimer / maxChargeTime) : 0f;

    private void Start()
    {
        readyToThrow = true;
        throwableAudio = objectToThrow.GetComponent<ThrowableAudio>();
    }

    private void Update()
    {
        ButtonControl button = GetButton(throwButton);

        if (button.wasPressedThisFrame)
            lastPressTime = Time.time;

        bool canThrow = readyToThrow && totalThrows > 0;
        bool pressBuffered = Time.time - lastPressTime <= inputBufferTime;

        if (!holdToCharge)
        {
            charging = false;

            if (pressBuffered && canThrow)
            {
                ConsumeBufferedPress();
                Throw(1f);
            }

            return;
        }

        if (!charging && canThrow)
        {
            // still holding a press made during the cooldown: start charging now
            if (button.isPressed)
            {
                ConsumeBufferedPress();
                StartCharging();
            }
            // quick tap during the cooldown: throw at minimum charge once ready
            else if (pressBuffered)
            {
                ConsumeBufferedPress();
                Throw(minChargeMultiplier);
            }
        }

        if (!charging) return;

        chargeTimer += Time.deltaTime;
        UpdateChargeSound();

        if (button.wasReleasedThisFrame || !button.isPressed)
        {
            charging = false;
            StopChargeSound();
            Throw(Mathf.Lerp(minChargeMultiplier, maxChargeMultiplier, Mathf.Clamp01(chargeTimer / maxChargeTime)));
        }
    }

    private void LateUpdate()
    {
        playerCamera.SetZoom(ChargePercent * maxChargeZoom);
    }

    private void ConsumeBufferedPress()
    {
        lastPressTime = float.NegativeInfinity;
    }

    private void StartCharging()
    {
        charging = true;
        chargeTimer = 0f;
        StartChargeSound();
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

        if (!projectile.TryGetComponent(out ProjectileLifetime _))
            projectile.AddComponent<ProjectileLifetime>();

        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();

        foreach (Collider projectileCol in projectile.GetComponentsInChildren<Collider>())
            foreach (Collider playerCol in GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(projectileCol, playerCol);

        Vector3 forceToAdd = (forceDirection * throwForce + transform.up * throwUpwardForce) * forceMultiplier;
        projectileRb.AddForce(forceToAdd, ForceMode.Impulse);

        if (chargeAudio != null && throwableAudio != null && throwableAudio.throwSound != null)
            chargeAudio.PlayOneShot(throwableAudio.throwSound);

        totalThrows--;

        Invoke(nameof(ResetThrow), throwCooldown);
    }

    private void ResetThrow()
    {
        readyToThrow = true;
    }

    private void StartChargeSound()
    {
        if (chargeAudio == null || throwableAudio == null || throwableAudio.chargeSound == null) return;

        chargeAudio.clip = throwableAudio.chargeSound;
        chargeAudio.pitch = throwableAudio.minPitch;
        chargeAudio.Play();
    }

    private void UpdateChargeSound()
    {
        if (chargeAudio == null || !chargeAudio.isPlaying || throwableAudio == null) return;

        chargeAudio.pitch = Mathf.Lerp(throwableAudio.minPitch, throwableAudio.maxPitch, ChargePercent);
    }

    private void StopChargeSound()
    {
        if (chargeAudio == null) return;

        chargeAudio.Stop();
        chargeAudio.pitch = 1f;
    }
}