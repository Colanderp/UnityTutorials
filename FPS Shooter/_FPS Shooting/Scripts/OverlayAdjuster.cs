using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlayAdjuster : MonoBehaviour
{
    public float adjustSpeed = 12f;

    [Header("Checking Variables")]
    public LayerMask collisionLayer;
    public float checkRadius = 0.125f;
    public float checkDis = 1.25f;

    Vector3 localPos;

    public List<CameraAdjust> scopes;

    void Start()
    {
        localPos = transform.localPosition;
    }

    public void GetScopes()
    {
        scopes = new List<CameraAdjust>();
        foreach (Camera cam in GetComponentsInChildren<Camera>())
        {
            Transform camTrans = cam.transform;
            if (camTrans == this.transform) continue;
            CameraAdjust camAdj = new CameraAdjust(camTrans);
            scopes.Add(camAdj);
        }
    }

    public void AddCameraAdjust(Transform t)
    {
        CameraAdjust camAdj = new CameraAdjust(t);
        scopes.Add(camAdj);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 startPos = transform.parent.TransformPoint(localPos);
        Vector3 backedUpPos = localPos;

        float adjust = 0;
        if (Physics.SphereCast(startPos, checkRadius, transform.forward, out var hit, (checkDis - checkRadius), collisionLayer))
        {
            adjust = (hit.point - startPos).magnitude - checkDis;
            Debug.DrawLine(hit.point, startPos);
        }

        backedUpPos.z += adjust;
        transform.localPosition = Vector3.Lerp(transform.localPosition, backedUpPos, Time.deltaTime * adjustSpeed);

        Vector3 dir = transform.parent.TransformDirection(localPos - backedUpPos);
        foreach(CameraAdjust adj in scopes)
            adj.UpdateAdjust(dir, adjustSpeed);
    }
}

[System.Serializable]
public class CameraAdjust
{
    public Transform camTrans;
    Vector3 localPos;

    public CameraAdjust(Transform t)
    {
        camTrans = t;
        localPos = camTrans.localPosition;
    }
    public void UpdateAdjust(Vector3 adjust, float adjustSpeed)
    {
        Vector3 backedUpPos = localPos;
        Vector3 dir = camTrans.parent.InverseTransformDirection(adjust);
        backedUpPos += dir;
        camTrans.localPosition = Vector3.Lerp(camTrans.localPosition, backedUpPos, Time.deltaTime * adjustSpeed);
    }
}
