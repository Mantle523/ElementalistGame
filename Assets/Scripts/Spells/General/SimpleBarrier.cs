using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBarrier : MonoBehaviour
{
    //Delegates
    public delegate void CollisionDelegate(GameObject shield, GameObject hitinfo);
    private dynamic OnBarrierCollision;
    public void RegisterDelegate(CollisionDelegate obj)
    {
        OnBarrierCollision = obj;
    }

    private float spawnTime;
    [SerializeField]
    private float despawnTime;

    public GameObject caster;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Activate(GameObject cstr, float duration)
    {
        caster = cstr;

        spawnTime = Time.time;
        despawnTime = spawnTime + duration;
    } 

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Time.time >= despawnTime)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider hitInfo)
    {
        if (hitInfo.gameObject.CompareTag("Projectile"))
        {
            OnBarrierCollision(gameObject, hitInfo.gameObject);
        }
    }
}
