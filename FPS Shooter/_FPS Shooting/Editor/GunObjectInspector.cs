using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;

[CustomEditor(typeof(GunObject))]
public class GunObjectInspector : Editor
{
    private SerializedProperty prefabName;
    private SerializedProperty prefabObj;
    private SerializedProperty prefabLocalData;

    private SerializedProperty shootType;
    private SerializedProperty rigidbodyBullet;
    private SerializedProperty impactEffect;
    private SerializedProperty initialForce;
    private SerializedProperty additiveForce;
    private SerializedProperty muzzleFlash;

    private SerializedProperty shooting;
    private SerializedProperty firerate;

    private SerializedProperty fireDelay;
    private SerializedProperty fireCooldownSpeed;
    private SerializedProperty canFireWhileDelayed;

    private SerializedProperty bulletsPerShot;
    private SerializedProperty fireWhenPressedUp;

    private SerializedProperty burstShot;
    private SerializedProperty burstTime;

    private SerializedProperty ammoClip;
    private SerializedProperty startingClips;
    private SerializedProperty looseAmmoOnReload;
    private SerializedProperty canFireWhileActing;

    private SerializedProperty bulletDamage;
    private SerializedProperty headshotMult;

    private SerializedProperty bulletRange;
    private SerializedProperty bulletSpread;
    private SerializedProperty aimSpreadMultiplier;

    private SerializedProperty aimFOV;
    private SerializedProperty aimDownSpeed;
    private SerializedProperty ironSightAim;

    private SerializedProperty aimDownMultiplier;
    private SerializedProperty cyclesInClip;
    private SerializedProperty recoil;


    private SerializedProperty animationController;
    private SerializedProperty gunMotions;

    private SerializedProperty IK_HandTarget; 

    private SerializedProperty gunIcon; 
    private SerializedProperty ammoOffsetX;

    private SerializedProperty shootClips;
    private SerializedProperty reloadSFX;
    GunObject gun;

    protected static bool showIKVariables = true;
    protected static bool showShootProcess = true;
    protected static bool showGunVariables = true;
    protected static bool showAmmoVariables = true;
    protected static bool showDamageVariables = true;
    protected static bool showAimVariables = true;
    protected static bool showBulletVariables = true;
    protected static bool showRecoilVariables = true;
    protected static bool showAnimationVariables = true;
    protected static bool showUIVariables = true;
    protected static bool showSFXVariables = true;

    private void OnEnable()
    {
        gun = (GunObject)target;

        prefabName = this.serializedObject.FindProperty("prefabName");
        prefabObj = this.serializedObject.FindProperty("prefabObj");
        prefabLocalData = this.serializedObject.FindProperty("prefabLocalData");

        shootType = this.serializedObject.FindProperty("shootType");
        rigidbodyBullet = this.serializedObject.FindProperty("rigidbodyBullet");
        impactEffect = this.serializedObject.FindProperty("impactEffect");
        initialForce = this.serializedObject.FindProperty("initialForce");
        additiveForce = this.serializedObject.FindProperty("additiveForce");

        muzzleFlash = this.serializedObject.FindProperty("muzzleFlash");

        shooting = this.serializedObject.FindProperty("shooting");
        firerate = this.serializedObject.FindProperty("firerate");

        fireDelay = this.serializedObject.FindProperty("fireDelay");
        fireCooldownSpeed = this.serializedObject.FindProperty("fireCooldownSpeed");
        canFireWhileDelayed = this.serializedObject.FindProperty("canFireWhileDelayed");
        
        bulletsPerShot = this.serializedObject.FindProperty("bulletsPerShot");
        fireWhenPressedUp = this.serializedObject.FindProperty("fireWhenPressedUp");

        burstShot = this.serializedObject.FindProperty("burstShot");
        burstTime = this.serializedObject.FindProperty("burstTime");

        ammoClip = this.serializedObject.FindProperty("ammoClip");
        startingClips = this.serializedObject.FindProperty("startingClips");
        looseAmmoOnReload = this.serializedObject.FindProperty("looseAmmoOnReload");
        canFireWhileActing = this.serializedObject.FindProperty("canFireWhileActing");

        bulletDamage = this.serializedObject.FindProperty("bulletDamage");
        headshotMult = this.serializedObject.FindProperty("headshotMult");

        bulletRange = this.serializedObject.FindProperty("bulletRange");
        bulletSpread = this.serializedObject.FindProperty("bulletSpread");
        aimSpreadMultiplier = this.serializedObject.FindProperty("aimSpreadMultiplier");

        aimFOV = this.serializedObject.FindProperty("aimFOV");
        aimDownSpeed = this.serializedObject.FindProperty("aimDownSpeed");
        ironSightAim = this.serializedObject.FindProperty("ironSightAim");

        aimDownMultiplier = this.serializedObject.FindProperty("aimDownMultiplier");
        cyclesInClip = this.serializedObject.FindProperty("cyclesInClip");
        recoil = this.serializedObject.FindProperty("recoil");

        animationController = this.serializedObject.FindProperty("animationController");
        gunMotions = this.serializedObject.FindProperty("gunMotions");

        IK_HandTarget = this.serializedObject.FindProperty("IK_HandTarget");

        gunIcon = this.serializedObject.FindProperty("gunIcon");
        ammoOffsetX = this.serializedObject.FindProperty("ammoOffsetX");

        shootClips = this.serializedObject.FindProperty("shootClips");
        reloadSFX = this.serializedObject.FindProperty("reloadSFX");
    }

    public override void OnInspectorGUI()
    {
        GUIStyle myFoldoutStyle = new GUIStyle();
        myFoldoutStyle.fontStyle = FontStyle.Bold;
        myFoldoutStyle.contentOffset = new Vector2(16, 0);

        GUIStyle foldoutContent = EditorStyles.helpBox;

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.BeginVertical(foldoutContent);
        EditorGUILayout.Foldout(true, "Base Settings", myFoldoutStyle);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(prefabName);
        EditorGUILayout.PropertyField(prefabObj);
        EditorGUILayout.PropertyField(prefabLocalData);
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();

        GUILayout.Space(8);

        EditorGUILayout.BeginVertical(foldoutContent);
        showIKVariables = EditorGUILayout.Foldout(showIKVariables, "IK Settings", myFoldoutStyle);
        if (showIKVariables)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(IK_HandTarget);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();

        GUILayout.Space(8);

        EditorGUILayout.BeginVertical(foldoutContent);
        showAnimationVariables = EditorGUILayout.Foldout(showAnimationVariables, "Animation Settings", myFoldoutStyle);
        if (showAnimationVariables)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(animationController);
            if (gun.animationController != null)
                EditorGUILayout.PropertyField(gunMotions);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();

        GUILayout.Space(8);

        EditorGUILayout.BeginVertical(foldoutContent);
        showShootProcess = EditorGUILayout.Foldout(showShootProcess, "Shooting Settings", myFoldoutStyle);
        if (showShootProcess)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(shootType);
            EditorGUILayout.PropertyField(muzzleFlash);
            EditorGUILayout.PropertyField(impactEffect);
            if (gun.shootType == GunObject.GunType.rigidbody)
            {
                EditorGUILayout.PropertyField(rigidbodyBullet);
                EditorGUILayout.PropertyField(initialForce);
                EditorGUILayout.PropertyField(additiveForce);
            }
            else
                gun.rigidbodyBullet = null;
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();

        GUILayout.Space(8);

        EditorGUILayout.BeginVertical(foldoutContent);
        showGunVariables = EditorGUILayout.Foldout(showGunVariables, "Gun Settings", myFoldoutStyle);
        if (showGunVariables)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(shooting);
            EditorGUILayout.PropertyField(firerate);
            EditorGUILayout.PropertyField(bulletsPerShot);
            if (gun.shooting == GunObject.ShootType.burst)
            {
                EditorGUILayout.PropertyField(burstShot);
                EditorGUILayout.PropertyField(burstTime);
            }

            bool isNotAuto = (gun.shooting != GunObject.ShootType.auto);
            EditorGUILayout.PropertyField(fireDelay);
            if (gun.fireDelay > 0)
            {
                EditorGUILayout.PropertyField(fireCooldownSpeed);
                if (isNotAuto)
                    EditorGUILayout.PropertyField(canFireWhileDelayed);
                else
                    gun.canFireWhileDelayed = false;

            }
            else
                gun.fireCooldownSpeed = 1f;

            EditorGUILayout.PropertyField(canFireWhileActing);
            if (isNotAuto) EditorGUILayout.PropertyField(fireWhenPressedUp);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();

        GUILayout.Space(8);

        EditorGUILayout.BeginVertical(foldoutContent);
        showAmmoVariables = EditorGUILayout.Foldout(showAmmoVariables, "Ammo Settings", myFoldoutStyle);
        if (showGunVariables)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(ammoClip);
            EditorGUILayout.PropertyField(startingClips);
            EditorGUILayout.PropertyField(looseAmmoOnReload);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();

        GUILayout.Space(8);

        EditorGUILayout.BeginVertical(foldoutContent);
        showAimVariables = EditorGUILayout.Foldout(showAimVariables, "Aim Settings", myFoldoutStyle);
        if (showAimVariables)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(aimFOV);
            EditorGUILayout.PropertyField(aimDownSpeed);
            EditorGUILayout.PropertyField(ironSightAim);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();

        GUILayout.Space(8);

        EditorGUILayout.BeginVertical(foldoutContent);
        showDamageVariables = EditorGUILayout.Foldout(showDamageVariables, "Damage Settings", myFoldoutStyle);
        if (showDamageVariables)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(bulletDamage);
            EditorGUILayout.PropertyField(headshotMult);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();

        GUILayout.Space(8);

        EditorGUILayout.BeginVertical(foldoutContent);
        showBulletVariables = EditorGUILayout.Foldout(showBulletVariables, "Bullet Settings", myFoldoutStyle);
        if (showBulletVariables)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(bulletRange);
            EditorGUILayout.PropertyField(bulletSpread);
            EditorGUILayout.PropertyField(aimSpreadMultiplier);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();

        GUILayout.Space(8);

        EditorGUILayout.BeginVertical(foldoutContent);
        showRecoilVariables = EditorGUILayout.Foldout(showRecoilVariables, "Recoil Settings", myFoldoutStyle);
        if (showRecoilVariables)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(aimDownMultiplier);

            if (gun.shooting == GunObject.ShootType.auto)
                EditorGUILayout.PropertyField(cyclesInClip);
            else
                gun.cyclesInClip = 1;

            EditorGUILayout.PropertyField(recoil);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();

        GUILayout.Space(8);

        EditorGUILayout.BeginVertical(foldoutContent);
        showUIVariables = EditorGUILayout.Foldout(showUIVariables, "UI Settings", myFoldoutStyle);
        if (showUIVariables)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(gunIcon);
            EditorGUILayout.PropertyField(ammoOffsetX);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();

        GUILayout.Space(8);

        EditorGUILayout.BeginVertical(foldoutContent);
        showSFXVariables = EditorGUILayout.Foldout(showSFXVariables, "SFX Settings", myFoldoutStyle);
        if (showSFXVariables)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(shootClips);
            EditorGUILayout.PropertyField(reloadSFX);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();

        if (EditorGUI.EndChangeCheck())
            this.serializedObject.ApplyModifiedProperties();

        GUILayout.Space(8);
        if(GUILayout.Button("Give The Player This Gun"))
            gun.GivePlayerGun();
        GUILayout.Space(8);
    }
}
    