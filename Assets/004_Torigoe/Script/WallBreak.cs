using UnityEngine;
using System.Collections;

public class WallBreak : MonoBehaviour
{
    [Header("壁を壊せるアイテムのタグ")]
    [SerializeField] private string breakItemTag = "CannonBall";

    [Header("破壊アニメーションの長さ")]
    [SerializeField] private float destroyDelay = 1.2f;

    private Animator animator;
    private bool isBroken = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isBroken) return;

        if (collision.gameObject.CompareTag(breakItemTag))
        {
            isBroken = true;

            // 壁の当たり判定を無効化
            GetComponent<Collider>().enabled = false;

            // アニメーション再生
            if (animator != null)
            {
                animator.SetTrigger("Break");
            }

            // 大砲の弾を消す
            Destroy(collision.gameObject);

            // 壁を削除
            StartCoroutine(DestroyWall());
        }
    }

    IEnumerator DestroyWall()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}