using System.Collections;
using UnityEngine;

/// <summary>Playerが近づくと、置かれた位置を支点に一度だけ倒れる。</summary>
public sealed class ProximityFallingPole : MonoBehaviour
{
    public enum FallAxis { X, Z }

    [Header("検知設定")]
    [SerializeField] private Transform player;
    [SerializeField, Min(0.1f)] private float triggerDistance = 3f;

    [Header("倒れる設定")]
    [SerializeField] private FallAxis fallAxis = FallAxis.X;
    [SerializeField, Range(-180f, 180f)] private float fallAngle = 80f;
    [SerializeField, Min(0.01f)] private float fallDuration = 0.8f;

    private bool hasFallen;

    private void Start()
    {
        if (player != null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void Update()
    {
        if (hasFallen || player == null)
        {
            return;
        }

        Vector3 offset = player.position - transform.position;
        offset.y = 0f;
        if (offset.sqrMagnitude <= triggerDistance * triggerDistance)
        {
            hasFallen = true;
            StartCoroutine(Fall());
        }
    }

    private IEnumerator Fall()
    {
        Quaternion startRotation = transform.localRotation;
        Vector3 axis = fallAxis == FallAxis.X ? Vector3.right : Vector3.forward;
        Quaternion endRotation = startRotation * Quaternion.AngleAxis(fallAngle, axis);

        float elapsed = 0f;
        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fallDuration);
            // 最初はゆっくり、重力で加速するような倒れ方。
            float fallT = t * t;
            transform.localRotation = Quaternion.Slerp(startRotation, endRotation, fallT);
            yield return null;
        }

        transform.localRotation = endRotation;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}
