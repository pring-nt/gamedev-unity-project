using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class HiddenObject : MonoBehaviour
{
    public static readonly List<HiddenObject> All = new List<HiddenObject>();

    [Header("Reveal")]
    // stays visible once found, otherwise it disappears again when the light moves off it
    public bool stayRevealed = true;
    public float popTime = 0.3f;
    public AudioClip revealSound;
    public UnityEvent onRevealed;

    private Renderer[] renderers;
    private Collider[] colliders;
    private Vector3 localCheckPoint;
    private Vector3 originalScale;
    private bool revealed;

    public Vector3 CheckPoint => transform.TransformPoint(localCheckPoint);

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        colliders = GetComponentsInChildren<Collider>(true);
        originalScale = transform.localScale;

        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        foreach (Renderer r in renderers)
            bounds.Encapsulate(r.bounds);

        localCheckPoint = transform.InverseTransformPoint(bounds.center);

        SetVisible(false);
    }

    private void OnEnable()
    {
        All.Add(this);
    }

    private void OnDisable()
    {
        All.Remove(this);
    }

    public void SetLit(bool lit)
    {
        if (lit && !revealed)
            Reveal();
        else if (!lit && revealed && !stayRevealed)
            Hide();
    }

    private void Reveal()
    {
        revealed = true;
        SetVisible(true);

        transform.DOKill();
        transform.localScale = Vector3.zero;
        transform.DOScale(originalScale, popTime).SetEase(Ease.OutBack);

        if (revealSound != null)
            AudioSource.PlayClipAtPoint(revealSound, CheckPoint);

        onRevealed?.Invoke();
    }

    public void Hide()
    {
        revealed = false;
        transform.DOKill();
        transform.localScale = originalScale;
        SetVisible(false);
    }

    public static void HideAll()
    {
        foreach (HiddenObject hidden in All)
            hidden.Hide();
    }

    private void SetVisible(bool visible)
    {
        foreach (Renderer r in renderers) r.enabled = visible;
        foreach (Collider c in colliders) c.enabled = visible;
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}