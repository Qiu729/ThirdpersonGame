using System;

public static class GameEventSystem {
    public static event Action<DamageEvent> OnDamageDealt;

    public static void TriggerDamage(DamageEvent damageEvent) {
        OnDamageDealt?.Invoke(damageEvent);
    }
}