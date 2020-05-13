using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SurfaceSwimmingMovement))]
public class UnderwaterSwimmingMovement : MovementType
{
    SurfaceSwimmingMovement swimming;

    public override void SetPlayerComponents(PlayerMovement move, PlayerInput input)
    {
        base.SetPlayerComponents(move, input);
        swimming = GetComponent<SurfaceSwimmingMovement>();
    }

    public override void Movement()
    {
        Vector3 swim = swimming.cameraMovement.TransformDirection(new Vector3(playerInput.input.x, 0, playerInput.input.y)) * 2f;
        swim += Vector3.up * playerInput.elevate;
        swim = Vector3.ClampMagnitude(swim, 2f);
        movement.Move(swim, 1f, 0f);
    }

    public override void Check(bool canInteract)
    {
        if (!swimming.isInWater() && playerStatus == changeTo)
            player.ChangeStatus(Status.walking);
    }
}
