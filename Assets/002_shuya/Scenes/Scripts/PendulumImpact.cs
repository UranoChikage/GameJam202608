using UnityEngine;

/// <summary>振り子の進行方向側の面にPlayerが触れたときだけ吹き飛ばす。</summary>
public sealed class PendulumImpact : MonoBehaviour
{
    [SerializeField, Min(0f)] private float knockbackSpeed = 12f;
    [SerializeField, Min(0f)] private float upwardSpeed = 4f;
    [SerializeField, Range(-1f, 1f)] private float leadingSideThreshold = 0.15f;
    [SerializeField, Min(0f)] private float minimumHitSpeed = 0.5f;
    [SerializeField, Min(0f)] private float contactPadding = 0.2f;
    [SerializeField, Min(0f)] private float hitCooldown = 0.35f;

    private ConstantSpeedPendulum pendulum;
    private Collider hitCollider;
    private FPSCameraController lastHitPlayer;
    private float nextHitTime;

    private void Awake()
    {
        pendulum = GetComponentInParent<ConstantSpeedPendulum>();
        hitCollider = GetComponent<Collider>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryHitPlayer(collision.collider);
    }

    private void OnCollisionStay(Collision collision)
    {
        TryHitPlayer(collision.collider);
    }

    private void FixedUpdate()
    {
        if (hitCollider == null)
        {
            return;
        }

        Bounds bounds = hitCollider.bounds;
        float radius = bounds.extents.magnitude + contactPadding;
        Collider[] overlaps = Physics.OverlapSphere(
            bounds.center,
            radius,
            Physics.AllLayers,
            QueryTriggerInteraction.Ignore);

        foreach (Collider overlap in overlaps)
        {
            if (TryHitPlayer(overlap))
            {
                break;
            }
        }
    }

    private bool TryHitPlayer(Collider other)
    {
        FPSCameraController playerMovement =
            other.GetComponentInParent<FPSCameraController>();

        if (playerMovement == null || pendulum == null)
        {
            return false;
        }

        if (playerMovement == lastHitPlayer && Time.time < nextHitTime)
        {
            return false;
        }

        Vector3 hitCenter = hitCollider != null
            ? hitCollider.bounds.center
            : transform.position;
        Vector3 swingVelocity = pendulum.GetVelocityAtPoint(hitCenter);
        Vector3 horizontalSwing = Vector3.ProjectOnPlane(swingVelocity, Vector3.up);
        if (horizontalSwing.magnitude < minimumHitSpeed)
        {
            return false;
        }

        Vector3 swingDirection = horizontalSwing.normalized;
        Vector3 towardPlayer = Vector3.ProjectOnPlane(
            playerMovement.transform.position - hitCenter,
            Vector3.up).normalized;

        // Playerが振り子の進行方向側にいる場合だけ吹き飛ばす。
        if (Vector3.Dot(swingDirection, towardPlayer) < leadingSideThreshold)
        {
            return false;
        }

        playerMovement.AddKnockback(
            swingDirection * knockbackSpeed + Vector3.up * upwardSpeed);

        lastHitPlayer = playerMovement;
        nextHitTime = Time.time + hitCooldown;
        return true;
    }
}
