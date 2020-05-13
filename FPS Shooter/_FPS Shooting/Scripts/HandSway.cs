using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandSway : InterpolatedTransformFull
{
    [SerializeField]
    [Range(0.1f, 90f)]
    private float tiltAngle = 4.25f;

    [SerializeField]
    [Range(0.01f, 0.1f)]
    private float minAmount = 0.025f;

    [SerializeField]
    [Range(0.01f, 0.1f)]
    private float maxAmount = 0.075f;

    [SerializeField]
    [Range(0.1f, 5f)]
    private float swaySmoothing = 2.75f;

    [SerializeField]
    [Range(0.1f, 5f)]
    private float smoothRotation = 1.5f;

    private Quaternion target;
    private Vector3 startingPos;

    private Vector2 rawLook;
    private Vector2 look;
    private float swayMultiplier;

    public override void Start()
    {
        base.Start();
        startingPos = transform.localPosition;
    }

    public void SetSwayMultiplier(float s)
    {
        swayMultiplier = Mathf.Clamp(s, 0f, 1f);
    }

    public override void Update()
    {
        base.Update();
        SetInputs(new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")));
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        
        float mult = Mathf.Pow(swayMultiplier, 2);
        float factorX = -rawLook.x * minAmount * mult;
        float factorY = -rawLook.y * minAmount * mult;

        float tiltZ = look.x * tiltAngle * mult;
        float tiltX = look.y * tiltAngle * mult;

        factorX = Clamp(factorX, -maxAmount, maxAmount);
        factorY = Clamp(factorY, -maxAmount, maxAmount);

        Vector3 finalPos = new Vector3(startingPos.x + factorX, startingPos.y + factorY, startingPos.z);
        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPos, Time.fixedDeltaTime * swaySmoothing);

        target = Quaternion.Euler(tiltX, 0, tiltZ);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, target, Time.fixedDeltaTime * smoothRotation);
    }

    public void SetInputs(Vector2 looking)
    {
        rawLook = looking.normalized;
        look = looking;
    }

    public static float Clamp(float amount, float min, float max)
    {
        return Mathf.Clamp(amount, min, max);
    }
}
