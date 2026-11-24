using UnityEngine;

public class DamageEvent {
    public IDamageable Target { get; }
    public float Amount { get; }
    public IDamageSource Source { get; }
    //public DamageType Type { get; }

    public DamageEvent(IDamageable target, float amount, IDamageSource source) {
        Target = target;
        Amount = amount;
        Source = source;
        //Type = type;
    }
}

public interface IDamageable {
    void TakeDamage(DamageEvent damageEvent);
    bool IsAlive { get; }
}

public interface IDamageSource {
    GameObject SourceObject { get; }
}