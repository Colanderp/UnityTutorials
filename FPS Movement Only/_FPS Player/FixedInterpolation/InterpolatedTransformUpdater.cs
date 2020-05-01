using UnityEngine;
using System.Collections;

/*
 * Credit To : Scott Sewell, developer at KinematicSoup
 * http://www.kinematicsoup.com/news/2016/8/9/rrypp5tkubynjwxhxjzd42s3o034o8
 * /
 
/*
 * Used to allow a later script execution order for FixedUpdate than in GameplayTransform.
 * It is critical this script runs after all other scripts that modify a transform from FixedUpdate.
 */
public class InterpolatedTransformUpdater : MonoBehaviour
{
    private InterpolatedTransform m_interpolatedTransform;
    
	void Awake()
    {
        m_interpolatedTransform = GetComponent<InterpolatedTransform>();
    }
	
	void FixedUpdate()
    {
        m_interpolatedTransform.LateFixedUpdate();
    }
}
