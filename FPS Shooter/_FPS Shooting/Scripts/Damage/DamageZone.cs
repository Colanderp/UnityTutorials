using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageZone : MonoBehaviour
{
    public enum Zone { body, head }
    public Zone damageZone;

    Damagable damagable;

    private void Start()
    {
        damagable = GetComponentInParent<Damagable>();
    }

    public bool Damage(float dmg, float headMult) // Returns true if you killed them
    {
        float overallDmg = dmg;
        overallDmg *= (damageZone == Zone.head) ? headMult : 1f;
        return damagable.Damage(overallDmg);
    }
}
