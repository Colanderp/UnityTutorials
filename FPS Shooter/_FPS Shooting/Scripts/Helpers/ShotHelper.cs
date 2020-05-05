using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ShotHelper : PooledObject
{
    new Rigidbody rigidbody;
    protected UnityAction<Vector3, Vector3> onImpact;
    protected LayerMask dmgLayer;

    TrailRenderer tr;
    GunObject gunShotFrom;

    public void Initialize(Rigidbody body, GunObject gun, UnityAction<Vector3, Vector3> impactCall)
    {
        Unpool();
        gunShotFrom = gun;

        rigidbody = body;
        onImpact = impactCall;
        tr = GetComponentInChildren<TrailRenderer>();

        StartCoroutine(changeLayer());
        IEnumerator changeLayer()
        {
            TransformHelper.ChangeLayers(this.transform, "Overlay");
            yield return new WaitForSeconds(0.1f);
            TransformHelper.ChangeLayers(this.transform, "Default");
        }
    }

    public override void Pool()
    {
        rigidbody.velocity = Vector3.zero;
        if (tr != null)
            tr.Clear();
        base.Pool();
    }

    public override void Update()
    {
        base.Update();

        if (rigidbody)
            rigidbody.AddForce(transform.forward * gunShotFrom.additiveForce * Time.deltaTime, ForceMode.Impulse);
    }

    public virtual void OnCollisionEnter(Collision collision)
    {
        ContactPoint collisionPoint = collision.GetContact(0);
        onImpact.Invoke(collisionPoint.point, collisionPoint.normal);
        Pool();
    }
}
