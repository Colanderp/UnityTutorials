using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterpolatedTransformFull : InterpolatedTransform
{
    [HideInInspector]
    public TransformData[] m_lastData; // Stores the transform of the object from the last two FixedUpdates

    public virtual void Start()
    {
        m_lastData = new TransformData[2];
        m_lastData[0] = new TransformData();
        m_lastData[1] = new TransformData();
        m_lastData[m_newTransformIndex] = GetCurrentData();
        m_lastData[OldTransformIndex()] = GetCurrentData();
    }
    /*
     * Resets the previous transform list to store only the objects's current transform. Useful to prevent
     * interpolation when an object is teleported, for example.
     */
    public override void ForgetPreviousTransforms()
    {
        m_lastData = new TransformData[2];
        m_lastData[0] = GetCurrentData();
        m_lastData[1] = GetCurrentData();
        m_newTransformIndex = 0;
    }

    public override void ResetPositionTo(Vector3 resetTo)
    {
        //Remove old interpolation
        ForgetPreviousTransforms();
        //Reset position to 'resetTo'
        transform.localPosition = resetTo;
        m_lastData[m_newTransformIndex] = GetCurrentData();
        m_lastData[OldTransformIndex()] = GetCurrentData();
    }

    TransformData GetCurrentData()
    {
        TransformData data = new TransformData();
        data.position = transform.localPosition;
        data.eulerAngles = transform.localEulerAngles;
        data.scale = transform.localScale;
        return data;
    }

    /*
     * Sets the object transform to what it was last FixedUpdate instead of where is was last interpolated to,
     * ensuring it is in the correct position for gameplay scripts.
     */
    public override void FixedUpdate()
    {
        TransformData mostRecentTransform = m_lastData[m_newTransformIndex];
        TransformHelper.SetLocalTransformData(transform, mostRecentTransform);
    }

    /*
     * Runs after ofther scripts to save the objects's final transform.
     */
    public override void LateFixedUpdate()
    {
        m_newTransformIndex = OldTransformIndex(); // Set new index to the older stored transform.
        m_lastData[m_newTransformIndex] = GetCurrentData();
    }

    /*
     * Interpolates the object transform to the latest FixedUpdate's transform
     */
    public override void Update()
    {
        TransformData newestData = m_lastData[m_newTransformIndex];
        TransformData olderData = m_lastData[OldTransformIndex()];

        transform.localPosition = Vector3.Lerp(olderData.position, newestData.position, InterpolationController.InterpolationFactor);

        transform.localRotation = Quaternion.Slerp(
                                    Quaternion.Euler(olderData.eulerAngles),
                                    Quaternion.Euler(newestData.eulerAngles),
                                    InterpolationController.InterpolationFactor);
    }
}
