using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class TakeOutEvent : UnityEvent<int> { }

[RequireComponent(typeof(HandSway))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(GunControllerHelper))]
public class GunController : MonoBehaviour
{
    public enum GunControlStatus { ready, aiming, swapping, takingOut, blocked, none }
    public Animator globalGunAni;
    public LayerMask damageLayer;
    public List<GunHandler> gunInventory = new List<GunHandler>();
    public GunControlStatus status;
    public LayerMask collisionLayer;
    public int selectedGun = -1;

    public bool toggleADS = false;
    [Range(1f, 2f)]
    public float actionADSMultiplier = 1.25f;
    [Range(0f, 1f)]
    public float crouchADSMultiplier = 0.75f;

    float fireTimer = 0;
    public float fireDelayTimer = 0;
    float crouchValue = 0f;
    public float bulletSpread = 0.01f;


    int continuousShots = 0;
    int semiCalculations = 16;
    int putAwayGun = -1;

    bool onShootUp = false;
    bool lastInput = false;
    bool shootingGun = false;
    List<int> autoReloadGun;

    AudioSource source;

    GunControllerHelper helper;
    GunControllerUI ui;
    GunHandler gunHandler;

    PlayerInput input;
    AnimateFOV animateFOV;
    CameraMovement cameraMovement;
    PlayerController player;
    HandSway weaponSwaying;

    Status playerStatus;

    Vector3 armPos;

    private void Start()
    {
        source = GetComponent<AudioSource>();
        helper = GetComponent<GunControllerHelper>();

        input = GetComponentInParent<PlayerInput>();
        animateFOV = GetComponentInParent<AnimateFOV>();
        cameraMovement = GetComponentInParent<CameraMovement>();
        player = GetComponentInParent<PlayerController>();
        player.AddToStatusChange(PlayerStatusChange);
        weaponSwaying = GetComponent<HandSway>();

        ui = FindObjectOfType<GunControllerUI>();
        if (ui == null) Debug.LogError("GunControllerUI not found, please add PlayerUI prefab");
        ResetGuns();
    }

    void ResetGuns()
    {
        autoReloadGun = new List<int>();

        status = GunControlStatus.ready;
        foreach (GunHandler g in gunInventory)
            g.gameObject.SetActive(false);

        if (gunSelected())
        {
            gunInventory[selectedGun].gameObject.SetActive(true);
            if(ui) ui.UpdateGunUI(SelectedGun());
        }
    }

    public void PlayerStatusChange(Status status, Func<IKData> call)
    {
        playerStatus = status;
    }

    public GunHandler SelectedGun()
    {
        return gunInventory[selectedGun];
    }

    private void Update()
    {
        GlobalAnimationHandler();
        gunHandler = (gunSelected()) ? SelectedGun() : null;

        AimingHandler();
        SpreadHandler();
        SprintHandler();
        FireDelayHandler();
        SwitchWeaponHandler();
        if ((int)status >= 2) return;

        FireGunHandler();
        ManualReloadHandler();
    }

    void GlobalAnimationHandler()
    {
        if (globalGunAni == null) return;
        crouchValue = Mathf.Lerp(crouchValue, (player.isCrouching() || isAiming()) ? 1f : 0f, Time.deltaTime * 2f);
        globalGunAni.SetBool("walking", player.isWalking());
        globalGunAni.SetFloat("crouch", crouchValue);
    }

    void SpreadHandler()
    {
        if (gunHandler)
        {
            GunObject gun = gunHandler.gun;
            float actualSpread = bulletSpread;
            float spread = CurrentBulletSpread();
            if (isAiming())
            {
                spread *= gun.aimSpreadMultiplier;
                actualSpread = Mathf.Lerp(actualSpread, spread, Time.deltaTime * gun.aimDownSpeed * (1f - gun.aimDownMultiplier));
            }
            else
                actualSpread = Mathf.Lerp(actualSpread, spread, Time.deltaTime * 4f);

            if (gunHandler.gun.canFireWhileDelayed) {
                float delaySpreadAdjust = (4f - (DelayPercent() * 3f)) / 4f;
                actualSpread = Mathf.Lerp(gunHandler.gun.bulletSpread, (spread * delaySpreadAdjust), DelayPercent());
            }
            bulletSpread = actualSpread;
        }
        else
            bulletSpread = 0.01f;
    }

    float DelayPercent()
    {
        return (fireDelayTimer / gunHandler.gun.fireDelay);
    }

    float CurrentBulletSpread()
    {
        float spread = gunHandler.gun.bulletSpread;
        if (playerBlocking() || playerStatus == Status.sprinting)
            spread *= actionADSMultiplier;
        else if (playerStatus == Status.crouching)
            spread *= crouchADSMultiplier;

        return spread;
    }

    void SprintHandler()
    {
        if (gunHandler)
            gunHandler.UpdateSprint(player.isSprinting());
    }

    void FireDelayHandler()
    {
        if (!gunHandler)
        {
            fireDelayTimer = 0;
            return;
        }

        float adjust = (gunCanShoot() && input.shooting) ? Time.deltaTime : -(Time.deltaTime * gunHandler.gun.fireCooldownSpeed);
        fireDelayTimer = Mathf.Clamp(fireDelayTimer + adjust, 0, gunHandler.gun.fireDelay);
    }

    void AimingHandler()
    {
        if (gunHandler)
        {
            if ((int)status < 2)
            {
                if ((int)gunHandler.status < 2 && !playerBlocking() && playerStatus != Status.sprinting)
                {
                    if (toggleADS && input.aim)
                        status = (status == GunControlStatus.aiming) ? GunControlStatus.ready : GunControlStatus.aiming;
                    else if (!toggleADS)
                        status = (input.aiming) ? GunControlStatus.aiming : GunControlStatus.ready;
                }
                else
                    status = GunControlStatus.ready;
            }

            AdjustFOV(isAiming());
            gunHandler.AimDownSights(isAiming());
            if(ui) ui.SetCrosshair(isAiming() ? 0.01f : bulletSpread, isAiming());
            weaponSwaying.SetSwayMultiplier(isAiming() ? gunHandler.gun.aimDownMultiplier : 1f);
        }
        else
        {
            status = GunControlStatus.none;
            weaponSwaying.SetSwayMultiplier(1f);
            if (ui) ui.SetCrosshair(0.01f, true);
        }
    }


    bool playerBlocking()
    {
        return ((int)playerStatus >= 4);
    }

    bool gunCanShoot()
    {
        if (!gunHandler) return false;
        if (gunHandler.status == GunHandler.GunStatus.reloading) return false;
        if (playerBlocking())
            return gunHandler.gun.canFireWhileActing;
        else
            return true;
    }

    void HandleOnShootUp()
    {
        if (input.shooting)
            lastInput = true;
        else if (lastInput)
        {
            onShootUp = true;
            lastInput = false;
        }
        else
            onShootUp = false;
    }

    void FireGunHandler()
    {
        GunObject gun = gunHandler.gun;
        HandleOnShootUp();

        gunHandler.OnDelayCall(fireDelayTimer);

        if (fireTimer > 0)
            fireTimer -= Time.deltaTime;
        else if(gunHandler)
        {

            if (!shootingGun)
            {
                if (autoReloadGun.Count > 0 && autoReloadGun.Contains(gunHandler.gunIndex))
                {
                    if (playerBlocking()) return;
                    if (!gunHandler.CanReload()) return;
                    if (gunHandler.status == GunHandler.GunStatus.reloading) return;
                    if (ReloadSelectedGun())
                        autoReloadGun.Remove(gunHandler.gunIndex);
                    return;
                }

                if (fireDelayTimer < gunHandler.gun.fireDelay)
                {
                    if (!gun.canFireWhileDelayed) return;
                }

                if (!gunCanShoot())
                    return;

                if (gun.shooting == GunObject.ShootType.auto && input.shooting)
                {
                    continuousShots++;
                    FireGun();
                }
                else
                {
                    bool fire = false;
                    if (gun.fireWhenPressedUp)
                    {
                        if (onShootUp) fire = true;
                    }
                    else if(input.shooting) fire = true;

                    if (fire)
                    {
                        continuousShots = 0;
                        FireGun();
                    }
                }
            }
        }
    }

    void ManualReloadHandler()
    {
        if (shootingGun) return; //If we are shooting, do nothing
        if (!gunHandler) return; //If we don't have a gun, do nothing
        if (playerBlocking()) return;

        if (!input.reload) return; //If we don't press R this frame, do nothing
        if (gunHandler.CanReload())
            ReloadSelectedGun();
    }

    bool ReloadSelectedGun()
    {
        GunObject gun = gunHandler.gun;
        if (gun.reloadSFX != null)
        {
            source.clip = gun.reloadSFX;
            source.time = gun.reloadSFX.length * gunHandler.getReloadStartPoint();
            source.Play();
        }
        return gunHandler.ReloadGun();
    }

    void SwitchWeaponHandler()
    {
        if (shootingGun || playerBlocking()) return;
        if (gunSelected())
        {
            float scroll = input.mouseScroll;
            if (scroll >= 0.1f) ChangeSelectedGun(1);
            else if (scroll <= -0.1f) ChangeSelectedGun(-1);
        }
    }

    void ChangeSelectedGun(int add)
    {
        if (status != GunControlStatus.swapping)
        {
            source.Stop(); //Stop reloading SFX if playing
            fireDelayTimer = 0; //Reset the fire delay
            SelectedGun().PutAwayWeapon(() => TakeOutSelectedGun());
            status = GunControlStatus.swapping;
            putAwayGun = selectedGun;
        }

        selectedGun += add;
        if (selectedGun < 0)
            selectedGun += gunInventory.Count;
        else if (selectedGun >= gunInventory.Count)
            selectedGun -= gunInventory.Count;
    }

    public void TakeOutSelectedGun()
    {
        if (putAwayGun >= 0)
        {
            gunInventory[putAwayGun].gameObject.SetActive(false);
            putAwayGun = -1;
        }

        if (ui) ui.UpdateGunUI(gunHandler);
        gunHandler.TakeOutWeapon(TakenGunOut);
        gunInventory[selectedGun].gameObject.SetActive(true);
        status = GunControlStatus.takingOut;
    }

    public void TakenGunOut(int reloadIndex)
    {
        status = GunControlStatus.ready;
        if (reloadIndex >= 0)
            AutoReloadGun(reloadIndex);
    }

    void AdjustFOV(bool aiming)
    {
        GunObject curGun = SelectedGun().gun;
        float fov = SelectedGun().gun.aimFOV;
        animateFOV.SetFOV(aiming, curGun.aimFOV, curGun.aimDownSpeed);
    }

    public bool isAiming()
    {
        return (status == GunControlStatus.aiming);
    }
    public bool gunSelected()
    {
        return (selectedGun >= 0 && selectedGun < gunInventory.Count);
    }

    void FireGun()
    {
        if (gunHandler.status == GunHandler.GunStatus.reloading || gunHandler.CantContinueShooting())
            return;
        
        shootingGun = true;
        GunObject gun = gunHandler.gun;
        fireTimer = gun.firerate;

        switch (gun.shooting)
        {
            case GunObject.ShootType.semi:
                PlayShotSFX();
                float addTime = gun.firerate / (float)semiCalculations;
                StartCoroutine(singleShot());
                if (!gunHandler.ShootGun())
                    AutoReloadGun(gunHandler.gunIndex);
                IEnumerator singleShot()
                {
                    SimulateShot();
                    for (int i = 0; i < semiCalculations; i++)
                    {
                        continuousShots++;
                        ApplyRecoil(addTime);
                        yield return new WaitForSeconds(addTime);
                    }
                    shootingGun = false;
                }
                fireDelayTimer = 0; //Restart the timer if semi or burst
                break;
            case GunObject.ShootType.auto:
                PlayShotSFX();
                SimulateShot();
                ApplyRecoil(gun.firerate);
                if (!gunHandler.ShootGun())
                    AutoReloadGun(gunHandler.gunIndex);
                shootingGun = false;
                break;
            case GunObject.ShootType.burst:
                PlayShotSFX();
                float shotTime = gun.burstTime / (float)gun.burstShot;
                StartCoroutine(burstShot());
                IEnumerator burstShot()
                {
                    for (int i = 0; i < gun.burstShot; i++)
                    {
                        SimulateShot();
                        continuousShots++;
                        ApplyRecoil(shotTime);
                        if (!gunHandler.ShootGun())
                        {   //Stop when we run out of bullets
                            AutoReloadGun(gunHandler.gunIndex);
                            yield return null;
                        }
                        else
                            yield return new WaitForSeconds(shotTime);
                    }
                    shootingGun = false;
                }
                fireDelayTimer = 0; //Restart the timer if semi or burst
                break;
        }
    }

    void PlayShotSFX()
    {
        GunObject gun = gunHandler.gun;
        if (source == null || gun.shootClips.Length == 0) return;
        source.PlayOneShot(gun.GetRandomShootSFX());
    }

    void AutoReloadGun(int index)
    {
        if (gunInventory[index].gun.startingClips < 0) return; //We won't reload a gun that cannot reload

        if (!autoReloadGun.Contains(index))
            autoReloadGun.Add(index);
    }

    void SimulateShot()
    {
        GunObject gun = gunHandler.gun;

        if (gun.shootType == GunObject.GunType.raycast)
        {
            for (int i = 0; i < gun.bulletsPerShot; i++)
                RaycastShot();
        }
        else
        {
            for(int i = 0; i < gun.bulletsPerShot; i++)
                RigidbodyShot();
        }
    }

    Vector3 getShotDir(float spread)
    {
        Vector3 shotDir = Vector3.zero;
        shotDir.x = Random.Range(-spread, spread);
        shotDir.y = Random.Range(-spread, spread);
        shotDir.z = 1f;
        return shotDir;
    }

    void RaycastShot()
    {
        Transform camera = cameraMovement.transform; //Shoot from camera, not gun

        GunObject gun = gunHandler.gun;

        Vector3 shotDir = getShotDir(bulletSpread);
        Vector3 worldDir = camera.TransformDirection(shotDir);
        Vector3 impactPos = camera.position;
        impactPos += worldDir * gun.bulletRange;

        Ray shotRay = new Ray(camera.position, worldDir);
        if (Physics.Raycast(shotRay, out var hit, gun.bulletRange, collisionLayer))
        {
            DamageZone damaged = null;
            float dis = Vector3.Distance(camera.position, hit.point) + 0.05f;
            if (Physics.Raycast(shotRay, out var dmg, dis, damageLayer))
                damaged = dmg.transform.GetComponent<DamageZone>();

            impactPos = hit.point;
            CreateImpact(impactPos, hit.normal);
            if (damaged != null) //If we hit something we should damage
            {
                if (!damaged.DamageableAlreadyDead())
                {
                    bool killed = damaged.Damage(gun.bulletDamage, gun.headshotMult);
                    if(ui) ui.ShowHitmarker(killed); //Damages and shows hitmarker
                }
            }
        }

        Vector3[] pos = { gunHandler.bulletSpawn.transform.position, impactPos };
        ShotTrailHelper trail = helper.getAvailableTrail();
        trail.Initialize(pos);
    }

    void RigidbodyShot()
    {
        Transform camera = cameraMovement.transform; //Shoot from camera, not gun

        GunObject gun = gunHandler.gun;

        if (gun.rigidbodyBullet == null || gunHandler.BulletPool == null) return;
        Vector3 spawnPos = gunHandler.bulletSpawn.transform.position;
        Vector3 shotDir = getShotDir(bulletSpread);
        Vector3 worldDir = camera.TransformDirection(shotDir);

        //Get bullet from pool
        PooledObject bullet = gunHandler.BulletPool.get();
        bullet.transform.position = spawnPos;
        bullet.transform.rotation = Quaternion.LookRotation(worldDir);

        if (bullet as DamageObject)
            (bullet as DamageObject).Damage = gun.bulletDamage;

        Rigidbody bulletBody = null;
        if ((bulletBody = bullet.transform.GetComponent<Rigidbody>()) == null) return;

        ShotHelper shotHelper = null;
        if((shotHelper = bullet as ShotHelper) != null)
            shotHelper.Initialize(bulletBody, gun, CreateImpact);

        float force = gun.initialForce;
        if (gunHandler.gun.canFireWhileDelayed)
            force *= DelayPercent();

        bulletBody.AddForce(worldDir * force, ForceMode.Impulse);
    }

    void ApplyRecoil(float overTime)
    {
        GunObject gun = gunHandler.gun;

        float aimAdjust = (isAiming()) ? gun.aimDownMultiplier : 1f;

        Vector3 shotRecoil = Vector3.zero;
        if (gun.shooting == GunObject.ShootType.auto)
            shotRecoil = gun.recoil.GetRecoil(continuousShots, gun.ammoClip, gun.cyclesInClip);
        else if (gun.shooting == GunObject.ShootType.burst)
            shotRecoil = gun.recoil.GetRecoil(continuousShots, gun.burstShot, 1);
        else if(gun.shooting == GunObject.ShootType.semi)
            shotRecoil = gun.recoil.GetRecoil(continuousShots, semiCalculations);

        shotRecoil *= aimAdjust;
        cameraMovement.AddRecoil(shotRecoil, overTime);

        shotRecoil.y = 0;
        float z = shotRecoil.z;
        shotRecoil.x = gun.recoil.xRecoil.EvaluteValue(Random.Range(0.0f, 1.0f));
        shotRecoil.x *= aimAdjust;
        shotRecoil *= overTime;
        shotRecoil.z = z;

        GunRecoil(shotRecoil, overTime);
    }

    void CreateImpact(Vector3 pos, Vector3 normal)
    {
        GunObject gun = gunHandler.gun;
        if (gun.impactEffect == null || gunHandler.ImpactPool == null) return;

        PooledObject impact = gunHandler.ImpactPool.get();
        impact.transform.position = pos;
        impact.transform.rotation = Quaternion.LookRotation(normal);

        if (!(impact as DamageObject)) return;
        
        DamageObject dmgImpact = (impact as DamageObject);
        dmgImpact.Damage = gun.bulletDamage;
        int simulation = dmgImpact.Simulate();

        if (simulation < 0 || !ui) return;
        ui.ShowHitmarker(simulation > 0); //Damages and shows hitmarker
    }

    void GunRecoil(Vector3 recoil, float time)
    {
        float recoilElapsed = 0;
        StartCoroutine(recoilIncrease());
        IEnumerator recoilIncrease()
        {
            while (recoilElapsed < time)
            {
                recoilElapsed += Time.deltaTime;
                gunHandler.AddRecoil(recoil * Time.deltaTime);
                yield return null;
            }
        }
    }

    public void RefillAmmo()
    {
        foreach(GunHandler handler in gunInventory)
        {
            GunObject gun = handler.gun;
            if (gun.startingClips >= 0)
                handler.totalAmmo = gun.ammoClip * gun.startingClips;
            else
                handler.ammoInClip = gun.ammoClip;
        }
    }

    [ExecuteInEditMode]
    void RemoveBlanks()
    {
        List<int> remove = new List<int>();
        for(int i = 0; i < gunInventory.Count; i++)
        {
            if (gunInventory[i] == null)
                remove.Add(i);
        }
        int deleted = 0;
        for(int i = 0; i < remove.Count; i++)
        {
            if (remove[i] <= selectedGun)
                selectedGun--;
            gunInventory.RemoveAt(remove[i] - deleted);
            deleted++;
        }
        for (int i = 0; i < gunInventory.Count; i++)
            gunInventory[i].gunIndex = i;
    }

#if UNITY_EDITOR
    static readonly string playerPrefabPath = "Assets/_FPS Shooting/ShooterPlayer.prefab";
    //Return the GunHandler if we already have the gun, otherwise return null
    [ExecuteInEditMode]
    public void AddGun(GunObject addGun)
    {
        if (Application.isPlaying){
            Debug.LogWarning("ONLY CALL THIS OUTSIDE OF PLAY");
            return;
        }
        RemoveBlanks();
        //Find where to place this gun
        PrefabHandler playerPrefab = new PrefabHandler(FindObjectOfType<PlayerController>().transform, playerPrefabPath);
        GunHandler handler = gunInventory.Find(x => x.gun.GetHashCode() == addGun.GetHashCode());
        if (handler == null) //If we did not find a handler with the gun using the gun name
        {
            playerPrefab.ChangePrefab(() =>
            {
                GameObject gunParent = new GameObject();
                gunParent.transform.name = addGun.prefabName;
                gunParent.transform.SetParent(this.transform);
                TransformHelper.ResetLocalTransform(gunParent.transform);

                Animator ani = gunParent.AddComponent(typeof(Animator)) as Animator;
                if (addGun.animationController != null)
                    ani.runtimeAnimatorController = addGun.animationController;

                handler = gunParent.AddComponent(typeof(GunHandler)) as GunHandler;

                handler.gun = addGun;
                handler.handIKTarget = new GameObject().transform;
                handler.handIKTarget.name = "IK_Hand";
                handler.handIKTarget.SetParent(gunParent.transform);
                TransformHelper.SetLocalTransformData(handler.handIKTarget, addGun.IK_HandTarget);

                gunInventory.Add(handler);
                handler.gunIndex = gunInventory.Count - 1;
            });
        }
        else
        {
            Debug.Log("Already have gun named : " + addGun.prefabName + " [UPDATING GUN]");

            playerPrefab.ChangePrefab(() =>
            {
                handler.transform.SetParent(this.transform);
                TransformHelper.ResetLocalTransform(handler.transform);

                if (addGun.animationController != null)
                {
                    Animator ani = handler.gameObject.GetComponent<Animator>();
                    ani.runtimeAnimatorController = addGun.animationController;
                }

                handler.gun = addGun;
                handler.handIKTarget.SetParent(handler.transform.parent);
                TransformHelper.DeleteAllChildren(handler.transform);
                handler.handIKTarget.SetParent(handler.transform);
                TransformHelper.SetLocalTransformData(handler.handIKTarget, addGun.IK_HandTarget);
            });
        }

        playerPrefab.ChangePrefab(() =>
        {
            if (addGun.prefabObj != null)
                CreateGunPrefab(handler);
            if (addGun.animationController != null)
                handler.SetAnimations(addGun.gunMotions);

            selectedGun = handler.gunIndex;
            for(int i = 0; i < gunInventory.Count; i++)
                gunInventory[i].gameObject.SetActive(i == selectedGun);

            handler.SetAmmo();
            handler.bulletSpawn = handler.gameObject.GetComponentInChildren<GunBulletSpawn>();

            InverseKinematics ik = null;
            if ((ik = FindObjectOfType<ArmIKController>().armIK) != null)
                ik.target = handler.handIKTarget;
        });

        playerPrefab.RecreatePrefab();
    }
#endif

    public void AddGunTemporarily(GunObject addGun) //Will not change the prefab, this should be called if the game is running
    {
        RemoveBlanks();
        //Find where to place this gun
        GunHandler handler = gunInventory.Find(x => x.gun.GetHashCode() == addGun.GetHashCode());
        if (handler == null) //If we did not find a handler with the gun using the gun name
        {
            GameObject gunParent = new GameObject();
            gunParent.transform.name = addGun.prefabName;
            gunParent.transform.SetParent(this.transform);
            TransformHelper.ResetLocalTransform(gunParent.transform);

            Animator ani = gunParent.AddComponent(typeof(Animator)) as Animator;
            if (addGun.animationController != null)
                ani.runtimeAnimatorController = addGun.animationController;

            handler = gunParent.AddComponent(typeof(GunHandler)) as GunHandler;

            handler.gun = addGun;
            handler.handIKTarget = new GameObject().transform;
            handler.handIKTarget.name = "IK_Hand";
            handler.handIKTarget.SetParent(gunParent.transform);
            TransformHelper.SetLocalTransformData(handler.handIKTarget, addGun.IK_HandTarget);

            gunInventory.Add(handler);
            handler.gunIndex = gunInventory.Count - 1;
        }
        else
        {
            Debug.Log("Already have gun named : " + addGun.prefabName + " [UPDATING GUN]");

            handler.transform.SetParent(this.transform);
            TransformHelper.ResetLocalTransform(handler.transform);

            if (addGun.animationController != null)
            {
                Animator ani = handler.gameObject.GetComponent<Animator>();
                ani.runtimeAnimatorController = addGun.animationController;
            }

            handler.gun = addGun;
            handler.handIKTarget.SetParent(handler.transform.parent);
            TransformHelper.DeleteAllChildren(handler.transform);
            handler.handIKTarget.SetParent(handler.transform);
            TransformHelper.SetLocalTransformData(handler.handIKTarget, addGun.IK_HandTarget);
        }

        if (addGun.prefabObj != null)
            CreateGunPrefab(handler);
        if (addGun.animationController != null)
            handler.SetAnimations(addGun.gunMotions);

        handler.SetAmmo();
        handler.bulletSpawn = handler.gameObject.GetComponentInChildren<GunBulletSpawn>();
        helper.InitializeGun(handler);
        handler.gameObject.SetActive(false);

        //Swap to new gun
        putAwayGun = selectedGun;
        source.Stop(); //Stop reloading SFX if playing
        fireDelayTimer = 0; //Reset the fire delay
        SelectedGun().PutAwayWeapon(() => TakeOutSelectedGun());
        selectedGun = handler.gunIndex;
        status = GunControlStatus.swapping;
    }

    [ExecuteInEditMode]
    void CreateGunPrefab(GunHandler handler)
    {
        GameObject gun = GameObject.Instantiate(handler.gun.prefabObj);
        gun.transform.SetParent(handler.transform);
        TransformHelper.SetLocalTransformData(gun.transform, handler.gun.prefabLocalData);
    }
}