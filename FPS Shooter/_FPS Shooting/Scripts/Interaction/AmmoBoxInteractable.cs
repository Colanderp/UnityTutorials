using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoBoxInteractable : Interactable
{
    public override void Start()
    {
        base.Start();
        GunController guns = null;
        if((guns = FindObjectOfType<GunController>()) != null)
            onInteract.AddListener(guns.RefillAmmo);
    }
}
