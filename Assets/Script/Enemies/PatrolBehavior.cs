// Assets/Scripts/AI/PatrolBehavior.cs
using UnityEngine;

[RequireComponent(typeof(MonsterAI))]
public class PatrolBehavior : MonoBehaviour, IEnemyBehavior {
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float waitTime = 1f;
    [SerializeField] private float stoppingDistance = 0.2f;
    [SerializeField] private bool faceMovement = true;

    private MonsterAI ai;
    private int currentPointIndex = 0;
    private float waitTimer;

    public void OnEnter(MonsterAI _ai) {
        ai = _ai;
        ai.animator?.SetBool("Move", true);
        currentPointIndex = 0;
        waitTimer = 0f;
    }

    public void OnExit() {
        waitTimer = 0f;
    }

    void Update() {
        if (ai == null) return;

        // 若已切换行为，停止更新
        if (ai.currentBehavior != this) return;

        // 等待阶段
        if (waitTimer > 0) {
            waitTimer -= Time.deltaTime;
            return;
        }

        // 移动阶段
        ai.animator?.SetBool("Move", true);
        Transform target = patrolPoints[currentPointIndex];
        Vector3 toTarget = target.position - transform.position;

        if (toTarget.magnitude > stoppingDistance) {
            // 使用 NavMeshAgent 移动
            ai.agent.speed = moveSpeed;
            ai.agent.SetDestination(target.position);
            ai.agent.stoppingDistance = stoppingDistance;

            //// 手动转向
            //if (faceMovement && ai.agent.velocity != Vector3.zero) {
            //    Vector3 flatVelocity = ai.agent.velocity;
            //    flatVelocity.y = 0;
            //    if (flatVelocity != Vector3.zero)
            //        transform.forward = Vector3.Slerp(transform.forward, flatVelocity.normalized, Time.deltaTime * 5f);
            //}
        } else {
            // 到达点，开始等待
            waitTimer = waitTime;
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
            ai.animator?.SetBool("Move", false);

        }
    }

    void OnDrawGizmosSelected() {
        if (patrolPoints == null) return;
        for (int i = 0; i < patrolPoints.Length; i++) {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(patrolPoints[i].position, 0.3f);
            if (i > 0) Gizmos.DrawLine(patrolPoints[i - 1].position, patrolPoints[i].position);
        }
    }
}