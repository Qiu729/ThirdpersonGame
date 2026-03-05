// Assets/Scripts/AI/ChaseBehavior.cs
using UnityEngine;

[RequireComponent(typeof(MonsterAI))]
public class ChaseBehavior : MonoBehaviour, IEnemyBehavior {
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private bool faceTarget = true;

    private MonsterAI ai;

    public void OnEnter(MonsterAI _ai) {
        ai = _ai;
        ai.animator?.SetBool("Move", true);
    }

    public void OnExit() { }

    void Update() {
        if (ai == null || ai.player == null || ai.currentBehavior != this) return;

        // 实时检测：是否该切 Attack / Patrol
        float dist = Vector3.Distance(transform.position, ai.player.position);
        if (ai.IsPlayerInAttackRange()) {
            ai.SwitchTo(ai.attack);
            return;
        } else if (dist > ai.GetComponent<MonsterAI>().chaseRadius) {
            ai.SwitchTo(ai.patrol);
            return;
        }

        // 追击玩家
        ai.agent.speed = chaseSpeed;
        ai.agent.SetDestination(ai.player.position);

        if (faceTarget && ai.agent.velocity != Vector3.zero) {
            Vector3 dir = ai.agent.velocity;
            dir.y = 0;
            if (dir != Vector3.zero)
                transform.forward = Vector3.Slerp(transform.forward, dir.normalized, Time.deltaTime * 12f);
        }
    }
}