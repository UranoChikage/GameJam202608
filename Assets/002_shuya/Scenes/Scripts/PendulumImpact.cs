using UnityEngine;

/// <summary>振り子の進行方向側の面にPlayerが触れたときだけ吹き飛ばす。</summary>
public sealed class PendulumImpact : MonoBehaviour
{
    [SerializeField, Min(0f)] private float knockbackSpeed = 12f;
    [SerializeField, Min(0f)] private float upwardSpeed = 4f;
    [SerializeField, Range(-1f, 1f)] private float leadingSideThreshold = 0.15f;
    [SerializeField, Min(0f)] private float minimumHitSpeed = 0.5f;

    private ConstantSpeedPendulum pendulum;

    private void Awake()
    {
        pendulum = GetComponentInParent<ConstantSpeedPendulum>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        FPSCameraController playerMovement =
            collision.collider.GetComponentInParent<FPSCameraController>();

        if (playerMovement == null || pendulum == null)
        {
            return;
        }

        Vector3 swingVelocity = pendulum.GetVelocityAtPoint(transform.position);
        Vector3 horizontalSwing = Vector3.ProjectOnPlane(swingVelocity, Vector3.up);
        if (horizontalSwing.magnitude < minimumHitSpeed)
        {
            return;
        }

        Vector3 swingDirection = horizontalSwing.normalized;
        Vector3 towardPlayer = Vector3.ProjectOnPlane(
            playerMovement.transform.position - transform.position,
            Vector3.up).normalized;

        // Playerが振り子の進行方向側にいる場合だけ吹き飛ばす。
        if (Vector3.Dot(swingDirection, towardPlayer) < leadingSideThreshold)
        {
            return;
        }

        playerMovement.AddKnockback(
            swingDirection * knockbackSpeed + Vector3.up * upwardSpeed);
    }
}
