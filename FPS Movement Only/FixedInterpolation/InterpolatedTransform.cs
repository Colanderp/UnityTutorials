using UnityEngine;
using System.Collections;

/*
 * Credit To : Scott Sewell, developer at KinematicSoup
 * http://www.kinematicsoup.com/news/2016/8/9/rrypp5tkubynjwxhxjzd42s3o034o8
 * /

[RequireComponent(typeof(InterpolatedTransformUpdater))]
/*
 * Interpolates an object to the transform at the latest FixedUpdate from the transform at the previous FixedUpdate.
 * It is critical this script's execution order is set before all other scripts that modify a transform from FixedUpdate.
 */
public class InterpolatedTransform : MonoBehaviour
{
    public Vector3[] m_lastPositions; // Stores the transform of the object from the last two FixedUpdates
    [HideInInspector]
    public int m_newTransformIndex; // Keeps track of which index is storing the newest value.

    /*
     * Initializes the list of previous orientations
     */
    public virtual void OnEnable()
    {
        ForgetPreviousTransforms();
    }

    /*
     * Resets the previous transform list to store only the objects's current transform. Useful to prevent
     * interpolation when an object is teleported, for example.
     */
    public virtual void ForgetPreviousTransforms()
    {
        m_lastPositions = new Vector3[2];
        m_newTransformIndex = 0;
    }

    public virtual void ResetPositionTo(Vector3 resetTo)
    {
        //Remove old interpolation
        ForgetPreviousTransforms();
        //Reset position to 'resetTo'
        transform.position = resetTo;
        m_lastPositions[m_newTransformIndex] = transform.position;
        m_lastPositions[OldTransformIndex()] = transform.position;
    }

    /*
     * Sets the object transform to what it was last FixedUpdate instead of where is was last interpolated to,
     * ensuring it is in the correct position for gameplay scripts.
     */
    public virtual void FixedUpdate()
    {
        Vector3 mostRecentTransform = m_lastPositions[m_newTransformIndex];
        transform.position = mostRecentTransform;
    }
    
    /*
     * Runs after ofther scripts to save the objects's final transform.
     */
    public virtual void LateFixedUpdate()
    {
        m_newTransformIndex = OldTransformIndex(); // Set new index to the older stored transform.
        m_lastPositions[m_newTransformIndex] = transform.position;
    }

    /*
     * Interpolates the object transform to the latest FixedUpdate's transform
     */
    public virtual void Update()
    {
        Vector3 newestTransform = m_lastPositions[m_newTransformIndex];
        Vector3 olderTransform = m_lastPositions[OldTransformIndex()];

        transform.position = Vector3.Lerp(olderTransform, newestTransform, InterpolationController.InterpolationFactor);
    }

    /*
     * The index of the older stored transform
     */
    public virtual int OldTransformIndex()
    {
        return (m_newTransformIndex == 0 ? 1 : 0);
    }
}
