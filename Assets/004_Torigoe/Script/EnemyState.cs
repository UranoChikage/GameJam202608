using UnityEngine;

// 1. 巡回状態（Patrol）
public class PatrolState : IEnemyState
{
    public void Enter(EnemyAI enemy)
    {
        enemy.agent.speed = enemy.patrolSpeed;
        enemy.MoveToNextWaypoint();
    }

    public void Update(EnemyAI enemy)
    {
        // プレイヤーを発見したら ➔ 2. 追跡へ
        if (enemy.CanSeePlayer())
        {
            enemy.ChangeState(new ChaseState());
            return;
        }

        // 行き先に到着したら次のポイントへ
        if (!enemy.agent.pathPending && enemy.agent.remainingDistance < 0.5f)
        {
            enemy.MoveToNextWaypoint();
        }
    }

    public void Exit(EnemyAI enemy) { }
}

// 2. 追跡状態（Chase）
public class ChaseState : IEnemyState
{
    public void Enter(EnemyAI enemy)
    {
        enemy.agent.speed = enemy.chaseSpeed;
        enemy.SetLightColor(Color.red);
        enemy.PlayDetectSound();
    }

    public void Update(EnemyAI enemy)
    {
        if (enemy.CanSeePlayer())
        {
            enemy.agent.SetDestination(enemy.player.position);

            // --- 変更点: 接近時のダメージ攻撃判定 ---
            float distance = Vector3.Distance(enemy.transform.position, enemy.player.position);
            if (distance <= enemy.attackDistance)
            {
                // プレイヤーの PlayerHealth コンポーネントを取得して攻撃
                PlayerHealth playerHealth = enemy.player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    enemy.AttackPlayer(playerHealth);
                }
            }
        }
        else
        {
            // 視線が切れたら見失う状態へ
            enemy.ChangeState(new LostState());
        }
    }

    public void Exit(EnemyAI enemy) { }
}

// 3. 見失う状態（Lost）
public class LostState : IEnemyState
{
    private float timer;

    public void Enter(EnemyAI enemy)
    {
        enemy.agent.speed = enemy.patrolSpeed;
        enemy.SetLightColor(Color.magenta); // 探索色：紫
        timer = enemy.searchDuration;       // 探索カウントダウン開始
    }

    public void Update(EnemyAI enemy)
    {
        // 探索中に再びプレイヤーを発見したら ➔ 2. 追跡へ戻る
        if (enemy.CanSeePlayer())
        {
            enemy.ChangeState(new ChaseState());
            return;
        }

        // タイマーカウントダウン
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            // 時間切れで諦めて ➔ 1. 巡回へ戻る
            enemy.ChangeState(new PatrolState());
        }
    }

    public void Exit(EnemyAI enemy) { }
}