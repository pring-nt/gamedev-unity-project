using UnityEngine;

public class LevelCompleteTrigger : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private AudioClip soundEffect;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        print("YIPPEEE YOU WINN, Level Complete");
        PlaySFX();
    }

    public void PlaySFX()
    {
        audioSource.PlayOneShot(soundEffect);
    }
}
