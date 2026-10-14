using UnityEngine;

public class ViewmodelPendant : MonoBehaviour
{
    [Header("References")]
    public Transform anchor;
    public Transform gem;
    public Transform gemAttachPoint;
    public LineRenderer line;

    [Header("Rope")]
    public int segments = 5;
    public int constraintIterations = 8;
    public float gravity = 9.81f;
    [Range(0f, 1f)] public float damping = 0.1f;

    [Header("Feel")]
    // 0 = the cord moves rigidly with the staff, 1 = it fully lags behind like a real world-space rope
    [Range(0f, 1f)] public float inertia = 0.25f;
    // snap back if the staff jumps further than this in one frame (teleports, respawns)
    public float resetDistance = 3f;

    private Vector3[] points;
    private Vector3[] previousPoints;
    private float segmentLength;

    private Matrix4x4 lastAnchorMatrix;
    private Vector3 restDirectionLocal;
    private Quaternion gemRotationOffset;

    private void Awake()
    {
        Vector3 toGem = gemAttachPoint.position - anchor.position;
        segmentLength = toGem.magnitude / segments;

        restDirectionLocal = anchor.InverseTransformDirection(toGem.normalized);
        gemRotationOffset = Quaternion.Inverse(anchor.rotation) * gem.rotation;

        points = new Vector3[segments + 1];
        previousPoints = new Vector3[segments + 1];
    }

    private void OnEnable()
    {
        ResetRope();
    }

    private void ResetRope()
    {
        Vector3 start = anchor.position;
        Vector3 direction = anchor.TransformDirection(restDirectionLocal);

        for (int i = 0; i <= segments; i++)
        {
            points[i] = start + direction * segmentLength * i;
            previousPoints[i] = points[i];
        }

        lastAnchorMatrix = anchor.localToWorldMatrix;
    }

    private void LateUpdate()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        if (Vector3.Distance(anchor.position, lastAnchorMatrix.GetColumn(3)) > resetDistance)
            ResetRope();

        CarryWithAnchor();
        Simulate(dt);
        ApplyConstraints();
        UpdateVisuals();

        lastAnchorMatrix = anchor.localToWorldMatrix;
    }

    // moves the rope along with the staff, keeping only `inertia` of the motion as swing
    private void CarryWithAnchor()
    {
        Matrix4x4 delta = anchor.localToWorldMatrix * lastAnchorMatrix.inverse;
        float follow = 1f - inertia;

        for (int i = 0; i <= segments; i++)
        {
            points[i] = Vector3.Lerp(points[i], delta.MultiplyPoint3x4(points[i]), follow);
            previousPoints[i] = Vector3.Lerp(previousPoints[i], delta.MultiplyPoint3x4(previousPoints[i]), follow);
        }
    }

    private void Simulate(float dt)
    {
        float keep = Mathf.Pow(1f - damping, dt * 60f);
        Vector3 gravityStep = Vector3.down * gravity * dt * dt;

        for (int i = 1; i <= segments; i++)
        {
            Vector3 velocity = (points[i] - previousPoints[i]) * keep;
            previousPoints[i] = points[i];
            points[i] += velocity + gravityStep;
        }
    }

    private void ApplyConstraints()
    {
        for (int iteration = 0; iteration < constraintIterations; iteration++)
        {
            points[0] = anchor.position;

            for (int i = 0; i < segments; i++)
            {
                Vector3 offset = points[i + 1] - points[i];
                float distance = offset.magnitude;
                if (distance < 0.0001f) continue;

                Vector3 correction = offset * (1f - segmentLength / distance);

                // the first point is pinned to the staff, so the next one takes the whole correction
                if (i == 0)
                {
                    points[i + 1] -= correction;
                }
                else
                {
                    points[i] += correction * 0.5f;
                    points[i + 1] -= correction * 0.5f;
                }
            }
        }
    }

    private void UpdateVisuals()
    {
        Vector3 end = points[segments];
        Vector3 hangDirection = (end - points[segments - 1]).normalized;
        Vector3 restDirection = anchor.TransformDirection(restDirectionLocal);

        gem.rotation = Quaternion.FromToRotation(restDirection, hangDirection) * anchor.rotation * gemRotationOffset;
        gem.position += end - gemAttachPoint.position;

        line.positionCount = points.Length;
        line.SetPositions(points);
    }
}