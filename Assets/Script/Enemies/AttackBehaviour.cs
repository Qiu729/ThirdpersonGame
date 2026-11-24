// Assets/Scripts/AI/AttackBehavior.cs
using UnityEngine;

[RequireComponent(typeof(MonsterAI))]
public class AttackBehavior : MonoBehaviour, IEnemyBehavior {
    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private bool faceTarget = true;

    private MonsterAI ai;
    private float nextAttackTime;

    public void OnEnter(MonsterAI _ai) {
        ai = _ai;
        ai.animator?.SetBool("Move", false);
        nextAttackTime = Time.time;
    }

    public void OnExit() { }

    void Update() {
        if (ai == null || ai.player == null || ai.currentBehavior != this) return;

        // 检测退出条件
        if (ai.ShouldExitAttack()) {
            ai.SwitchTo(ai.chase);
            return;
        }

        // 面向玩家
        if (faceTarget) {
            Vector3 dir = ai.player.position - transform.position;
            dir.y = 0;
            if (dir != Vector3.zero)
                transform.forward = Vector3.Slerp(transform.forward, dir.normalized, Time.deltaTime * 10f);
        }

        // 执行攻击
        if (Time.time >= nextAttackTime) {
            DoAttack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void DoAttack() {
        // 动画
        ai.animator.SetTrigger("Attack");

        // 播放音效/特效


        // 调用伤害系统（RPC / 事件）
        var player = ai.player.gameObject.GetComponent<DamageableBehaviour>();
        if (player != null) {
            //Debug.Log("triggerDamge!");
            GameEventSystem.TriggerDamage(new DamageEvent(player, 1, ai));
        }
    }
}