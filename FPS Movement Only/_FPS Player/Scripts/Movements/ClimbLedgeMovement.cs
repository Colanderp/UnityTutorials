using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(GrabLedgeMovement))]
public class ClimbLedgeMovement : MovementType
{
    GrabLedgeMovement grabLedge;

    public override void SetPlayerComponents(PlayerMovement move, PlayerInput input)
    {
        base.SetPlayerComponents(move, input);
        grabLedge = GetComponent<GrabLedgeMovement>();
    }

    public override void Movement()
    {
        if (grabLedge == null) return;
        Vector3 dir = grabLedge.pushFrom - transform.position;
        Vector3 right = Vector3.Cross(Vector3.up, dir).normalized;
        Vector3 move = Vector3.Cross(dir, right).normalized;

        playerInput.ResetJump();
        movement.Move(move, movement.runSpeed, 0f);
        if (new Vector2(dir.x, dir.z).magnitude < 0.125f)
            player.ChangeStatus(Status.idle);
    }

    public override void Check(bool canInteract)
    {
        if (grabLedge == null) return;
        if (playerStatus == grabLedge.changeTo)
        {
            if (playerInput.down.y == 1 || (playerInput.Jump() && playerInput.raw.y > 0))
                player.ChangeStatus(changeTo, IK);
        }
    }

    public override IKData IK()
    {
        IKData data = new IKData();
        if (grabLedge == null) return data;
        Vector3 dir = (grabLedge.pushFrom - transform.position).normalized;

        data.handPos = grabLedge.pushFrom;
        data.handEulerAngles = Quaternion.LookRotation(dir).eulerAngles;
        data.armElbowPos = transform.position;
        data.armElbowPos += Vector3.Cross(dir, Vector3.up) * player.info.radius;
        data.armElbowPos.z = transform.position.z;
        return data;
    }
}
