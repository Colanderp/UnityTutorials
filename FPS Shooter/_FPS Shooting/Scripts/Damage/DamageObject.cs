using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageObject : PooledObject
{
    public LayerMask damageLayer;
    private float damage;

    Transform parentAdjuster;
    float size = 0.05f;

    public float Damage
    {
        get { return damage; }
        set { damage = value; }
    }

    private void Start()
    {
        Collider col = null;
        if ((col = GetComponent<Collider>()) == null) return;
        if (col as CapsuleCollider)
        {
            size = (col as CapsuleCollider).radius;
            return;
        }
        if (col as SphereCollider)
        {
            size = (col as SphereCollider).radius;
            return;
        }
    }

    public virtual int Simulate()
    {
        int simulation = -1;
        Vector3 dir = -transform.forward;
        Vector3 pos = transform.position + (transform.forward * 0.125f);
        if (Physics.Raycast(pos, dir, out var hit, 0.175f))
        {
            parentAdjuster = new GameObject().transform;
            parentAdjuster.position = hit.point;

            parentAdjuster.SetParent(hit.transform);
            transform.SetParent(parentAdjuster);
            float dis = Vector3.Distance(hit.point, pos) + 0.05f + size;
            if (Physics.SphereCast(pos, size, dir, out var hitDmg, dis, damageLayer))
            {
                DamageZone damaged = hitDmg.transform.GetComponent<DamageZone>();
                if (damaged != null)
                {
                    if (!damaged.DamageableAlreadyDead())
                    {
                        simulation = Mathf.Max(simulation, 0);
                        if (damaged.Damage(Damage, 1f))
                            simulation = 1;
                    }
                }
            }
        }
        return simulation;
    }

    public override void Pool()
    {
        transform.SetParent(parent);
        if(parentAdjuster != null)
            Destroy(parentAdjuster.gameObject);
        parentAdjuster = null;
        base.Pool();
    }
}
