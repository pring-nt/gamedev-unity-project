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

    [Header("Detection")]
    // pulls the corner points slightly inward so they don't sit exactly on the edge
    [Range(0f, 0.5f)] public float cornerInset = 0.1f;
    // extra points to test, e.g. for odd shapes
    public Transform[] extraCheckPoints;

    private Renderer[] renderers;
    private Collider[] colliders;
    private Vector3[] localCheckPoints;
    private Vector3 originalScale;
    private bool revealed;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        colliders = GetComponentsInChildren<Collider>(true);
        originalScale = transform.localScale;

        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        foreach (Renderer r in renderers)
            bounds.Encapsulate(r.bounds);

        BuildCheckPoints(bounds);
        SetVisible(false);
    }

    private void BuildCheckPoints(Bounds bounds)
    {
        Vector3 c = bounds.center;
        Vector3 e = bounds.extents;
        float x = e.x * (1f - cornerInset * 2f);
        float z = e.z * (1f - cornerInset * 2f);
        float top = e.y;

        Vector3[] worldPoints =
        {
            c,
            c + new Vector3(0f, top, 0f),
            c + new Vector3( x, top,  z),
            c + new Vector3(-x, top,  z),
            c + new Vector3( x, top, -z),
            c + new Vector3(-x, top, -z),
        };

        localCheckPoints = new Vector3[worldPoints.Length];
        for (int i = 0; i < worldPoints.Length; i++)
            localCheckPoints[i] = transform.InverseTransformPoint(worldPoints[i]);
    }

    private void OnEnable()
    {
        All.Add(this);
    }

    private void OnDisable()
    {
        All.Remove(this);
    }

    public bool IsLitBy(Light light, LayerMask obstacleMask)
    {
        foreach (Vector3 local in localCheckPoints)
            if (LightCone.IsLit(light, transform.TransformPoint(local), obstacleMask, transform))
                return true;

        if (extraCheckPoints != null)
            foreach (Transform point in extraCheckPoints)
                if (point != null && LightCone.IsLit(light, point.position, obstacleMask, transform))
                    return true;

        return false;
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
            AudioSource.PlayClipAtPoint(revealSound, transform.position);

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