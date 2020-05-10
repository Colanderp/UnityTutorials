using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class GunHandler : MonoBehaviour
{
    public enum GunStatus { idle, aiming, reloading, putAway, takingOut }
    public GunObject gun;
    public Transform handIKTarget;
    public GunBulletSpawn bulletSpawn;

    [Header("Handler Variables")]
    public GunStatus status;
    public int gunIndex = 0;

    [Header("Gun Variables")]
    public int ammoInClip;
    public int totalAmmo;
    public GunAnimations motions;

    Vector3 aimPos;
    Vector3 recoil;
    Animator animator;
    float adjustSpeed;

    UnityEvent onPutAway;
    TakeOutEvent onTakeOut;
    GunHandlerListener listener;

    bool forceReload = false;
    float reloadStartPoint = 0;

    Transform muzzleFlash;
    Func<ObjectPool> bulletPool;
    Func<ObjectPool> impactPool;
    public ObjectPool BulletPool
    {
        get { return bulletPool(); }
    }
    public ObjectPool ImpactPool
    {
        get { return impactPool(); }
    }

    public void SetBulletPool(Func<ObjectPool> pool)
    {
        bulletPool = pool;
    }

    public void SetImpactPool(Func<ObjectPool> pool)
    {
        impactPool = pool;
    }

    public void Initialize()
    {
        bulletPool = null;
        impactPool = null;
        onPutAway = new UnityEvent();
        onTakeOut = new TakeOutEvent();
        animator = GetComponent<Animator>();
        listener = GetComponentInChildren<GunHandlerListener>();
        if (listener) listener.Initialize(gun);

        if (gun.muzzleFlash == null) return;
        muzzleFlash = GameObject.Instantiate(gun.muzzleFlash, bulletSpawn.transform).transform;
        TransformHelper.ResetLocalTransform(muzzleFlash);
        muzzleFlash.gameObject.SetActive(false);
    }

    public void AddRecoil(Vector3 r)
    {
        recoil += (isAiming()) ? (r / 2f) : r;
    }

    private void Update()
    {
        transform.localRotation = Quaternion.Slerp(transform.localRotation, 
            (isAiming()) ? Quaternion.Euler(gun.ironSightAim.eulerAngles) : Quaternion.identity, 
            Time.deltaTime * 4f);
        aimPos = Vector3.Lerp(aimPos, (isAiming()) ? gun.ironSightAim.position : Vector3.zero, Time.deltaTime * gun.aimDownSpeed);
        recoil = Vector3.Lerp(recoil, Vector3.zero, Time.deltaTime * adjustSpeed);
        transform.localPosition = aimPos + recoil;
    }

    public bool ShootGun()
    {
        if (bulletSpawn) bulletSpawn.ShootOutSmoke();
        if (muzzleFlash) StartCoroutine(flashMuzzle());
        animator.Play(motions.Shoot(status == GunStatus.aiming), -1, 0);
        adjustSpeed = (isAiming()) ? 12f : Random.Range(6.0f, 12.0f);
        if (listener) listener.onShoot.Invoke();

        if (--ammoInClip <= 0)
            return false;
        else
            return true;
    }

    IEnumerator flashMuzzle()
    {
        Vector3 rot = Vector3.zero;
        rot.z = Random.Range(0f, 180f);
        muzzleFlash.localEulerAngles = rot;
        muzzleFlash.localScale = new Vector3(Random.Range(1.00f, 1.20f), Random.Range(1.00f, 1.20f), Random.Range(0.75f, 1.50f));
        muzzleFlash.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        muzzleFlash.gameObject.SetActive(false);
    }

    public void AimDownSights(bool aim)
    {
        if (status == GunStatus.reloading) return;
        status = (aim) ? GunStatus.aiming : GunStatus.idle;
    }

    public void TakeOutWeapon(UnityAction<int> call)
    {
        if (listener) listener.onTakeOut.Invoke();

        onTakeOut.RemoveAllListeners();
        onTakeOut.AddListener(call);
    }

    public void PutAwayWeapon(UnityAction call)
    {
        forceReload = (status == GunStatus.reloading || NoAmmo());
        status = GunStatus.putAway;
        animator.Play(motions.putAway, -1, 0);
        if (listener) listener.onPutAway.Invoke();

        onPutAway.RemoveAllListeners();
        onPutAway.AddListener(call);
    }

    public void GunIsAway()
    {
        onPutAway.Invoke();
    }

    public void GunIsOut()
    {
        onTakeOut.Invoke(forceReload ? gunIndex : -1);
    }

    public bool isAiming()
    {
        return (status == GunStatus.aiming);
    }

    public void SetAmmo()
    {
        ammoInClip = gun.ammoClip;
        if (gun.startingClips >= 0)
            totalAmmo = ammoInClip * gun.startingClips;
        else totalAmmo = 0;
    }

    public bool ReloadGun()
    {
        if (CantContinueShooting()) return false;

        status = GunStatus.reloading;
        animator.Play(motions.reload, -1, reloadStartPoint);
        if (gun.looseAmmoOnReload)
            ammoInClip = 0; 
        return true;
    }

    public float getReloadStartPoint()
    {
        return reloadStartPoint;
    }

    public void ReloadAmmo()
    {
        reloadStartPoint = 0;
        status = GunStatus.idle;
        totalAmmo += ammoInClip;
        totalAmmo -= gun.ammoClip;
        ammoInClip = gun.ammoClip;
        if (totalAmmo < 0)
        {
            ammoInClip += totalAmmo;
            totalAmmo = 0;
        }

        if (listener) listener.onReload.Invoke();
    }

    public void UpdateSprint(bool playerSprint)
    {
        bool viewSprint = playerSprint;
        if (status != GunStatus.idle)
            viewSprint = false;
        animator.SetBool("sprinting", viewSprint);
    }

    public void SetAnimations(GunAnimations animations)
    {
        if (motions == null) motions = new GunAnimations();
        motions.SetAnimations(animations);
    }

    public bool CantContinueShooting()
    {
        return (status == GunStatus.reloading || NoAmmo());
    }

    public bool NoAmmo()
    {
        return (totalAmmo <= 0 && ammoInClip <= 0);
    }

    public bool CanReload()
    {
        return ((int)status < 2 && ammoInClip < gun.ammoClip && totalAmmo > 0); // && !CantContinueShooting()); <- redundant because it is checked in ReloadGun()
    }

    public void SetReloadRestartPoint(float time)
    {
        reloadStartPoint = time;
    }

    public void OnDelayCall(float delayTime)
    {
        if (!listener) return;
        listener.onDelay.Invoke(delayTime);
    }
}

[System.Serializable]
public class GunAnimations
{
    public string idle = "idle";
    public string hipfireShoot = "shoot";
    public string aimedShoot = "aimedShoot";
    public string reload = "reload";
    public string putAway = "putAway";
    public string takeOut = "takeOut";

    public GunAnimations()
    {
    }

    public GunAnimations(string i, string h, string a, string r, string p, string t)
    {
        idle = i; 
        hipfireShoot = h; 
        aimedShoot = a; 
        reload = r; 
        putAway = p; 
        takeOut = t;
    }

    public void SetAnimations(GunAnimations ani)
    {
        idle = ani.idle; 
        hipfireShoot = ani.hipfireShoot; 
        aimedShoot = ani.aimedShoot; 
        reload = ani.reload; 
        putAway = ani.putAway; 
        takeOut = ani.takeOut;
    }

    public string Shoot(bool aiming)
    {
        return (aiming) ? aimedShoot : hipfireShoot;
    }
}
