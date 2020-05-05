using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowShot : ShotHelper
{
    public override void OnCollisionEnter(Collision collision)
    {
        ContactPoint collisionPoint = collision.GetContact(0);
        onImpact.Invoke(collisionPoint.point, -transform.forward);
        Pool();
    }
}
