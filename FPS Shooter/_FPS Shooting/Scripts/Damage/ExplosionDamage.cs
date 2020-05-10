using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionDamage : DamageObject
{
    public float radius = 3f;
    [Range(2, 32)]
    public int ringCount = 4;
    [Range(8, 64)]
    public int perRing = 16;
    [Range(1, 8)]
    public int hitsForFullDmg = 3;

    public override int Simulate()
    {
        int simulation = -1;
        float heightPerRing = radius / (float)ringCount;
        for(int i = 1; i <= ringCount; i++)
        {
            float h = (heightPerRing * i) - (heightPerRing / 2f);

            float height = radius - h;
            float ringRadius = Mathf.Sqrt(height * ((2 * radius) - height));

            float rotateAmount = 360f / (float)perRing;
            Vector3 ringDir = transform.TransformDirection(new Vector3(0, ringRadius, h));
            ringDir = Quaternion.AngleAxis(-i * (rotateAmount / ringCount), transform.forward) * ringDir;

            for (int j = 0; j < perRing; j++)
            {
                ringDir = Quaternion.AngleAxis(-rotateAmount, transform.forward) * ringDir;
                simulation = Mathf.Max(simulation, SendExplosionRays(ringDir));
            }
        }
        return simulation;
    }

    int SendExplosionRays(Vector3 ringDir)
    {
        return Mathf.Max(ExplosionRay(ringDir), ExplosionRay(-ringDir));
    }

    int ExplosionRay(Vector3 ringDir)
    {
        int simulation = -1;
        Vector3 pos = transform.position + (transform.forward * 0.125f);
        if (Physics.Raycast(pos, ringDir, out var hit, (radius - 0.125f)))
        {
            float dmg = Damage / (float)hitsForFullDmg;
            float dis = Vector3.Distance(hit.point, pos) + 0.05f;
            Rigidbody body = null;
            if ((body = hit.transform.GetComponent<Rigidbody>()) != null)
                body.AddForceAtPosition(ringDir * dmg, hit.point, ForceMode.Force);

            if (Physics.Raycast(pos, ringDir, out var hitDmg, dis, damageLayer))
            {
                Debug.DrawRay(pos, ringDir * dis, Color.red);
                DamageZone damaged = hitDmg.transform.GetComponent<DamageZone>();
                if (damaged != null)
                {
                    simulation = Mathf.Max(simulation, 0);
                    if (damaged.Damage(dmg, 1f))
                        simulation = 1;
                }
            }
        }
        return simulation;
    }
}
