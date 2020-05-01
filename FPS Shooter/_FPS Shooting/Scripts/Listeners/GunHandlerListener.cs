using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DelayEvent : UnityEvent<float> { }
public class GunHandlerListener : MonoBehaviour
{
    public UnityEvent onShoot;
    public UnityEvent onReload;
    public UnityEvent onPutAway;
    public UnityEvent onTakeOut;
    public DelayEvent onDelay;

    protected GunObject gun;

    public void Initialize(GunObject g)
    {
        gun = g;
        onDelay = new DelayEvent();
    }

    public float getDelayTime()
    {
        return gun.fireDelay;
    }
}
