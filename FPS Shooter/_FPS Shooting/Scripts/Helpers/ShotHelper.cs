using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ShotHelper : PooledObject
{
    float additiveForce = 0;
    new Rigidbody rigidbody;
    new Collider collider;
    UnityAction<Vector3, Vector3> onImpact;

    TrailRenderer tr;

    public void Initialize(Rigidbody body, float addForce, UnityAction<Vector3, Vector3> impactCall)
    {
        Unpool();
        rigidbody = body;
        onImpact = impactCall;
        tr = GetComponentInChildren<TrailRenderer>();

        additiveForce = addForce;
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
        additiveForce = 0f;
        rigidbody.velocity = Vector3.zero;
        if (tr != null)
            tr.Clear();
        base.Pool();
    }

    public override void Update()
    {
        base.Update();

        if (rigidbody)
            rigidbody.AddForce(transform.forward * additiveForce * Time.deltaTime, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint collisionPoint = collision.GetContact(0);
        onImpact.Invoke(collisionPoint.point, collisionPoint.normal);
        Pool();
    }
}
