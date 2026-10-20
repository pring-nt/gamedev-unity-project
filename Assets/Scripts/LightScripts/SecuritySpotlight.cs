using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class SecuritySpotlight : MonoBehaviour
{
    public enum MotionType { None, Sweep, Slide }

    [Header("References")]
    public Light spotLight;
    public Transform player;
    public LaserSpawner[] spawners;

    [Header("Detection")]
    public LayerMask obstacleMask = ~0;
    // checks the player's head, center and feet so being half in the light still counts
    public float playerHalfHeight = 0.8f;
    public float checkInterval = 0.05f;

    [Header("Alarm")]
    public float alarmDuration = 5f;
    public float retriggerCooldown = 1f;
    public Color alarmColor = Color.red;
    public AudioSource alarmAudio;
    public AudioClip alarmSound;
    public UnityEvent onPlayerSpotted;

    [Header("Motion")]
    public MotionType motion = MotionType.Slide;
    public float motionDuration = 3f;
    public Ease motionEase = Ease.InOutSine;

    [Header("Sweep")]
    public float sweepAngle = 40f;

    [Header("Slide")]
    // in the parent's space; a negative value moves the other way first
    public Vector3 slideOffset = new Vector3(4f, 0f, 0f);

    private Color normalColor;
    private float nextCheckTime;
    private float nextAlarmTime;
    private Tween motionTween;

    private void Start()
    {
        normalColor = spotLight.color;

        if (player == null && FindAnyObjectByType<PlayerStats>() is PlayerStats stats)
            player = stats.transform;

        StartMotion();
    }

    private void StartMotion()
    {
        switch (motion)
        {
            case MotionType.Sweep:
                Quaternion center = transform.localRotation;
                transform.localRotation = center * Quaternion.Euler(0f, -sweepAngle, 0f);

                motionTween = transform.DOLocalRotateQuaternion(center * Quaternion.Euler(0f, sweepAngle, 0f), motionDuration)
                    .SetEase(motionEase)
                    .SetLoops(-1, LoopType.Yoyo);
                break;

            case MotionType.Slide:
                motionTween = transform.DOLocalMove(transform.localPosition + slideOffset, motionDuration)
                    .SetEase(motionEase)
                    .SetLoops(-1, LoopType.Yoyo);
                break;
        }
    }

    private void Update()
    {
        if (player == null || Time.time < nextCheckTime) return;
        nextCheckTime = Time.time + checkInterval;

        if (PlayerInLight() && Time.time >= nextAlarmTime)
            TriggerAlarm();
    }

    private bool PlayerInLight()
    {
        Vector3 center = player.position;
        Vector3 up = Vector3.up * playerHalfHeight;

        return LightCone.IsLit(spotLight, center, obstacleMask, player)
            || LightCone.IsLit(spotLight, center + up, obstacleMask, player)
            || LightCone.IsLit(spotLight, center - up, obstacleMask, player);
    }

    private void TriggerAlarm()
    {
        nextAlarmTime = Time.time + retriggerCooldown;

        foreach (LaserSpawner spawner in spawners)
            if (spawner != null) spawner.ActivateFor(alarmDuration);

        if (alarmAudio != null && alarmSound != null && !alarmAudio.isPlaying)
            alarmAudio.PlayOneShot(alarmSound);

        spotLight.color = alarmColor;
        CancelInvoke(nameof(ResetColor));
        Invoke(nameof(ResetColor), alarmDuration);

        onPlayerSpotted?.Invoke();
    }

    private void ResetColor()
    {
        spotLight.color = normalColor;
    }

    private void OnDestroy()
    {
        motionTween?.Kill();
    }
}