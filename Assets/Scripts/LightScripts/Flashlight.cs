using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class Flashlight : MonoBehaviour
{
    public enum MouseButton { Left, Right, Middle }

    [Header("Light")]
    public Light spotLight;
    public MouseButton toggleButton = MouseButton.Left;
    public bool startOn;

    [Header("Detection")]
    public LayerMask obstacleMask = ~0;
    public float checkInterval = 0.05f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip toggleSound;

    public bool IsOn => spotLight.enabled;

    private float nextCheckTime;

    private void Start()
    {
        spotLight.enabled = startOn;
    }

    private void Update()
    {
        if (GetButton(toggleButton).wasPressedThisFrame)
            Toggle();

        if (Time.time < nextCheckTime) return;
        nextCheckTime = Time.time + checkInterval;

        CheckHiddenObjects();
    }

    public void Toggle()
    {
        spotLight.enabled = !spotLight.enabled;

        if (audioSource != null && toggleSound != null)
            audioSource.PlayOneShot(toggleSound);
    }

    private void CheckHiddenObjects()
    {
        // backwards so an object can remove itself from the list while we loop
        for (int i = HiddenObject.All.Count - 1; i >= 0; i--)
        {
            HiddenObject hidden = HiddenObject.All[i];
            hidden.SetLit(IsOn && hidden.IsLitBy(spotLight, obstacleMask));
        }
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
}