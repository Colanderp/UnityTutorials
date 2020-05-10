using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunPickup : MonoBehaviour
{
    public GunObject pickupThisGun;

    public void PickupAndGivePlayerGun()
    {
        pickupThisGun.AddTempGunToPlayer();
        Destroy(gameObject);
    }
}
