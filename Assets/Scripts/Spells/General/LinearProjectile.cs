using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Experimental.VFX;

public class LinearProjectile : MonoBehaviour
{
    private bool active;
    
    // Delegates
    public delegate void CollisionDelegate(GameObject proj, GameObject hitinfo, GameObject caster);
    private dynamic OnProjectileCollision;
    public void RegisterDelegate(CollisionDelegate obj)
    {
        //Debug.Log(obj);
        //Debug.Log("Test");
        OnProjectileCollision = obj;
    }

    public GameObject projCaster;
    [SerializeField]
    private GameObject projTarget;
    private float projSpeed;    

    private float spawnTime;

    public Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        //rb = GetComponent<RigidBody>();
    }

    public void Activate(GameObject caster, GameObject target, float speed)
    {
        projCaster = caster;
        projTarget = target;
        projSpeed = speed;
        spawnTime = Time.time;

        active = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 destination = (projTarget.transform.position - transform.position).normalized * projSpeed;
        rb.AddForce(destination);        
    }

    void OnTriggerEnter(Collider hitInfo)
    {
        if (!active)
        {
            return;
        }
        var target = hitInfo.gameObject;
        
        //Ignore collisions with nodes, caster, and other projectiles
        if (target.CompareTag("Projectile"))
        {
            return;
        }
        if (target.CompareTag("Node"))
        {
            return;
        }
        if (target.transform.root.gameObject.name == "Player_Root" && target.name != "Player")
        {
            /*
            If this is the case, chances are pretty high we hit a collider that doesn't have any stats,
            i.e. we hit a hand. in this case, we need to change to target object to one that does have stats.
            */

            Transform trueTarget = target.transform.root.transform.Find("Player");
            target = trueTarget.gameObject;
            //Debug.Log("Reassigned Target");
        }
        if (target == projCaster)
        {
            return;
        }
        
        OnProjectileCollision(gameObject, target, projCaster);
        active = false;
        Destroy(gameObject);
    }


}
