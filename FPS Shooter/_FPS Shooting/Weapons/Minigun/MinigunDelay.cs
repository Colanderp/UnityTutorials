using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigunDelay : DelayedWeapon
{
    public Transform minigunBarrel;
    public float maxSpinSpeed = 90f;
    float speed = 0;

    public override void Start()
    {
        base.Start();
    }

    private void Update()
    {
        minigunBarrel.Rotate(speed * Time.deltaTime, 0, 0);
    }

    public override void Delay(float t)
    {
        if (tillShooting <= 0) return;
        speed = Mathf.Clamp(t, 0, tillShooting) / tillShooting;
        speed *= maxSpinSpeed;
    }
}
