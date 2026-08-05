using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("HP設定")]
    public int maxHealth = 4;        // 最大被弾回数（4回目で死亡）
    public float invincibleTime = 1.5f; // ダメージ後の無敵時間（秒）

    private int currentHealth;
    private bool isInvincible = false;
    private CharacterController characterController; // 移動制御用（アタッチされている場合）

    public int CurrentHealth => currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        characterController = GetComponent<CharacterController>();
    }

    // ダメージ処理
    public void TakeDamage(int damageAmount, Vector3 attackerPosition)
    {
        // 無敵時間中、またはすでに死亡している場合は処理しない
        if (isInvincible || currentHealth <= 0) return;

        currentHealth -= damageAmount;
        Debug.Log($"ダメージを受けました！ 残り耐久回数: {currentHealth - 1}回 (HP: {currentHealth}/{maxHealth})");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // ノックバックと無敵時間の開始
            ApplyKnockback(attackerPosition);
            StartCoroutine(InvincibleRoutine());
        }
    }

    // 敵から離れる方向へ弾き飛ばす
    private void ApplyKnockback(Vector3 attackerPosition)
    {
        Vector3 knockbackDir = (transform.position - attackerPosition).normalized;
        knockbackDir.y = 0.2f; // 少し浮かす

        // SimpleMoveやTranslate等、使用している移動コンポーネントに合わせて調整
        transform.position += knockbackDir * 2.0f;
    }

    // 無敵時間のタイマー処理
    private IEnumerator InvincibleRoutine()
    {
        isInvincible = true;

        // （任意）ここで点滅エフェクトなどを入れるとわかりやすくなります

        yield return new WaitForSeconds(invincibleTime);
        isInvincible = false;
    }

    // 死亡処理（4回目のダメージ）
    private void Die()
    {
        Debug.Log("プレイヤーが死亡しました。ゲームオーバー！");
        // ここにゲームオーバー画面の表示やシーンリロードの処理を記述
    }
}