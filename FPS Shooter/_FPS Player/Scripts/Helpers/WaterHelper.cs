using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterHelper : MonoBehaviour
{
    public enum WaterType { top, underwater }
    public WaterType type;
    PlayerController player = null;

    private void OnTriggerEnter(Collider other)
    {
        player = other.GetComponent<PlayerController>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (player == null) return; //if you didn't get a component then it will be null so don't continue
        if (type == WaterType.top)
            player.WithinWaterTop();
        else
            player.CurrentlyInWater(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (player == null) return; //if you didn't get a component then it will be null so don't continue
        if (type != WaterType.top)
            player.CurrentlyInWater(false);
    }
}
