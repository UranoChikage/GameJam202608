using UnityEngine;

/// <summary>
/// Rotates a pendulum with smooth acceleration and deceleration.
/// Attach this component to the pendulum's pivot object.
/// </summary>
public sealed class ConstantSpeedPendulum : MonoBehaviour
{
    public enum SwingAxis
    {
        X,
        Y,
        Z
    }

    [Header("Pendulum Settings")]
    [SerializeField] private SwingAxis swingAxis = SwingAxis.Z;
    [SerializeField, Range(0f, 180f)] private float maxAngle = 45f;
    [SerializeField, Min(0f)] private float angularSpeed = 60f;

    private Quaternion startRotation;
    private float elapsedTime;
    private float currentAngularVelocity;
    private Rigidbody pendulumRigidbody;

    private void Awake()
    {
        startRotation = transform.localRotation;
        pendulumRigidbody = GetComponent<Rigidbody>();

        if (pendulumRigidbody != null)
        {
            pendulumRigidbody.isKinematic = true;
            pendulumRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    private void Update()
    {
        // Rigidbodyがない振り子は描画フレームごとに更新し、カクつきを防ぐ。
        if (pendulumRigidbody == null)
        {
            AdvancePendulum(Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        if (pendulumRigidbody != null)
        {
            AdvancePendulum(Time.fixedDeltaTime);
        }
    }

    private void AdvancePendulum(float deltaTime)
    {
        if (angularSpeed <= 0f || maxAngle <= 0f)
        {
            return;
        }

        elapsedTime += deltaTime;

        // angularSpeedを中央通過時の最高速度として扱う。
        // Sin波にすることで両端では自然に減速し、滑らかに折り返す。
        float angularFrequency = angularSpeed / maxAngle;
        float currentAngle = maxAngle * Mathf.Sin(elapsedTime * angularFrequency);
        currentAngularVelocity = angularSpeed * Mathf.Cos(elapsedTime * angularFrequency);
        Quaternion targetRotation = startRotation * Quaternion.AngleAxis(currentAngle, GetAxis());

        if (pendulumRigidbody != null)
        {
            Quaternion worldTarget = transform.parent == null
                ? targetRotation
                : transform.parent.rotation * targetRotation;
            pendulumRigidbody.MoveRotation(worldTarget);
        }
        else
        {
            transform.localRotation = targetRotation;
        }
    }

    /// <summary>振り子上の指定位置が現在どちらへ、どの速さで動いているかを返す。</summary>
    public Vector3 GetVelocityAtPoint(Vector3 worldPoint)
    {
        Vector3 worldAxis = transform.TransformDirection(GetAxis()).normalized;
        Vector3 angularVelocity = worldAxis * (currentAngularVelocity * Mathf.Deg2Rad);
        return Vector3.Cross(angularVelocity, worldPoint - transform.position);
    }

    private Vector3 GetAxis()
    {
        return swingAxis switch
        {
            SwingAxis.X => Vector3.right,
            SwingAxis.Y => Vector3.up,
            SwingAxis.Z => Vector3.forward,
            _ => Vector3.forward
        };
    }

    private void OnValidate()
    {
        maxAngle = Mathf.Clamp(maxAngle, 0f, 180f);
        angularSpeed = Mathf.Max(0f, angularSpeed);
    }
}
