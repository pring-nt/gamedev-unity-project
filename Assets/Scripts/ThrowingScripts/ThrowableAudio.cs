using UnityEngine;

public class ThrowableAudio : MonoBehaviour
{
    [Header("Throw Audio")]
    public AudioClip chargeSound;
    public AudioClip throwSound;

    [Header("Charge Pitch")]
    public float minPitch = 0.8f;
    public float maxPitch = 1.4f;
}