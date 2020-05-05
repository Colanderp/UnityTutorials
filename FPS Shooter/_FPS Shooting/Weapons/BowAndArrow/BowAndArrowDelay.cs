using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowAndArrowDelay : DelayedWeapon
{
    [SerializeField]
    private Transform TopStringPoint;
    [SerializeField]
    private Transform StringPullBack;
    [SerializeField]
    private Transform BottomStringPoint;
    [SerializeField]
    public float zWhenPulled = 0.45f;

    float startZ;
    float percent;
    LineRenderer lr;

    public override void Start()
    {
        base.Start();
        startZ = StringPullBack.localPosition.z;
        lr = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (!StringPullBack) return;
        UpdateLine();
        Vector3 localPos = StringPullBack.localPosition;
        localPos.z = Mathf.Lerp(startZ, zWhenPulled, percent);
        StringPullBack.localPosition = localPos;
    }

    void UpdateLine()
    {
        if (!lr) return;
        if (!TopStringPoint || !BottomStringPoint) return;
        lr.positionCount = 3;
        Vector3[] pos =
        {
            TopStringPoint.position,
            StringPullBack.position,
            BottomStringPoint.position
        };
        lr.SetPositions(pos);
    }

    public override void Delay(float t)
    {
        if (tillShooting <= 0) return;
        percent = Mathf.Clamp(t, 0, tillShooting) / tillShooting;
    }
}
