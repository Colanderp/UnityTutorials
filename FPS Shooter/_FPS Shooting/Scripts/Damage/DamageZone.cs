using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageZone : MonoBehaviour
{
    public enum Zone { body, head }
    public Zone damageZone;

    Damageable damageable;

    private void Start()
    {
        damageable = GetComponentInParent<Damageable>();
    }

    public bool DamageableAlreadyDead()
    {
        if (damageable == null) return false;
        return damageable.isDead();
    }

    public bool Damage(float dmg, float headMult) // Returns true if you killed them
    {
        if (damageable == null) return false;

        float overallDmg = dmg;
        overallDmg *= (damageZone == Zone.head) ? headMult : 1f;
        return damageable.Damage(overallDmg);
    }
}
