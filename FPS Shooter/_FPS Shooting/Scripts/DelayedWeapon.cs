using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayedWeapon : MonoBehaviour
{
    protected float tillShooting;

    public virtual void Start()
    {
        GunHandlerListener listener = GetComponent<GunHandlerListener>();
        if (!listener) return;
        tillShooting = listener.getDelayTime();
        listener.onDelay.AddListener(Delay);
    }

    public virtual void Delay(float t)
    {

    }
}
