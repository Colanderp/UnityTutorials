using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateFOV : MonoBehaviour
{
    float adjustToFOV;
    float adjustSpeed;
    float baseFOV = 60f;

    Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        baseFOV = cam.fieldOfView;
        adjustToFOV = baseFOV;
    }

    public void SetFOV(bool setTo, float fov, float speed)
    {
        adjustSpeed = Mathf.Abs(fov - baseFOV) / (speed / 2);
        adjustToFOV = (setTo) ? fov : baseFOV;
    }

    private void Update()
    {
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, adjustToFOV, Time.deltaTime * adjustSpeed);
    }
}
