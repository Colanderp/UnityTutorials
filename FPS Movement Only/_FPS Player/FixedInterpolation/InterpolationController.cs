using UnityEngine;
using System.Collections;

/*
 * Credit To : Scott Sewell, developer at KinematicSoup
 * http://www.kinematicsoup.com/news/2016/8/9/rrypp5tkubynjwxhxjzd42s3o034o8
 * /

/*
 * Manages the interpolation factor that InterpolatedTransforms use to position themselves.
 * Must be attached to a single object in each scene, such as a gamecontroller.
 * It is critical this script's execution order is set before InterpolatedTransform.
 */
public class InterpolationController : MonoBehaviour
{
    private float[] m_lastFixedUpdateTimes; // Stores the last two times at which a FixedUpdate occured.
    private int m_newTimeIndex; // Keeps track of which index is storing the newest value.

    // The proportion of time since the previous FixedUpdate relative to fixedDeltaTime
    private static float m_interpolationFactor;
    public static float InterpolationFactor
    {
        get { return m_interpolationFactor; }
    }

    /*
     * Initializes the array of FixedUpdate times.
     */
    public void Start()
    {
        m_lastFixedUpdateTimes = new float[2];
        m_newTimeIndex = 0;
    }

    /*
     * Record the time of the current FixedUpdate and remove the oldest value.
     */
    public void FixedUpdate()
    {
        m_newTimeIndex = OldTimeIndex(); // Set new index to the older stored time.
        m_lastFixedUpdateTimes[m_newTimeIndex] = Time.fixedTime; // Store new time.
    }

    /*
     * Sets the interpolation factor
     */
    public void Update()
    {
        float newerTime = m_lastFixedUpdateTimes[m_newTimeIndex];
        float olderTime = m_lastFixedUpdateTimes[OldTimeIndex()];

        if (newerTime != olderTime)
        {
            m_interpolationFactor = (Time.time - newerTime) / (newerTime - olderTime);
        }
        else
        {
            m_interpolationFactor = 1;
        }
    }
    
    /*
     * The index of the older stored time 
     */
    private int OldTimeIndex()
    {
        return (m_newTimeIndex == 0 ? 1 : 0);
    }
}
