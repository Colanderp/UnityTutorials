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
    [HideInInspector]
    public bool onWaterSurface;
    [HideInInspector]
    public bool isInWater;

    bool canJumpOutOfWater;

    float treadTime;
    UnderwaterSwimmingMovement underwater;

    public void StartSwim()
    {
        StartCoroutine(startSwimming());
        IEnumerator startSwimming()
        {
            movement.controller.height = player.info.halfheight;
            yield return new WaitForEndOfFrame();
            player.ChangeStatus(Status.surfaceSwimming, IK);
            canJumpOutOfWater = false;
            treadTime = 0;
        }
    }

    void EndSwim()
    {
        movement.controller.height = player.info.height;
        player.ChangeStatus(Status.walking);
    }

    bool CanSwimUnderwater()
    {
        return (underwater != null && underwater.enabled);
    }

    public void CurrentlyInWater(bool inWater)
    {
        isInWater = inWater;
    }

    public float getWaterLevel()
    {
        float waterLevel = transform.position.y; //This is just a default y value
        Vector3 pos = transform.position; pos.y += 100f; //Add 100 to make it above the player and the water (NOTE: just don't have 100 units deep water, if you do, increase this)
        if (Physics.Raycast(pos, Vector3.down, out var hit, Mathf.Infinity, topWaterLayer))
            waterLevel = hit.point.y - (player.info.halfheight / 2f);
        return waterLevel;
    }

    public void WithinWaterTop(bool onSurface)
    {
        onWaterSurface = onSurface;
        if (!onWaterSurface) return;
        float wantedYPos = getWaterLevel();
        float dif = transform.position.y - wantedYPos;
        if ((int)playerStatus < 9) //If we are not swimming
        {
            if (dif <= 0f && movement.controller.velocity.y <= 0)
                StartSwim();
        }
        else
        {
            //If we are already in the water swimming, then only go back to surface swimming when heading upwards
            if (dif >= -0.1f)
            {
                if (movement.grounded)
                    EndSwim();
                else if (playerStatus == Status.underwaterSwimming)
                    StartSwim();
            }
        }
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

        if (dif < player.info.halfheight / 4f)
            canJumpOutOfWater = true;

        if (playerInput.elevate >= 0.02f && canJumpOutOfWater)
        {
            if (dif >= player.info.halfheight / 4)
            {
                movement.Jump(Vector3.up, 1f);
                EndSwim();
            }
            else
                move.y = 1f;
        }
        else if (isInWater)
        {
            if (playerInput.elevate <= -0.02f)
                move.y = -1;
            else
            {
                float downWithOffset = cameraMovement.forward.y + 0.333f;
                float swimDown = Mathf.Clamp(downWithOffset * playerInput.input.y, -Mathf.Infinity, 0f);
                move.y = (swimDown <= -0.02f && CanSwimUnderwater()) ? swimDown : treadTime;
            }

            if (dif < -player.info.halfheight / 4f && CanSwimUnderwater())
                player.ChangeStatus(Status.underwaterSwimming);
        }

        movement.Move(move, 1f, Mathf.Clamp(swimAdjust * 0.5f, 0f, Mathf.Infinity));
    }

    public override void Check(bool canInteract)
    {
        if (!isInWater && !onWaterSurface && playerStatus == changeTo)
            player.ChangeStatus(Status.walking);
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
