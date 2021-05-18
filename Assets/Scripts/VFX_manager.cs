using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;

public class VFX_manager : MonoBehaviour
{
    //A Simple script to clean-up any particle systems that aren't generating any particles
    // has a 1-second grace period to let the system actually spawn particles

    VisualEffect vfx;
    float spawnParticleBuffer;
    
    // Start is called before the first frame update
    void Start()
    {
        vfx = gameObject.GetComponent<VisualEffect>();
        spawnParticleBuffer = Time.time + 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (vfx.aliveParticleCount <= 0 && Time.time > (spawnParticleBuffer))
        {
            Destroy(gameObject);
        }
    }
}
