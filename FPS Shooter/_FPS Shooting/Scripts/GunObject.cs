using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[CreateAssetMenu(fileName = "GunObject", menuName = "Gun Object", order = 1)]
public class GunObject : ScriptableObject
{
    public enum GunType { raycast, rigidbody }; 
    public enum ShootType { semi, auto, burst };

    public string prefabName;
    public GameObject prefabObj;
    public TransformData prefabLocalData;

    //[Header("IK Variables")]
    public TransformData IK_HandTarget;

    //[Header("Shoot Process")]
    public GunType shootType;
    public GameObject muzzleFlash;
    public PooledObject impactEffect;
    public PooledObject rigidbodyBullet;
    public float initialForce = 100f;
    public float additiveForce = 10f;

    //[Header("Gun Variables")]
    public ShootType shooting;
    public int bulletsPerShot = 1;
    public int burstShot = 3;
    public float burstTime = 0.125f;
    
    public int ammoClip = 12;
    public int startingClips = 3;
    public float firerate = 0.25f;
    public float fireDelay = 0f; //0 to shoot instantly
    [Range(0f, 2f)]
    public float fireCooldownSpeed = 1f; //speed in which the gun cools down
    public bool canFireWhileDelayed = false; //Shoot while delay is active (affects spread)
    public bool looseAmmoOnReload = false;
    public bool canFireWhileActing = true;
    public bool fireWhenPressedUp = false; //Only for burst and semi

    //[Header("Aim Variables")]
    public float aimFOV = 60f;
    public float aimDownSpeed = 8f;
    public TransformData ironSightAim;

    //[Header("Damage Variables")]
    public float bulletDamage = 10f;
    [Range(1f, 2f)]
    public float headshotMult = 1.25f;

    //[Header("Bullet Variables")]
    public float bulletRange = 100f;
    [Range(0.01f, 0.25f)]
    public float bulletSpread = 0.1f;
    [Range(0.0f, 1.0f)]
    public float aimSpreadMultiplier = 0.2f;

    //[Header("Recoil Variables")]
    [Range(0f, 1f)]
    public float aimDownMultiplier = 0.5f;
    public int cyclesInClip = 1;
    public GunRecoil recoil;

    //[Header("Animation Variables")]
    public RuntimeAnimatorController animationController;
    public GunAnimations gunMotions;

    //[Header("UI Variables")]
    public Sprite gunIcon;
    public float ammoOffsetX = 180f;

    //[Header("SFX Variables")]
    public AudioClip[] shootClips;
    public AudioClip reloadSFX;
    void OnValidate()
    {
        fireDelay = Mathf.Max(fireDelay, 0); //Force fireDelay to be positive
        startingClips = Mathf.Max(startingClips, -1); //Force startingClips to higher than or equal to -1
    }

    public AudioClip GetRandomShootSFX()
    {
        if (shootClips.Length == 0) return null;
        return shootClips[Random.Range(0, shootClips.Length)];
    }

    public void AddTempGunToPlayer() //The difference here is that it will not change the ArmIk or Prefab varaibles
    {
        GunController controller = null;
        if ((controller = FindObjectOfType<GunController>()) != null)
            controller.AddGunTemporarily(this);
    }

#if UNITY_EDITOR
    public void GivePlayerGun()
    {
        GunController controller = null;
        if((controller = FindObjectOfType<GunController>()) != null)
            controller.AddGun(this);
    }
#endif
}
[System.Serializable]
public class GunRecoil
{
    public RecoilValue xRecoil;
    public RecoilValue yRecoil;
    public RecoilValue zRecoil;

    [Range(0f, 1f)]
    public float randomizeRecoil = 0;

    public Vector3 GetRecoil(int shot, int max, int cyclesInClip)
    {
        int shotsPerCycle = (max / cyclesInClip);
        int cycle = (shot / shotsPerCycle);
        int numberInCycle = shot - (cycle * shotsPerCycle);
        float percent = (float)numberInCycle / (float)shotsPerCycle;

        float random = Random.Range(-randomizeRecoil, randomizeRecoil) / (float)cyclesInClip;
        percent = Mathf.Clamp(percent + random, 0f, 1f);
        return RecoilValue(percent);
    }

    public Vector3 GetRecoil(int shot, int max)
    {
        float percent = (float)shot / (float)max;
        float random = Random.Range(-randomizeRecoil, randomizeRecoil);
        percent = Mathf.Clamp(percent + random, 0f, 1f);
        return RecoilValue(percent);
    }

    Vector3 RecoilValue(float percent)
    {
        return new Vector3(xRecoil.EvaluteValue(percent),
                            yRecoil.EvaluteValue(percent),
                            zRecoil.EvaluteValue(percent));
    }
}

[System.Serializable]
public class RecoilValue
{
    public AnimationCurve graph;
    public float multiplier = 1;

    public float EvaluteValue(float time)
    {
        return graph.Evaluate(time) * multiplier;
    }
}