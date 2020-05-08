using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class VaultMovement : MovementType
{
    [SerializeField]
    private LayerMask vaultLayer;

    Vector3 vaultOver;
    Vector3 vaultDir;

    GameObject vaultHelper;

    void CreateVaultHelper()
    {
        vaultHelper = new GameObject();
        vaultHelper.transform.name = "_Vault Helper";
    }

    void SetVaultHelper()
    {
        vaultHelper.transform.position = vaultOver;
        vaultHelper.transform.rotation = Quaternion.LookRotation(vaultDir);
    }

    public override void SetPlayerComponents(PlayerMovement move, PlayerInput input)
    {
        base.SetPlayerComponents(move, input);
        CreateVaultHelper();
    }

    public override void Movement()
    {
        Vector3 dir = vaultOver - transform.position;
        Vector3 localPos = vaultHelper.transform.InverseTransformPoint(transform.position);
        Vector3 move = (vaultDir + (Vector3.up * -(localPos.z - player.info.radius) * player.info.height)).normalized;

        if (localPos.z < -(player.info.radius * 2f))
            move = dir.normalized;
        else if (localPos.z > player.info.halfheight)
        {
            movement.controller.height = player.info.height;
            player.ChangeStatus(Status.walking);
        }

        movement.Move(move, movement.runSpeed, 0f);
    }

    public override void Check(bool canInteract)
    {
        if (!canInteract) return;
        if (playerStatus == changeTo) return;

        float movementAdjust = (Vector3.ClampMagnitude(movement.controller.velocity, 16f).magnitude / 16f);
        float checkDis = player.info.radius + movementAdjust;

        if (player.hasObjectInfront(checkDis, vaultLayer) && playerInput.Jump())
        {
            if (Physics.SphereCast(transform.position + (transform.forward * (player.info.radius - 0.25f)), 0.25f, transform.forward, out var sphereHit, checkDis, vaultLayer))
            {
                if (Physics.SphereCast(sphereHit.point + (Vector3.up * player.info.halfheight), player.info.radius, Vector3.down, out var hit, player.info.halfheight - player.info.radius, vaultLayer))
                {
                    Debug.DrawRay(hit.point + (Vector3.up * player.info.radius), Vector3.up * player.info.halfheight);
                    //Check above the point to make sure the player can fit
                    if (Physics.SphereCast(hit.point + (Vector3.up * player.info.radius), player.info.radius, Vector3.up, out var trash, player.info.halfheight))
                        return; //If cannot fit the player then do not vault

                    //Check in-front of the vault to see if something is blocking
                    Vector3 fromPlayer = transform.position;
                    Vector3 toVault = hit.point + (Vector3.up * player.info.radius);
                    fromPlayer.y = toVault.y;

                    Vector3 dir = (toVault - fromPlayer);
                    if (Physics.SphereCast(fromPlayer, player.info.radius / 2f, dir.normalized, out var trash2, dir.magnitude + player.info.radius))
                        return; //If we hit something blocking the vault, then do nothing

                    vaultOver = hit.point;
                    vaultDir = transform.forward;
                    SetVaultHelper();

                    movement.controller.height = player.info.radius;
                    player.ChangeStatus(changeTo, IK);
                }
            }
        }
    }

    public override IKData IK()
    {
        IKData data = new IKData();
        data.handPos = vaultOver + (Vector3.up * player.info.radius);
        data.handEulerAngles = Quaternion.LookRotation(vaultDir - Vector3.up).eulerAngles;
        data.armElbowPos = vaultOver;
        data.armElbowPos.y = transform.position.y;
        data.armElbowPos += Vector3.Cross(vaultDir, Vector3.up) * player.info.radius;
        return data;
    }
}
