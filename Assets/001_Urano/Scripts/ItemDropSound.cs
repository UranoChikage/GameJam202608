using UnityEngine;

/// <summary>アイテムが床などに着地した際、SEを1回再生する汎用コンポーネント。</summary>
[RequireComponent(typeof(Rigidbody))]
public class ItemDropSound : MonoBehaviour
{
    [SerializeField] private AudioClip[] dropClips;
    [SerializeField] private float minImpactSpeed = 1.5f;
    [SerializeField] private float cooldown = 0.2f;

    private float lastPlayTime = -999f;

    private void OnCollisionEnter(Collision collision)
    {
        if (dropClips == null || dropClips.Length == 0) return;
        if (Time.time - lastPlayTime < cooldown) return;
        if (collision.relativeVelocity.magnitude < minImpactSpeed) return;

        lastPlayTime = Time.time;

        AudioClip clip = dropClips[Random.Range(0, dropClips.Length)];

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySfx(clip, transform.position);
        }
    }
}
