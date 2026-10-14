using DG.Tweening;
using UnityEngine;

public class TargetFlip : MonoBehaviour, IDamageable
{
    public enum Axis { X, Z }

    [Header("References")]
    public Transform pivot;
    public Collider hitCollider;

    [Header("Flip")]
    public Axis flipAxis = Axis.X;
    // positive or negative decides which way it falls; flip the sign if it falls toward you
    public float flipAngle = -90f;
    public float flipDuration = 0.35f;
    public Ease flipEase = Ease.OutBounce;

    [Header("Stand Up")]
    public float downTime = 2f;
    public float standDuration = 0.5f;
    public Ease standEase = Ease.OutBack;

    [Header("Movement")]
    public bool moveSideways;
    public Axis moveAxis = Axis.X;
    // negative distance moves the other way first
    public float moveDistance = 4f;
    public float moveDuration = 2f;
    public Ease moveEase = Ease.InOutSine;
    public bool pauseWhileDown = true;

    [Header("Filter")]
    // only thrown projectiles knock it over, not the player walking into it
    public bool projectilesOnly = true;

    private Quaternion uprightRotation;
    private Sequence flipSequence;
    private Tween moveTween;
    private bool flipped;

    private void Awake()
    {
        uprightRotation = pivot.localRotation;
    }

    private void Start()
    {
        if (moveSideways)
            StartMoving();
    }

    private void StartMoving()
    {
        Vector3 axis = moveAxis == Axis.X ? Vector3.right : Vector3.forward;
        Vector3 target = pivot.localPosition + axis * moveDistance;

        moveTween = pivot.DOLocalMove(target, moveDuration)
            .SetEase(moveEase)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (flipped) return;

        if (projectilesOnly && (collision.rigidbody == null || !collision.rigidbody.TryGetComponent(out ProjectileLifetime _)))
            return;

        if (hitCollider != null && collision.GetContact(0).thisCollider != hitCollider)
            return;

        Flip();
    }

    public void Flip()
    {
        flipped = true;

        if (pauseWhileDown)
            moveTween?.Pause();

        Vector3 flipEuler = flipAxis == Axis.X
            ? new Vector3(flipAngle, 0f, 0f)
            : new Vector3(0f, 0f, flipAngle);

        Quaternion downRotation = uprightRotation * Quaternion.Euler(flipEuler);

        flipSequence?.Kill();
        flipSequence = DOTween.Sequence()
            .Append(pivot.DOLocalRotateQuaternion(downRotation, flipDuration).SetEase(flipEase))
            .AppendInterval(downTime)
            .Append(pivot.DOLocalRotateQuaternion(uprightRotation, standDuration).SetEase(standEase))
            .OnComplete(() =>
            {
                flipped = false;
                moveTween?.Play();
            });
    }

    private void OnDestroy()
    {
        flipSequence?.Kill();
        moveTween?.Kill();
    }

    public void TakeDamage(float amount)
    {
        if (flipped) return;

        Flip();
    }
}