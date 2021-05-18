using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : GenericSpell, ISpell
{
    private static FireBall _instance;
    private FireBall()
    {

    }
    public static FireBall GetInstance()
    {
        if (_instance != null) return _instance;
        return _instance = ScriptableObject.CreateInstance(typeof(FireBall)) as FireBall;
    }

    [SerializeField] private SpellProfile _spellProfile = null;//Assigned in Inspector
    public SpellProfile spellProfile
    {
        get
        {
            return _spellProfile;
        }
    }

    public SpellCastType spellCastType
    {
        get
        {
            return SpellCastType.SPELL_CAST_SIMPLE;
        }
    }

    public SpellType spellType
    {
        get
        {
            return SpellType.SPELL_TYPE_OFFENCE;
        }
    }

    public string[] spellTriggerGesture
    {
        get
        {
            return new string[] { "short_punch_R", "short_punch_L" };
        }
    }

    public SpawnPos spawnPos
    {
        get
        {
            return SpawnPos.Hand;
        }
    }

    public float manacost
    {
        get
        {
            return 20;
        }
    }

    private float speed = 30.0f;
    private float damage = 10;

    private Object FireBall_Trail;
    private Object FireBall_Launch;
    private Object FireBall_Land;

    
    //private LinearProjectle projScript;

    public void PreLoad()
    {
        FireBall_Trail = Resources.Load("Prefabs/ParticleSystems/FireBall/FireBall_Trail");
        FireBall_Launch = Resources.Load("Prefabs/ParticleSystems/FireBall/FireBall_Launch");
        FireBall_Land = Resources.Load("Prefabs/ParticleSystems/FireBall/FireBall_Land");
    }

    public void Cast(GameObject caster, GameObject target, GameObject hand, string gesture)
    {
        //Create the projectile
        GameObject Projectile = CreateLinearProjectile(caster, target,spawnPos, hand, speed);
        LinearProjectile projScript = Projectile.GetComponent<LinearProjectile>();

        AttachFX(FireBall_Trail, Projectile);
        FireFX(FireBall_Launch, hand);
        //GameObject SimBarrier = GameObject.Instantiate(Resources.Load("Prefabs/SimpleBarrier"), spawnPos.transform.position, spawnPos.transform.rotation) as GameObject;

        projScript.RegisterDelegate(OnProjectileCollision);

        // Probably need an OnCast event here (Put mana costs etc in said event)
        //StartCombo("EXwindow", caster);
    }

    public void OnProjectileCollision(GameObject proj, GameObject hitinfo, GameObject caster)
    {
        Debug.Log(proj.name + " has collided with " + hitinfo.name);
        //print("Now casting " + spellName + " effects!");        
        DamageTarget(caster, hitinfo, damage);
        FireFX(FireBall_Land, proj);
    }
}
