using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum IKStatus { weaponIK, lockedIK, animatedIK };
public class ArmIKController : MonoBehaviour
{
    public static Vector3 defaultArmPos;
    public static Vector3 defaultElbowPos;

    public IKStatus IKStatus;
    public InverseKinematics armIK;
    public Transform lockedIKParent;
    public Animator animatedIKTarget;
    public float aimArmZ = -0.45f;

    Transform lockedIKTarget;
    Transform IKTarget;

    GunController guns;
    PlayerController player;

    GunHandler currentGun;
    Status playerStatus;
    Func<IKData> lockedData;
    string armLayer;

    private void Start()
    {
        defaultArmPos = armIK.transform.localPosition;
        defaultElbowPos = armIK.elbow.localPosition;
        armLayer =LayerMask.LayerToName(armIK.gameObject.layer);

        guns = GetComponent<GunController>();
        player = GetComponentInParent<PlayerController>();
        player.AddToStatusChange(PlayerStatusChange);

        IKTarget = new GameObject().transform;
        lockedIKTarget = new GameObject().transform;

        lockedIKTarget.name = "_IK-LockedTarget";
        lockedIKTarget.position = player.transform.position;
        IKTarget.name = "_IK-Target";
        IKTarget.position = player.transform.position;
        armIK.target = IKTarget;
    }

    public void PlayerStatusChange(Status status, Func<IKData> call)
    {
        playerStatus = status;
        lockedData = call;
    }

    private void Update()
    {
        if (guns == null) return;

        currentGun = guns.SelectedGun();
        int gunControllerStatus = (int)guns.status;
        //If we are Reloading, Aiming, Swapping, or Taking Out a gun
        if ((currentGun != null && currentGun.status == GunHandler.GunStatus.reloading) || (gunControllerStatus >= 1 && gunControllerStatus <= 3))
            UpdateStatus(IKStatus.weaponIK);
        else
        {
            int playerControllerStatus = (int)playerStatus;
            if (playerControllerStatus == 11) //underwater swimming
                UpdateStatus(IKStatus.animatedIK);
            else if (playerControllerStatus >= 4 && playerControllerStatus <= 10) //sliding, climbingLadder, wallRunning, vaulting, grabbedLedge, climbingLedge, or surface swimming
                UpdateStatus(IKStatus.lockedIK);
            else
                UpdateStatus(IKStatus.weaponIK);
        }

        IKData lockedIK = null;
        TransformData data = new TransformData(player.transform);
        if (IKStatus == IKStatus.lockedIK && lockedData != null)
            data = (lockedIK = lockedData.Invoke()).HandData();
        TransformHelper.LerpTransform(lockedIKTarget, data, 16f);

        switch (IKStatus)
        {
            case IKStatus.weaponIK:
                if (currentGun != null && guns.status != GunController.GunControlStatus.swapping)
                    UpdateTarget(currentGun.handIKTarget);

                Vector3 armAimPos = defaultArmPos;
                armAimPos.z = guns.isAiming() ? aimArmZ : defaultArmPos.z;
                armIK.transform.localPosition = Vector3.Lerp(armIK.transform.localPosition, armAimPos, Time.deltaTime * 12f);
                armIK.elbow.localPosition = Vector3.Lerp(armIK.elbow.localPosition, defaultElbowPos, Time.deltaTime * 12f);
                break;
            case IKStatus.animatedIK:
                UpdateTarget(animatedIKTarget.transform);
                animatedIKTarget.SetInteger("playerStatus", (int)playerStatus);
                armIK.transform.localPosition = Vector3.Lerp(armIK.transform.localPosition, defaultArmPos, Time.deltaTime * 12f);
                armIK.elbow.localPosition = Vector3.Lerp(armIK.elbow.localPosition, defaultElbowPos, Time.deltaTime * 12f);
                break;
            case IKStatus.lockedIK:
                UpdateTarget(lockedIKTarget);
                if (lockedIK != null)
                {
                    armIK.transform.localPosition = Vector3.Lerp(armIK.transform.localPosition, lockedIK.armLocalPos, Time.deltaTime * 12f);
                    armIK.elbow.position = Vector3.Lerp(armIK.elbow.position, lockedIK.armElbowPos, Time.deltaTime * 12f);
                }
                break;
        }
    }

    private void UpdateStatus(IKStatus to)
    {
        if (IKStatus == to) return;
        IKStatus = to;

        TransformHelper.ChangeLayers(armIK.transform, (IKStatus == IKStatus.lockedIK) ? "Default" : armLayer);
        armIK.transform.SetParent((IKStatus == IKStatus.lockedIK) ? lockedIKParent : this.transform);
    }

    void UpdateTarget(Transform t)
    {
        if (IKTarget.parent != t)
            IKTarget.SetParent(t);

        TransformHelper.LerpLocalTransform(IKTarget, new TransformData(), 24f);
    }
}
public class IKData
{
    public Vector3 handPos;
    public Vector3 handEulerAngles;

    public Vector3 armElbowPos;
    public Vector3 armLocalPos;

    public IKData()
    {
        handPos = Vector3.zero;
        handEulerAngles = Vector3.zero;
        armElbowPos = Vector3.zero;
        armLocalPos = ArmIKController.defaultArmPos;
    }

    public TransformData HandData()
    {
        TransformData data = new TransformData();
        data.position = handPos;
        data.eulerAngles = handEulerAngles;
        return data;
    }
}