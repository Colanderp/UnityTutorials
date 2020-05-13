using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterHelper : MonoBehaviour
{
    public enum WaterType { top, underwater }
    public WaterType type;
    PlayerController player = null;
    SurfaceSwimmingMovement swimming = null;

    public bool playerInWater = false;
    public bool PlayerIsInWater
    {
        get { return playerInWater; }
    }

    private void OnTriggerEnter(Collider other)
    {
        player = other.GetComponent<PlayerController>();
        swimming = player.GetSwimmingMovement();

        if (swimming == null) return; //if you didn't get a component then it will be null so don't continue

        playerInWater = true;
        swimming.AddWaterHelper(this);
    }

    private void OnTriggerStay(Collider other)
    {
        if (swimming == null) return; //if you didn't get a component then it will be null so don't continue

        playerInWater = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (swimming == null) return; //if you didn't get a component then it will be null so don't continue
        
        playerInWater = false;
    }
}
