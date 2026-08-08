using UnityEngine;

/// <summary>
/// Moves a platform back and forth from its starting position.
/// </summary>
public class MovingPlatform : MonoBehaviour, IMovingPlatform
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
    private Vector3 previousPosition;

    /// <summary>直前のFixedUpdateからの移動量。乗っているプレイヤー等に加算するために使う。</summary>
    public Vector3 DeltaMovement { get; private set; }

    private void Awake()
    {
        startPosition = transform.position;
        previousPosition = startPosition;
        movementAxis = GetDirection();
        platformRigidbody = GetComponent<Rigidbody>();

        if (platformRigidbody != null)
        {
            platformRigidbody.isKinematic = true;

            // FixedUpdate間の動きを補間
            platformRigidbody.interpolation =
                RigidbodyInterpolation.Interpolate;
        }

        // 接触摩擦による押し出しとDeltaMovementの手動加算が二重に効くのを防ぐ
        MovingPlatformFriction.ApplyFrictionless(
            GetComponentsInChildren<Collider>()
        );
    }

    private void FixedUpdate()
    {
        if (speed <= 0f || moveDistance <= 0f)
        {
            DeltaMovement = Vector3.zero;
            return;
        }

        // speedを最大移動速度として位相を進める
        movementProgress +=
            speed / moveDistance *
            Time.fixedDeltaTime;

        // 端で減速して滑らかに折り返す
        float offset =
            Mathf.Sin(movementProgress) *
            moveDistance;

        Vector3 targetPosition =
            startPosition +
            movementAxis * offset;

        if (platformRigidbody != null)
        {
            platformRigidbody.MovePosition(
                targetPosition
            );
        }
        else
        {
            transform.position = targetPosition;
        }

        DeltaMovement = targetPosition - previousPosition;
        previousPosition = targetPosition;
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
