using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class SlidingMovement : MovementType
{
    public FloatRange slideSpeed = new FloatRange(7.0f, 12.0f);
    bool controlledSlide;

    float slideLimit;
    float slideTime;
    float slideBlendTime = 0.222f;
    float slideDownward = 0f;

    Vector3 groundPos;
    Vector3 slideDir;

    bool canSlide()
    {
        if (!player.isSprinting()) return false;
        if (slideTime > 0 || playerStatus == changeTo) return false;
        return true;
    }

    public override void SetPlayerComponents(PlayerMovement move, PlayerInput input)
    {
        base.SetPlayerComponents(move, input);
        slideLimit = movement.controller.slopeLimit - .1f;
    }

    public override void Movement()
    {
        if (movement.grounded && playerInput.Jump())
        {
            if (controlledSlide)
                slideDir = transform.forward;
            movement.Jump(slideDir + Vector3.up, 1f);
            playerInput.ResetJump();
            slideTime = 0;
        }

        float blend = Mathf.Clamp(slideTime, 0f, slideBlendTime) / slideBlendTime;
        float speed = Mathf.Lerp(slideSpeed.min, slideSpeed.max, slideDownward);
        movement.Move(slideDir, speed * blend, 1f, slideDir.y);
    }
    public override void Check(bool canInteract)
    {
        if (!canInteract) return;
        if (Physics.Raycast(transform.position, -Vector3.up, out var hit, player.info.rayDistance, player.collisionLayer)) //Don't hit the player
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            Vector3 hitNormal = hit.normal;

            Vector3 slopeDir = Vector3.ClampMagnitude(new Vector3(hitNormal.x, -hitNormal.y, hitNormal.z), 1f);
            Vector3.OrthoNormalize(ref hitNormal, ref slopeDir);

            if (angle > 0 && playerStatus == changeTo) //Adjust to slope direction
            {
                Debug.DrawRay(transform.position - Vector3.up * player.info.halfheight, slideDir, Color.green);
                slideDir = Vector3.RotateTowards(slideDir, slopeDir, slideSpeed.min * Time.deltaTime / 2f, 0.0f);
            }
            else
                slideDir.y = 0;

            if (angle > slideLimit && playerStatus != changeTo)
            {
                player.Crouch(true);
                slideDir = slopeDir;
                controlledSlide = false;
                slideTime = slideBlendTime;
                player.ChangeStatus(changeTo, IK);
            }
        }
        else if (playerStatus == changeTo)
        {
            slideDir.y = 0;
            slideDir = slideDir.normalized;
            slideDownward = 0f;
        }

        //Check to slide when running
        if (playerInput.crouch && canSlide())
        {
            player.ChangeStatus(changeTo, IK);
            slideDir = transform.forward;
            movement.controller.height = player.crouchHeight;
            controlledSlide = true;
            slideDownward = 0f;
            slideTime = 1f;
        }

        //Lower slidetime
        if (slideTime > 0)
        {
            if (slideDir.y < 0)
            {
                slideDownward = Mathf.Clamp(slideDownward + Time.deltaTime * Mathf.Sqrt(Mathf.Abs(slideDir.y)), 0f, 1f);
                if (slideTime <= slideBlendTime)
                    slideTime += Time.deltaTime;
            }
            else
            {
                slideDownward = Mathf.Clamp(slideDownward - Time.deltaTime, 0f, 1f);
                slideTime -= Time.deltaTime;
            }

            if (controlledSlide && slideTime <= slideBlendTime)
            {
                if (player.shouldSprint() && player.Uncrouch())
                    player.ChangeStatus(Status.sprinting);
            }
        }
        else if (playerStatus == changeTo)
        {
            if (playerInput.crouching)
                player.Crouch(true);
            else if (!player.Uncrouch()) //Try to uncrouch, if this is false then we cannot uncrouch
                player.Crouch(true); //So just keep crouched
        }
    }

    public override IKData IK()
    {
        IKData data = new IKData();
        Vector3 dir = Vector3.Cross(slideDir, Vector3.up);
        if (Physics.Raycast(transform.position + ((slideDir + dir) * player.info.radius), -Vector3.up, out var hit, 1f))
            groundPos = hit.point;
        data.handPos = groundPos;
        data.handEulerAngles = Quaternion.LookRotation(dir, Vector3.up).eulerAngles;

        data.armElbowPos = transform.position - ((transform.right - Vector3.up) * player.info.radius);
        data.armLocalPos.x = -0.35f;
        return data;
    }
}
