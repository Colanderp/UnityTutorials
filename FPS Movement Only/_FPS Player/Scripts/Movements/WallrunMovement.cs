using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class WallrunMovement : MovementType
{
    [SerializeField]
    private LayerMask wallrunLayer;
    [SerializeField]
    private float wallrunMinimum = 0.2f;

    Vector3 wallNormal = Vector3.zero;
    float wallrunTime;
    int wallDir = 1;

    public int getWallDir()
    {
        return wallDir;
    }

    public override void Movement()
    {
        Vector3 input = playerInput.input;
        float s = (input.y > 0) ? input.y : 0;

        Vector3 move = wallNormal * s;

        if (playerInput.Jump() && wallrunTime > wallrunMinimum)
        {
            wallrunTime = 0;
            Vector3 forward = wallNormal.normalized;
            Vector3 right = Vector3.Cross(forward, Vector3.up) * wallDir;
            Vector3 wallJump = (Vector3.up * (s + 0.5f) + forward * s * 1.5f + right * (s + 0.5f)).normalized;
            movement.Jump(wallJump, (s + 1f));
            playerInput.ResetJump();
            player.ChangeStatus(Status.walking);
        }

        if (!player.hasWallToSide(wallDir, wallrunLayer) || movement.grounded)
            player.ChangeStatus(Status.walking);

        float inputGravity = (1f - s) + (s / 4f); //More input, less gravity
        float timeGravity = Mathf.Lerp(0f, 1f, wallrunTime / wallrunMinimum);
        movement.Move(move, movement.runSpeed, inputGravity * timeGravity);
        wallrunTime += Time.deltaTime;
    }

    public override void Check(bool canInteract)
    {
        if (!canInteract || movement.grounded || movement.moveDirection.y >= 0)
            return;

        int wall = 0;
        if (player.hasWallToSide(1, wallrunLayer))
            wall = 1;
        else if (player.hasWallToSide(-1, wallrunLayer))
            wall = -1;

        if (wall == 0) return;

        if (Physics.Raycast(transform.position + (transform.right * wall * player.info.radius), transform.right * wall, out var hit, player.info.halfradius, wallrunLayer))
        {
            wallDir = wall;
            wallrunTime = 0;
            wallNormal = Vector3.Cross(hit.normal, Vector3.up) * -wallDir;
            player.ChangeStatus(changeTo, IK);
        }
    }

    public override IKData IK()
    {
        IKData data = new IKData();
        bool left = (wallDir == -1);
        if (Physics.Raycast(transform.position + (transform.right * wallDir * player.info.radius), transform.right * wallDir, out var hit, player.info.halfradius, wallrunLayer))
            data.handPos = hit.point;
        if (left)
        {
            data.armLocalPos.x = -0.35f;
            data.armLocalPos.z = -0.55f;
            data.handPos += (Vector3.up + wallNormal) * player.info.radius * 2f;
            data.handEulerAngles = Quaternion.LookRotation(wallNormal, wallDir * Vector3.Cross(wallNormal, Vector3.up)).eulerAngles;
            data.armElbowPos = data.handPos - wallNormal;
        }
        else
        {
            data.armElbowPos = data.handPos;
            data.armLocalPos.x = 0;
            data.armLocalPos.z = -0.325f;
            data.handPos += (2 * Vector3.up + wallNormal) * player.info.radius;
            data.handEulerAngles = Quaternion.LookRotation(Vector3.up, wallDir * Vector3.Cross(wallNormal, Vector3.up)).eulerAngles;
        }

        data.armLocalPos.y = 0;
        return data;
    }
}
