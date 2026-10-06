using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class PlayerCamera : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform orientation;
    public Transform camHolder;

    private float xRotation;
    private float yRotation;

    private Camera cam;

    private float tilt;
    private float damageRoll;
    private Tween tiltTween;
    private Tween damageTween;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = mouseDelta.x * sensX * 0.01f;
        float mouseY = mouseDelta.y * sensY * 0.01f;

        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    private void LateUpdate()
    {
        transform.localRotation = Quaternion.Euler(0f, 0f, tilt + damageRoll);
    }

    public void DoFov(float endValue)
    {
        cam.DOFieldOfView(endValue, 0.25f);
    }

    public void DoTilt(float zTilt)
    {
        tiltTween?.Kill();
        tiltTween = DOTween.To(() => tilt, x => tilt = x, zTilt, 0.25f);
    }

    public void DoDamageKick(float strength, float duration)
    {
        damageTween?.Kill();
        damageRoll = 0f;

        damageTween = DOTween.Punch(
            () => new Vector3(0f, 0f, damageRoll),
            v => damageRoll = v.z,
            new Vector3(0f, 0f, strength),
            duration, 8, 0.5f);
    }
}