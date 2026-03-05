// Assets/Scripts/AI/MonsterAI.cs
using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour {
    [Header("References")]
    public Transform player;
    public NavMeshAgent agent;
    public Animator animator;

    [Header("Detection")]
    [SerializeField] public float chaseRadius = 8f;
    [SerializeField] public float attackRadiusIn = 1.8f;
    [SerializeField] public float attackRadiusOut = 2.5f;

    // 行为组件（Inspector 拖入，或 Awake 自动获取）
    public PatrolBehavior patrol;
    public ChaseBehavior chase;
    public AttackBehavior attack;

    public IEnemyBehavior currentBehavior;

    public void OnPlayerJoined() {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null) {
            Debug.Log($"{name}: Player found and assigned.");
        }
    }

    void Awake() {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        // 自动获取行为组件（更灵活）
        patrol = GetComponent<PatrolBehavior>();
        chase = GetComponent<ChaseBehavior>();
        attack = GetComponent<AttackBehavior>();

        // 默认从巡逻开始
        SwitchTo(patrol);
    }

    void Update() {
        // 实时检测状态切换（轻量，非协程依赖）
        if (player != null) {
            float dist = Vector3.Distance(transform.position, player.position);

            if (dist <= attackRadiusIn) {
                SwitchTo(attack);
            } else if (dist <= chaseRadius && dist > attackRadiusOut) {
                SwitchTo(chase);
            } else if (dist > chaseRadius) {
                SwitchTo(patrol);
            }
        }
    }

    public void SwitchTo(IEnemyBehavior newBehavior) {
        if (newBehavior == null) return;

        // 退出当前行为
        currentBehavior?.OnExit();

        // 进入新行为
        currentBehavior = newBehavior;
        currentBehavior.OnEnter(this);
    }

    // 供行为组件调用
    public bool IsPlayerInAttackRange() =>
        player != null && Vector3.Distance(transform.position, player.position) <= attackRadiusIn;

    public bool ShouldExitAttack() =>
        player != null && Vector3.Distance(transform.position, player.position) > attackRadiusOut;
}