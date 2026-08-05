using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("参照設定")]
    public Transform player;
    public Transform[] waypoints;
    public Light eyeLight;
    public AudioSource audioSource;
    public AudioClip detectSound;

    [Header("移動・視界パラメータ")]
    public float patrolSpeed = 2.0f;
    public float chaseSpeed = 5.5f;
    public float viewDistance = 10.0f;
    [Range(0, 360)] public float viewAngle = 120.0f;
    public float searchDuration = 3.0f; // 見失ってから巡回に戻るまでの時間

    [Header("攻撃設定")]
    public float attackDistance = 1.5f; // 攻撃（ダメージ）が届く距離
    public int attackDamage = 1;       // 1回あたりのダメージ量

    [HideInInspector] public NavMeshAgent agent;
    private IEnemyState currentState;
    private int currentWaypointIndex = 0;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // 最初は「1. 巡回状態」からスタート
        ChangeState(new PatrolState());
    }

    void Update()
    {
        currentState?.Update(this);
    }

    public void ChangeState(IEnemyState newState)
    {
        currentState?.Exit(this);
        currentState = newState;
        currentState.Enter(this);
    }

    // --- 視界（プレイヤー検知）判定 ---
    public bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > viewDistance) return false;

        if (Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2f)
        {
            // 障害物（Obstacleレイヤー）の陰に隠れていないか確認
            if (!Physics.Raycast(transform.position + Vector3.up, dirToPlayer, distanceToPlayer, LayerMask.GetMask("Obstacle")))
            {
                return true;
            }
        }
        return false;
    }

    // --- 巡回地点の更新 ---
    public void MoveToNextWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        agent.SetDestination(waypoints[currentWaypointIndex].position);
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    }

    // --- 攻撃実行 ---
    public void AttackPlayer(PlayerHealth playerHealth)
    {
        // プレイヤーにダメージを与える（自分の位置を渡してノックバック方向を計算させる）
        playerHealth.TakeDamage(attackDamage, transform.position);
    }

    // --- 演出・補助用 ---
    public void SetLightColor(Color color)
    {
        if (eyeLight != null) eyeLight.color = color;
    }

    public void PlayDetectSound()
    {
        if (audioSource != null && detectSound != null)
        {
            audioSource.PlayOneShot(detectSound);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // ギズモで視界距離を表示
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, viewDistance);
    }
}