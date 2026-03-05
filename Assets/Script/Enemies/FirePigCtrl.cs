using Starter.ThirdPersonCharacter;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class FirePigCtrl : MonoBehaviour {
    [Header("攻击设置")]
    [SerializeField] private float attackRadiusIn = 2.0f;   // 进入攻击的距离
    [SerializeField] private float attackRadiusOut = 2.5f;  // 退出攻击的距离（必须 > In）
    [SerializeField] private float attackCooldown = 1.5f;   // 攻击冷却时间

    [Header("游走设置")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float waitTime = 1f;
    [SerializeField] private float stoppingDistance = 0.2f;

    [Header("追击设置")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float chaseRadius = 8f;        // 开始追击的距离
    [SerializeField] private float returnRadius = 10f;      // 超出此距离返回巡逻
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private bool faceTarget = true;

    [Header("引用")]
    private Animator animator;
    public bool isWaking = false;

    // 内部状态
    private NavMeshAgent navAgent;
    private Transform player;
    private int currentPatrolIndex = 0;
    private Coroutine currentRoutine;
    private bool canAttack = true;

    public enum State { Patrol, Chase, Attack }
    public State currentState = State.Patrol;

    private void Awake() {
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        if (navAgent != null) {
            navAgent.updateRotation = false; // 手动控制朝向
            navAgent.speed = moveSpeed;
            navAgent.stoppingDistance = stoppingDistance;
        }

        // 设置触发器（用于高效检测玩家进入范围）
        var col = GetComponent<SphereCollider>();
        if (col != null) {
            col.isTrigger = true;
            col.radius = returnRadius; // 用最大半径覆盖
        }
    }

    private void Start() {
        if (patrolPoints.Length == 0) {
            Debug.LogWarning($"{name}: No patrol points assigned.", this);
            enabled = false;
            return;
        }

        StartPatrol();
    }

    // 需要在编辑器中订阅GameManger的PlayerJoin事件
    public void OnPlayerJoin() {
        player = GameObject.FindWithTag("Player")?.transform;
        if (player == null) {
            Debug.LogError($"{name}: Player not found via GameManager. Check GameManager setup.", this);
            enabled = false;
            return;
        } else {
            Debug.Log($"{name}: Player found: {player.name}");
        }

    }

    private void LateUpdate() {
        // 同步动画参数
        animator?.SetBool("Move", isWaking);
    }

    // ===== 状态管理 =====
    private void StartPatrol() {
        Debug.Log($"{name}: → Patrol", this);
        StopCurrentRoutine();
        currentState = State.Patrol;
        isWaking = true;
        currentRoutine = StartCoroutine(PatrolRoutine());
    }

    private void StartChase() {
        Debug.Log($"{name}: → Chase", this);
        StopCurrentRoutine();
        currentState = State.Chase;
        isWaking = true;
        currentRoutine = StartCoroutine(ChaseRoutine());
    }

    private void StartAttack() {
        Debug.Log($"{name}: → Attack", this);
        StopCurrentRoutine();
        currentState = State.Attack;
        isWaking = false; // 攻击时通常停住（根据动画调整）
        currentRoutine = StartCoroutine(AttackRoutine());
    }

    private void StopCurrentRoutine() {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);
    }

    // ===== 状态协程 =====
    private IEnumerator PatrolRoutine() {
        while (true) {

            // 等待获取玩家引用
            if (player == null) {
                yield return new WaitForSeconds(0.2f);
                continue;
            }

            float distToPlayer = Vector3.Distance(transform.position, player.position);
            if (ShouldChase()) {
                StartChase();
                yield break;
            }

            // 移动到巡逻点
            Transform target = patrolPoints[currentPatrolIndex];
            navAgent.speed = moveSpeed;
            navAgent.SetDestination(target.position);

            while (navAgent.pathPending || navAgent.remainingDistance > navAgent.stoppingDistance) {
                // 🔔 新增：实时检测玩家是否进入追击范围！
                if (player != null) {
                    distToPlayer = Vector3.Distance(transform.position, player.position);
                    if (distToPlayer <= chaseRadius) {
                        Debug.Log($"{name}: Player detected during patrol move → Switch to Chase!", this);
                        StartChase(); // 中断当前移动，进入追击
                        yield break;
                    }
                }

                // 正常转向 & 移动
                if (faceTarget && navAgent.velocity != Vector3.zero) {
                    transform.forward = Vector3.Slerp(transform.forward, navAgent.velocity.normalized, Time.deltaTime * 5f);
                }

                yield return null;
            }

            yield return new WaitForSeconds(waitTime);
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    private IEnumerator ChaseRoutine() {
        while (player != null) {
            float distToPlayer = Vector3.Distance(transform.position, player.position);

            if (ShouldReturnToPatrol()) {
                StartPatrol();
                yield break;
            } else if (ShouldAttack()) {
                StartAttack();
                yield break;
            }

            // 追击玩家
            navAgent.speed = chaseSpeed;
            navAgent.SetDestination(player.position);

            if (faceTarget && navAgent.velocity != Vector3.zero) {
                // 追击时快速转向盯紧玩家
                transform.forward = Vector3.Slerp(transform.forward, navAgent.velocity.normalized, Time.deltaTime * 12f);
            }

            yield return null;
        }
    }

    private IEnumerator AttackRoutine() {
        while (player != null) {
            float distToPlayer = Vector3.Distance(transform.position, player.position);

            if (ShouldExitAttack()) {
                StartChase();
                yield break;
            }

            // 精确面向玩家（水平方向）
            if (faceTarget) {
                Vector3 direction = (player.position - transform.position).normalized;
                direction.y = 0;
                transform.forward = Vector3.Slerp(transform.forward, direction, Time.deltaTime * 12f);
            }

            // 执行攻击
            if (canAttack) {
                canAttack = false;
                DoAttack();
                yield return new WaitForSeconds(attackCooldown);
                canAttack = true;
            }

            yield return null;
        }
    }

    // ===== 攻击行为 =====
    private void DoAttack() {
        Debug.Log($"{name} attacks {player?.name}!", this);

        // 🔔 在此处添加：
        // - 播放攻击动画：animator.SetTrigger("Attack");
        // - 造成伤害：player.GetComponent<PlayerHealth>()?.TakeDamage(10);
        // - 播放音效、特效等
    }

    // ===== 触发检测（高效）=====
    private void OnTriggerEnter(Collider other) {
        if ((playerLayer.value & (1 << other.gameObject.layer)) == 0) return;

        if (currentState == State.Patrol) {
            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist <= chaseRadius) {
                StartChase();
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if ((playerLayer.value & (1 << other.gameObject.layer)) == 0) return;

        if (currentState == State.Chase || currentState == State.Attack) {
            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist > returnRadius) {
                StartPatrol();
            }
        }
    }

    private bool ShouldChase() {
        return player != null &&
               Vector3.Distance(transform.position, player.position) <= chaseRadius;
    }

    private bool ShouldAttack() {
        return player != null &&
               Vector3.Distance(transform.position, player.position) <= attackRadiusIn;
    }
    private bool ShouldExitAttack() {
        return player != null &&
               Vector3.Distance(transform.position, player.position) > attackRadiusOut;
    }

    private bool ShouldReturnToPatrol() {
        return player != null &&
               Vector3.Distance(transform.position, player.position) > returnRadius;
    }

    //// ===== 调试可视化 =====
    //private void OnDrawGizmosSelected() {
    //    if (Application.isPlaying && player != null) {
    //        Gizmos.color = Color.yellow;
    //        Gizmos.DrawWireSphere(transform.position, chaseRadius);
    //        Gizmos.color = Color.red;
    //        Gizmos.DrawWireSphere(transform.position, attackRadius);
    //        Gizmos.color = Color.gray;
    //        Gizmos.DrawWireSphere(transform.position, returnRadius);
    //    }
    //}
}