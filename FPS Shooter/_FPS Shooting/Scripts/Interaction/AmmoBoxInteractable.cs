using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoBoxInteractable : Interactable
{
    public override void Start()
    {
        base.Start();
        onInteract.AddListener(FindObjectOfType<GunController>().RefillAmmo);
    }
}
