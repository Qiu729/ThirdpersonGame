using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : DamageableBehaviour {
    public override void TakeDamage(DamageEvent e) {
        Debug.Log($"{name}take damage from {e.Source.ToString()}");
    }
}
