using UnityEngine;

/// <summary>
/// Moves a platform back and forth from its starting position.
/// </summary>
public class MovingPlatform : MonoBehaviour
{
    public enum MoveDirection
    {
        ForwardBackward,
        LeftRight,
        UpDown
    }

    [Header("Movement Settings")]
    [SerializeField] private MoveDirection moveDirection = MoveDirection.ForwardBackward;
    [SerializeField, Min(0f)] private float speed = 2f;
    [SerializeField, Min(0f)] private float moveDistance = 3f;

    [Header("Options")]
    [Tooltip("Use the platform's own axes instead of world axes.")]
    [SerializeField] private bool useLocalDirection = true;

    private Vector3 startPosition;
    private Vector3 movementAxis;
    private float movementProgress;
    private Rigidbody platformRigidbody;

    private void Awake()
    {
        startPosition = transform.position;
        movementAxis = GetDirection();
        platformRigidbody = GetComponent<Rigidbody>();

        if (platformRigidbody != null)
        {
            platformRigidbody.isKinematic = true;
        }
    }

    private void FixedUpdate()
    {
        if (speed <= 0f || moveDistance <= 0f)
        {
            return;
        }

        movementProgress += speed * Time.fixedDeltaTime;
        float offset = Mathf.PingPong(movementProgress + moveDistance, moveDistance * 2f) - moveDistance;
        Vector3 targetPosition = startPosition + movementAxis * offset;

        if (platformRigidbody != null)
        {
            platformRigidbody.MovePosition(targetPosition);
        }
        else
        {
            transform.position = targetPosition;
        }
    }

    private Vector3 GetDirection()
    {
        if (useLocalDirection)
        {
            return moveDirection switch
            {
                MoveDirection.ForwardBackward => transform.forward,
                MoveDirection.LeftRight => transform.right,
                MoveDirection.UpDown => transform.up,
                _ => transform.forward
            };
        }

        return moveDirection switch
        {
            MoveDirection.ForwardBackward => Vector3.forward,
            MoveDirection.LeftRight => Vector3.right,
            MoveDirection.UpDown => Vector3.up,
            _ => Vector3.forward
        };
    }

    private void OnValidate()
    {
        speed = Mathf.Max(0f, speed);
        moveDistance = Mathf.Max(0f, moveDistance);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = Application.isPlaying ? startPosition : transform.position;
        Vector3 direction = GetDirection();

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin, origin + direction * moveDistance);
        Gizmos.DrawLine(origin, origin - direction * moveDistance);
        Gizmos.DrawWireSphere(origin + direction * moveDistance, 0.15f);
        Gizmos.DrawWireSphere(origin - direction * moveDistance, 0.15f);
    }
}
