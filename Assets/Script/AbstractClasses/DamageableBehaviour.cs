// DamageableBehaviour.cs
using UnityEngine;

public abstract class DamageableBehaviour : MonoBehaviour, IDamageable {
    [SerializeField] protected float maxHealth = 100f;
    protected float currentHealth;

    public bool IsAlive => currentHealth > 0;

    protected virtual void OnEnable() {
        GameEventSystem.OnDamageDealt += HandleDamage;
    }

    protected virtual void OnDisable() {
        GameEventSystem.OnDamageDealt -= HandleDamage;
    }

    private void HandleDamage(DamageEvent e) {
        // 基类统一处理"是否是自己"的判断
        if (e.Target is DamageableBehaviour target && target.gameObject == this.gameObject)
            TakeDamage(e);
    }

    // 子类只需关注"受伤后做什么"
    public abstract void TakeDamage(DamageEvent e);
}