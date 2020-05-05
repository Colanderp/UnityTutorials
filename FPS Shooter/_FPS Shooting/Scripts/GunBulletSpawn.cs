using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunBulletSpawn : MonoBehaviour
{
    ParticleSystem smoke;
    
    void Start()
    {
        smoke = GetComponent<ParticleSystem>();    
    }

    public void ShootOutSmoke()
    {
        if(smoke != null)
            smoke.Play();
    }
}
