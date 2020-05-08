using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterHelper : MonoBehaviour
{
    public enum WaterType { top, underwater }
    public WaterType type;
    PlayerController player = null;
    SurfaceSwimmingMovement swimming = null;

    private void OnTriggerEnter(Collider other)
    {
        player = other.GetComponent<PlayerController>();
        swimming = player.GetSwimmingMovement();
    }

    private void OnTriggerStay(Collider other)
    {
        if (swimming == null) return; //if you didn't get a component then it will be null so don't continue
        if (type == WaterType.top)
            swimming.WithinWaterTop();
        else
            swimming.CurrentlyInWater(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (swimming == null) return; //if you didn't get a component then it will be null so don't continue
        if (type != WaterType.top)
            swimming.CurrentlyInWater(false);
    }
}
