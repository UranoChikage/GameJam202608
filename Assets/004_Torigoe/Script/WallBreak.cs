using UnityEngine;

public class WallBreak : MonoBehaviour
{
    [Header("壊せるアイテムのタグ")]
    [SerializeField]
    private string breakTag = "Item";

    [Header("破片プレハブ")]
    [SerializeField]
    private GameObject brokenWallPrefab;

    [Header("破壊エフェクト（任意）")]
    [SerializeField]
    private ParticleSystem breakEffect;

    [Header("破壊音（任意）")]
    [SerializeField]
    private AudioClip breakSound;

    [Header("飛び散る力")]
    [SerializeField]
    private float explosionForce = 200f;

    [Header("爆発半径")]
    [SerializeField]
    private float explosionRadius = 3f;

    [Header("上方向への力")]
    [SerializeField]
    private float upwardsModifier = 1f;

    [Header("破片を消すまでの時間")]
    [SerializeField]
    private float destroyPiecesAfter = 3f;

    private bool isBroken = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (isBroken)
            return;

        if (!collision.gameObject.CompareTag(breakTag))
            return;

        isBroken = true;

        Vector3 hitPoint = collision.contacts[0].point;

        // エフェクト
        if (breakEffect != null)
        {
            Instantiate(breakEffect, hitPoint, Quaternion.identity);
        }

        // 効果音
        if (breakSound != null)
        {
            AudioSource.PlayClipAtPoint(breakSound, transform.position);
        }

        // 破片生成
        if (brokenWallPrefab != null)
        {
            GameObject broken =
                Instantiate(
                    brokenWallPrefab,
                    transform.position,
                    transform.rotation);

            Rigidbody[] pieces =
                broken.GetComponentsInChildren<Rigidbody>();

            foreach (Rigidbody rb in pieces)
            {
                rb.AddExplosionForce(
                    explosionForce,
                    hitPoint,
                    explosionRadius,
                    upwardsModifier,
                    ForceMode.Impulse);
            }

            Destroy(broken, destroyPiecesAfter);
        }

        // 元の壁を削除
        Destroy(gameObject);
    }
}