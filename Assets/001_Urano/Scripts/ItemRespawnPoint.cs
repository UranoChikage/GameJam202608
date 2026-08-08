using UnityEngine;

/// <summary>DeathZoneに入った際、シーン開始時の位置へ戻すためのアイテム用コンポーネント。</summary>
public class ItemRespawnPoint : MonoBehaviour
{
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Rigidbody rb;

    private void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            // DeathZoneのColliderが薄いため、高速落下時のすり抜け(トンネリング)を防ぐ
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
    }

    public void ResetToStart()
    {
        transform.SetPositionAndRotation(startPosition, startRotation);

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
