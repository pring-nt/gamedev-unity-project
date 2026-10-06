using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DamageFeedback : MonoBehaviour
{
    [Header("Hurt Sound")]
    [SerializeField] private AudioClip soundEffect;
    private AudioSource audioSource;

    [Header("Camera Kick")]
    public PlayerCamera playerCamera;
    public float tiltStrength = 6f;
    public float tiltDuration = 0.25f;

    [Header("Overlay")]
    public Image damageOverlay;
    public float overlayMaxAlpha = 0.35f;
    public float overlayFadeTime = 0.4f;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void OnDamaged(float amount)
    {
        float side = Random.value < 0.5f ? -1f : 1f;
        playerCamera.DoDamageKick(tiltStrength * side, tiltDuration);

        damageOverlay.DOKill();

        Color color = damageOverlay.color;
        color.a = overlayMaxAlpha;
        damageOverlay.color = color;

        damageOverlay.DOFade(0f, overlayFadeTime);
        PlaySFX();
    }

    public void PlaySFX()
    {
        if (soundEffect == null) return;

        audioSource.pitch = Random.Range(0.8f, 1.2f);
        audioSource.PlayOneShot(soundEffect);
    }
}