using System.Collections;
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
    public float searchDuration = 3.0f;

    [Header("攻撃設定")]
    public int attackDamage = 1;       // ダメージ量
    public float attackCooldown = 1.0f; // 攻撃後に敵が止まる時間（秒）

    [HideInInspector] public NavMeshAgent agent;
    private IEnemyState currentState;
    private int currentWaypointIndex = 0;
    private bool isStopped = false; // 硬直中フラグ

    public bool IsStopped => isStopped;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        ChangeState(new PatrolState());
    }

    void Update()
    {
        // 攻撃直後の硬直中は追跡や移動の処理を行わない
        if (isStopped) return;

        currentState?.Update(this);
    }

    // ★物理的に衝突した瞬間にダメージ・吹き飛ばし・敵停止を発動！
    private void OnCollisionEnter(Collision collision)
    {
        // ぶつかった相手がプレイヤーの場合
        if (collision.gameObject.CompareTag("Player") || collision.transform == player)
        {
            if (isStopped) return;

            PlayerScript playerScript = collision.gameObject.GetComponent<PlayerScript>();
            if (playerScript != null)
            {
                // 1. ダメージ ＆ 吹き飛ばしを実行
                playerScript.TakeDamage(attackDamage, transform.position);

                // 2. エネミー自身をピタッと停止させる
                StartCoroutine(StopMovementRoutine(attackCooldown));

            }
        }
    }

    // 攻撃後にエネミーの足を止める処理
    public IEnumerator StopMovementRoutine(float stopTime)
    {
        isStopped = true;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;       // NavMeshAgentの移動停止
            agent.velocity = Vector3.zero; // 慣性を切る
        }

        yield return new WaitForSeconds(stopTime);

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;      // 移動再開
        }

        isStopped = false;
    }

    public void ChangeState(IEnemyState newState)
    {
        currentState?.Exit(this);
        currentState = newState;
        currentState.Enter(this);
    }

    public bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > viewDistance) return false;

        if (Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2f)
        {
            if (!Physics.Raycast(transform.position + Vector3.up, dirToPlayer, distanceToPlayer, LayerMask.GetMask("Obstacle")))
            {
                return true;
            }
        }
        return false;
    }

    public void MoveToNextWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        agent.SetDestination(waypoints[currentWaypointIndex].position);
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    }

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
}