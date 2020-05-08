using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class LadderMovement : MovementType
{
    [SerializeField]
    private LayerMask ladderLayer;
    Vector3 ladderNormal = Vector3.zero;
    Vector3 lastTouch = Vector3.zero;

    public override void Movement()
    {
        Vector3 input = playerInput.input;
        Vector3 move = Vector3.Cross(Vector3.up, ladderNormal).normalized;
        move *= input.x;
        move.y = input.y * movement.walkSpeed;

        bool goToGround = (move.y < -0.02f && movement.grounded);

        if (playerInput.Jump())
        {
            movement.Jump((-ladderNormal + Vector3.up * 2f).normalized, 1f);
            playerInput.ResetJump();
            player.ChangeStatus(Status.walking);
        }

        if (!player.hasObjectInfront(0.05f, ladderLayer) || goToGround)
        {
            player.ChangeStatus(Status.walking);
            Vector3 pushUp = ladderNormal;
            pushUp.y = 0.25f;

            movement.ForceMove(pushUp, movement.walkSpeed, 0.25f, true);
        }
        else
            movement.Move(move, 1f, 0f);
    }

    public override void Check(bool canInteract)
    {
        //Check for ladder all across player (so they cannot use the side)
        bool right = Physics.Raycast(transform.position + (transform.right * player.info.halfradius), transform.forward, player.info.radius + 0.125f, ladderLayer);
        bool left = Physics.Raycast(transform.position - (transform.right * player.info.halfradius), transform.forward, player.info.radius + 0.125f, ladderLayer);

        if (Physics.Raycast(transform.position, transform.forward, out var hit, player.info.radius + 0.125f, ladderLayer) && right && left)
        {
            if (hit.normal != hit.transform.forward) return;

            ladderNormal = -hit.normal;
            if (player.hasObjectInfront(0.05f, ladderLayer) && playerInput.input.y > 0.02f)
                player.ChangeStatus(changeTo, IK);
        }
    }

    public override IKData IK()
    {
        IKData data = new IKData();
        Vector3 upOffset = Vector3.up * player.info.radius * 2f;
        Vector3 handUp = Vector3.Cross(ladderNormal, Vector3.up);
        if (Physics.SphereCast(transform.position + upOffset, player.info.radius, ladderNormal, out var hit, 0.125f, ladderLayer))
        {
            if (Physics.SphereCast(hit.point + handUp, 0.125f, -handUp, out var hit2, 1.125f, ladderLayer))
                lastTouch = hit2.point - (ladderNormal * 0.125f);
        }
        lastTouch.y = (int)(lastTouch.y * 2f) / 2f;
        data.handPos = lastTouch;

        data.handEulerAngles = Quaternion.LookRotation(ladderNormal, handUp).eulerAngles;
        data.armElbowPos = transform.position + handUp * player.info.radius;
        data.armLocalPos.x = -0.35f;
        return data;
    }
}
