using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("HP設定")]
    public int maxHealth = 4;           // 最大耐久数（4回目で死亡）
    public float invincibleTime = 1.5f; // 無敵時間（秒）

    [Header("吹き飛ばし設定")]
    public float knockbackDistance = 3.0f; // 吹っ飛ぶ距離（メートル）
    public float knockbackDuration = 0.1f; // 吹っ飛ぶ時間（秒）

    private int currentHealth;
    private bool isInvincible = false;
    private CharacterController cc;

    public int CurrentHealth => currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        cc = GetComponent<CharacterController>();
    }

    // ダメージ処理
    public void TakeDamage(int damageAmount, Vector3 attackerPosition)
    {
        Debug.Log("テイクダメージが呼ばれました！！");
        // 無敵時間中、または死亡済みの場合は処理しない
        if (isInvincible || currentHealth <= 0) return;

        currentHealth -= damageAmount;
        Debug.Log($"<color=red>【ダメージ受傷】</color> 残り耐久: {currentHealth - 1}回 (HP: {currentHealth}/{maxHealth})");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // 吹き飛ばしと無敵処理を開始
            StartCoroutine(KnockbackRoutine(attackerPosition));
            StartCoroutine(InvincibleRoutine());
        }
    }

    // すり抜けず、壁にも埋まらずに1回だけ吹っ飛ばす処理
    private IEnumerator KnockbackRoutine(Vector3 attackerPosition)
    {
        // 攻撃者の反対方向（水平方向）を算出
        Vector3 dir = (transform.position - attackerPosition);
        dir.y = 0;
        dir = dir.normalized;

        // 壁や敵に埋まらないよう、レイキャストで移動可能な限界距離を調べる
        float moveDistance = knockbackDistance;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, dir, out RaycastHit hit, knockbackDistance))
        {
            // 障害物がある場合は、その手前までの距離に制限
            moveDistance = Mathf.Max(0, hit.distance - 0.5f);
        }

        float timer = 0;
        float speed = moveDistance / knockbackDuration;

        while (timer < knockbackDuration)
        {
            Vector3 move = dir * speed * Time.deltaTime;

            if (cc != null && cc.enabled)
            {
                cc.Move(move);
            }
            else
            {
                transform.position += move;
            }

            timer += Time.deltaTime;
            yield return null;
        }
    }

    // 無敵タイマー処理
    private IEnumerator InvincibleRoutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibleTime);
        isInvincible = false;
    }

    private void Die()
    {
        Debug.Log("<color=black>【死亡】プレイヤーが死亡しました。ゲームオーバー！</color>");
    }
}