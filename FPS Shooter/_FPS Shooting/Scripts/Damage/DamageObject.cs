using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageObject : PooledObject
{
    public LayerMask damageLayer;
    private float damage;
    public float Damage
    {
        get { return damage; }
        set { damage = value; }
    }

    public virtual int Simulate()
    {
        return -1;
    }
}
