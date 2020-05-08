using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class GrabLedgeMovement : MovementType
{
    [SerializeField]
    private LayerMask ledgeLayer;
    [HideInInspector]
    public Vector3 pushFrom;
    [HideInInspector]
    public bool canGrabLedge = true;

    public override void Movement()
    {
        if (playerInput.Jump() && playerInput.raw.y <= 0)
        {
            movement.Jump((Vector3.up - transform.forward).normalized, 1f);
            playerInput.ResetJump();
            player.ChangeStatus(Status.walking);
        }

        movement.Move(Vector3.zero, 0f, 0f); //Stay in place
    }

    public override void Check(bool canInteract)
    {
        if (playerStatus == changeTo)
        {
            canGrabLedge = false;
            if (playerInput.down.y == -1)
                player.ChangeStatus(Status.walking);
        }

        if (movement.grounded || movement.moveDirection.y > 0)
            canGrabLedge = true;

        if (!canGrabLedge) return;

        if (!movement.grounded && movement.moveDirection.y >= 0)
            return;

        if (!canInteract) return;
        //Check for ledge to grab onto 
        Vector3 dir = transform.TransformDirection(new Vector3(0, -0.5f, 1).normalized);
        Vector3 pos = transform.position + (Vector3.up * player.info.height / 3f) + (transform.forward * player.info.radius / 2f);
        bool right = Physics.Raycast(pos + (transform.right * player.info.radius / 2f), dir, player.info.radius + 0.125f, ledgeLayer);
        bool left = Physics.Raycast(pos - (transform.right * player.info.radius / 2f), dir, player.info.radius + 0.125f, ledgeLayer);

        if (Physics.Raycast(pos, dir, out var hit, player.info.radius + 0.125f, ledgeLayer) && right && left)
        {
            Vector3 rotatePos = transform.InverseTransformPoint(hit.point);
            rotatePos.x = 0; rotatePos.z = 1;
            pushFrom = transform.position + transform.TransformDirection(rotatePos); //grab the position with local z = 1
            rotatePos.z = player.info.radius * 2f;

            Vector3 checkCollisions = transform.position + transform.TransformDirection(rotatePos); //grab it closer now

            //Check if you would be able to stand on the ledge
            if (!Physics.SphereCast(checkCollisions, player.info.radius, Vector3.up, out hit, player.info.height - player.info.radius))
                player.ChangeStatus(changeTo, IK);
        }
    }

    public override IKData IK()
    {
        IKData data = new IKData();
        Vector3 dir = (pushFrom - transform.position); dir.y = 0;
        dir = dir.normalized;

        float handRadius = 0.125f;
        data.handPos = transform.position;
        data.handPos.y = pushFrom.y;
        data.handPos += dir * (player.info.radius + handRadius);

        Vector3 handDir = -Vector3.Cross(dir, Vector3.up);
        data.armElbowPos = (data.handPos - (handDir * player.info.radius));
        data.handEulerAngles = Quaternion.LookRotation(handDir).eulerAngles;
        data.armLocalPos.y = 0.075f; data.armLocalPos.z = -0.5f;
        return data;
    }
}
