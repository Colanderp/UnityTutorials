using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunControllerHelper : MonoBehaviour
{
    public string trailLayer = "Overlay"; //Change this to what your weapon layer is to display on top
    public List<ObjectPool> bulletPools;
    public List<ObjectPool> impactPools;
    ObjectPool trailPool;

    GunController controller;
    LineRenderer cloneSettings;
    OverlayAdjuster overlayAdjuster;
    ArmIKController armIK;

    void Start()
    {
        bulletPools = new List<ObjectPool>();
        impactPools = new List<ObjectPool>();
        cloneSettings = GetComponent<LineRenderer>();
        trailPool = new ObjectPool(createTrail());
        cloneSettings.enabled = false;

        controller = GetComponent<GunController>();
        foreach (GunHandler g in controller.gunInventory)
        {
            GunObject gun = g.gun;
            g.gameObject.SetActive(true);
            g.Initialize();

            if(gun.shootType == GunObject.GunType.rigidbody && gun.rigidbodyBullet)
            {
                ObjectPool bulletPool = getFromPool(gun, gun.rigidbodyBullet, ref bulletPools);
                g.SetBulletPool(bulletPool.getPool);
            }

            if (gun.impactEffect == null) continue;
            ObjectPool impactPool = getFromPool(gun, gun.impactEffect, ref impactPools);
            g.SetImpactPool(impactPool.getPool);
        }

        if ((overlayAdjuster = GetComponentInParent<OverlayAdjuster>()) != null)
            overlayAdjuster.GetScopes();
    }

    ObjectPool getFromPool(GunObject gun, PooledObject target, ref List<ObjectPool> fromPool)
    {
        ObjectPool pool = null;
        if ((pool = fromPool.Find(x => (x.pooledObj.GetHashCode() == target.GetHashCode()))) == null)
        {
            pool = new ObjectPool(target);
            fromPool.Add(pool);
        }
        return pool;
    }

    public ShotTrailHelper getAvailableTrail()
    {
        return trailPool.get() as ShotTrailHelper;
    }

    ShotTrailHelper createTrail()
    {
        GameObject trail = new GameObject();
        trail.layer = LayerMask.NameToLayer(trailLayer);

        trail.transform.name = "ShootTrail";
        LineRenderer line = trail.AddComponent<LineRenderer>() as LineRenderer;
        if(cloneSettings != null)
        {
            line.material = cloneSettings.material;
            line.numCapVertices = cloneSettings.numCapVertices;
            line.widthCurve = cloneSettings.widthCurve;
            line.startColor = cloneSettings.startColor;
            line.endColor = cloneSettings.endColor;
        }

        ShotTrailHelper helper = trail.AddComponent<ShotTrailHelper>() as ShotTrailHelper;
        helper.Initialize();
        return helper;
    }
}
