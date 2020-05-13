using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotTrailHelper : PooledObject
{
    LineRenderer render;
    Vector3 shootDir;

    public override void Initialize()
    {
        base.Initialize();
        render = GetComponent<LineRenderer>();
        render.enabled = false;
    }

    public void Initialize(Vector3[] pos)
    {
        base.Initialize();
        Vector3 dir = (pos[1] - pos[0]).normalized;
        transform.rotation = Quaternion.LookRotation(dir);
        shootDir = dir;

        render.useWorldSpace = true;
        render.SetPositions(pos);
        render.enabled = true;

        StartCoroutine(changeLayer());
        IEnumerator changeLayer()
        {
            gameObject.layer = LayerMask.NameToLayer("Overlay");
            yield return new WaitForSeconds(0.1f);
            gameObject.layer = LayerMask.NameToLayer("Default");
        }
    }

    public override void Update()
    {
        base.Update();
        if (isInPool) return;
        if (render == null) return;
        Vector3 start = render.GetPosition(0);
        render.SetPosition(0, start + (shootDir * Time.deltaTime * 50f));
        float startZ = transform.InverseTransformPoint(render.GetPosition(0)).z;
        float endZ = transform.InverseTransformPoint(render.GetPosition(1)).z;
        if(startZ >= endZ)
        {
            render.enabled = false;
            Pool();
        }
    }
}
