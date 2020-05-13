using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class SurfaceSwimmingMovement : MovementType
{
    [SerializeField]
    private LayerMask topWaterLayer;
    [HideInInspector]
    public Transform cameraMovement;
    
    bool canJumpOutOfWater;

    float treadTime;
    UnderwaterSwimmingMovement underwater;
    public List<WaterHelper> waterHelpers;

    public bool isInWater()
    {
        if (waterHelpers == null) return false;
        return CheckWater(WaterHelper.WaterType.underwater);
    }

    public bool onWaterSurface()
    {
        if (waterHelpers == null) return false;
        return CheckWater(WaterHelper.WaterType.top);
    }

    public bool CheckWater(WaterHelper.WaterType type)
    {
        foreach (WaterHelper water in waterHelpers)
        {
            if (water.type != type) continue; //Do not check for other types
            if (water.PlayerIsInWater) return true;
        }
        return false;
    }

    public void AddWaterHelper(WaterHelper help)
    {
        if (waterHelpers == null) waterHelpers = new List<WaterHelper>();
        if (!WaterAlreadyAdded(help))
            waterHelpers.Add(help);
    }

    public bool WaterAlreadyAdded(WaterHelper check)
    {
        if (waterHelpers == null) return false;
        if (waterHelpers.Contains(check)) return true;
        else return false;
    }

    public void StartSwim()
    {
        player.Crouch(false);
        player.ChangeStatus(Status.surfaceSwimming, IK);
        canJumpOutOfWater = false;
        treadTime = 0;
    }

    void EndSwim()
    {
        if(!player.Uncrouch())
            player.Crouch(true);
    }

    bool CanSwimUnderwater()
    {
        return (underwater != null && underwater.enabled);
    }

    public float getWaterLevel()
    {
        float waterLevel = transform.position.y; //This is just a default y value
        Vector3 pos = transform.position; pos.y += player.info.height;
        if (Physics.Raycast(pos, Vector3.down, out var hit, Mathf.Infinity, topWaterLayer))
            waterLevel = hit.point.y - (player.info.halfheight / 2f);
        return waterLevel;
    }


    public override void PlayerStatusChange(Status status, Func<IKData> call)
    {
        base.PlayerStatusChange(status, call);

        underwater = GetComponent<UnderwaterSwimmingMovement>();
        cameraMovement = GetComponentInChildren<CameraMovement>().transform;
    }

    public override void Movement()
    {
        float wantedYPos = getWaterLevel();
        float dif = transform.position.y - wantedYPos;
        float swimAdjust = Mathf.Sin(dif);
        Vector3 move = new Vector3(playerInput.input.x, 0, playerInput.input.y);
        move = transform.TransformDirection(move) * 2f;

        bool isTreading = (move.sqrMagnitude < 0.02f);
        treadTime = Mathf.PingPong(treadTime + Time.deltaTime, isTreading ? 0.5f : 0.25f);

        float halfCrouch = player.crouchHeight / 2f;
        if (dif < halfCrouch)
            canJumpOutOfWater = true;

        if (playerInput.elevate >= 0.02f && canJumpOutOfWater)
        {
            if (dif >= halfCrouch)
            {
                movement.Jump(Vector3.up, 1f);
                EndSwim();
            }
            else
                move.y = 1f;
        }
        else if (playerInput.elevate <= -0.02f)
            move.y = -1;
        else
        {
            float downWithOffset = cameraMovement.forward.y + 0.333f;
            float swimDown = Mathf.Clamp(downWithOffset * playerInput.input.y, -Mathf.Infinity, 0f);
            move.y = (swimDown <= -0.02f && CanSwimUnderwater()) ? swimDown : treadTime;
        }

        movement.Move(move, 1f, Mathf.Clamp(swimAdjust * 0.5f, 0f, Mathf.Infinity));
    }

    public override void Check(bool canInteract)
    {
        bool onSurface = onWaterSurface();
        bool inWater = isInWater();

        if (!onSurface)
        {
            if(inWater && CanSwimUnderwater())
                player.ChangeStatus(Status.underwaterSwimming);
            else if (playerStatus == changeTo)
                player.ChangeStatus(Status.walking);
        }
        else
        {
            float wantedYPos = getWaterLevel();
            float dif = transform.position.y - wantedYPos;
            Debug.Log(dif);
            if (playerStatus != changeTo) //If we are not swimming
            {
                bool swim = (dif <= 0.1f && movement.controller.velocity.y <= 0); //Check to see if we can swim down
                if (!swim && playerStatus == Status.underwaterSwimming)
                    swim = (dif >= 0f && movement.controller.velocity.y >= 0); //Check the other way too if the first is false and we are swimming underwater

                if (swim) StartSwim();
            }
            else
            {
                //If we are already in the water swimming, then only go back to surface swimming when heading upwards
                if (dif >= -0.1f)
                {
                    if (movement.grounded)
                        EndSwim();
                }
            }
        }
    }

    public override IKData IK()
    {
        IKData data = new IKData();
        float onWater = getWaterLevel() + (player.info.halfheight / 2f);
        Vector3 adjust = (transform.forward - transform.right) * player.info.radius;
        data.handPos = transform.position + adjust;
        data.handPos.y = onWater;

        float time = Mathf.Repeat(Time.time * Mathf.PI, Mathf.PI * 2);
        Vector3 animate = (new Vector3(Mathf.Sin(time), 0, -Mathf.Cos(time)) * player.info.radius) / Mathf.PI;
        data.handPos += animate;

        data.handEulerAngles = transform.eulerAngles;

        adjust = (-transform.forward - transform.right) * player.info.radius;
        data.armElbowPos = transform.position + adjust * 2f;
        data.armElbowPos.y = onWater - player.info.radius;

        data.armLocalPos.x -= player.info.radius;
        return data;
    }
}
