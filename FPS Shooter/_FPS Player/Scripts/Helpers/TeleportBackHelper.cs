using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportBackHelper : MonoBehaviour
{
    public Vector3 teleportTo = Vector3.zero;
    private void OnTriggerEnter(Collider other)
    {
        InterpolatedTransform movable = null;
        if ((movable = other.GetComponent<InterpolatedTransform>()) == null) return;
        if (movable as PlayerMovement)
            movable.ResetPositionTo(teleportTo);
    }

}
