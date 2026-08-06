using UnityEngine;

/// <summary>
/// Rotates a pendulum back and forth at a constant angular speed.
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
    private float currentAngle;
    private float targetAngle;
    private Rigidbody pendulumRigidbody;

    private void Awake()
    {
        startRotation = transform.localRotation;
        targetAngle = maxAngle;
        pendulumRigidbody = GetComponent<Rigidbody>();

        if (pendulumRigidbody != null)
        {
            pendulumRigidbody.isKinematic = true;
        }
    }

    private void FixedUpdate()
    {
        if (angularSpeed <= 0f || maxAngle <= 0f)
        {
            return;
        }

        currentAngle = Mathf.MoveTowards(
            currentAngle,
            targetAngle,
            angularSpeed * Time.fixedDeltaTime);

        if (Mathf.Approximately(currentAngle, targetAngle))
        {
            targetAngle = -targetAngle;
        }

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
