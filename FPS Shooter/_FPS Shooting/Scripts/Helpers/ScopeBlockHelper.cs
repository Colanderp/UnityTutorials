using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScopeBlockHelper : MonoBehaviour
{
    //Min is when it is clear, Max is when its opeque
    public FloatRange clearRange = new FloatRange(0, 1);

    Transform distanceFrom = null;
    float distance = float.MaxValue;
    Material blockMat;
    Color blockedColor;
    float rangeDif;

    // Start is called before the first frame update
    void Start()
    {
        rangeDif = clearRange.GetDifference();
        blockMat = GetComponent<Renderer>().material;
        blockedColor = blockMat.GetColor("_Color");
        //This transform is where we will be reading the distance from
        OverlayAdjuster overlay = null;
        if((overlay = GetComponentInParent<OverlayAdjuster>()) != null)
            distanceFrom = overlay.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (distanceFrom == null) return;
        distance = Vector3.Distance(transform.position, distanceFrom.position);
        distance = Mathf.Clamp(distance, clearRange.min, clearRange.max);
        blockedColor.a = (distance - clearRange.min) / rangeDif;
        blockMat.SetColor("_Color", blockedColor);
    }
}
