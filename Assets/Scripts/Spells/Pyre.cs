using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;

public class Pyre : GenericSpell, ISpell, IDualChannel
{
    private static Pyre _instance;
    private Pyre()
    {

    }
    public static Pyre GetInstance()
    {
        if (_instance != null) return _instance;
        return _instance = ScriptableObject.CreateInstance(typeof(Pyre)) as Pyre;
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
            return SpellCastType.SPELL_CAST_DUALCHANNELLED;
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
            return new string[] { "sun_praise_L", "sun_praise_R" };
        }
    }

    public SpawnPos spawnPos
    {
        get
        {
            return SpawnPos.Pos_Overhead;
        }
    }

    public float manacost
    {
        get
        {
            return 50;
        }
    }

    public float channelDuration
    {
        get
        {
            return 3.0f;
        }
    }

    private float speed = 18;
    private float baseDamage = 10;
    private float damagePerStack = 2;

    private Object Pyre_Charge;
    private Object Pyre_Trail;
    private Object Pyre_Land;
    

    public void PreLoad()
    {
        Pyre_Charge = Resources.Load("Prefabs/ParticleSystems/Pyre/Pyre_Charge");
        Pyre_Trail = Resources.Load("Prefabs/ParticleSystems/Pyre/Pyre_Trail");
        Pyre_Land = Resources.Load("Prefabs/ParticleSystems/Pyre/Pyre_Land");
    }

    public void Cast(GameObject caster, GameObject target, GameObject hand, string gesture)
    {
        
    }

    public void DualCast(GameObject caster, GameObject target, GameObject[] hands, string gesture)
    {
        Vector3 beam_converge_point = FindSpawnPosition(caster, spawnPos);
        Debug.Log("DUALCAST");

        foreach (GameObject hand in hands)
        {            
            VisualEffect pyreChargeFX = AttachFX(Pyre_Charge, hand);
            if (pyreChargeFX.HasVector3("targetPos"))
            {
                pyreChargeFX.SetVector3("targetPos", beam_converge_point);
            }

            Pyre_Charge_Modifier mod = FindModifier(caster, "Pyre_Charge_Modifier") as Pyre_Charge_Modifier;
            if (mod == null)
            {
                Pyre_Charge_Modifier modifier = new Pyre_Charge_Modifier(pyreChargeFX);
                CreateModifier(caster, caster, modifier, spellProfile);
            }
            else
            {
                mod.AddSecondParticle(pyreChargeFX);
            }
        }
    }

    public void onDualChannelTick(GameObject caster, GameObject target, GameObject[] hands, float channelTime)
    {
        
    }

    public void onDualChannelComplete(GameObject caster, GameObject target, GameObject[] hands)
    {
        Pyre_Charge_Modifier pyreChargeModifier = FindModifier(caster, "Pyre_Charge_Modifier") as Pyre_Charge_Modifier;
        if (pyreChargeModifier != null)
        {
            pyreChargeModifier.OnPyreChannelComplete();
        }
    }

    public void onDualChannelInterrupt(GameObject caster, GameObject target, GameObject[] hands, float channelTime)
    {

    }

    public void onDualChannelEnd(GameObject caster, GameObject target, GameObject[] hands, float channelTime)
    {
        Pyre_Charge_Modifier pyreChargeModifier = FindModifier(caster, "Pyre_Charge_Modifier") as Pyre_Charge_Modifier;
        if (pyreChargeModifier != null)
        {
            pyreChargeModifier.OnPyreChannelEnded();
        }
        GameObject Projectile = CreateLinearProjectile(caster, target, spawnPos, null, speed);
        LinearProjectile projscript = Projectile.GetComponent<LinearProjectile>();
        AttachFX(Pyre_Trail, Projectile);

        projscript.RegisterDelegate(OnProjectileCollision);        
    }

    public void OnProjectileCollision(GameObject proj, GameObject hitinfo, GameObject caster)
    {
        Debug.Log(proj.name + " has collided with " + hitinfo.name);
        int modStackCount = 0;
        Pyre_Charge_Modifier pyreChargeModifier = FindModifier(caster, "Pyre_Charge_Modifier") as Pyre_Charge_Modifier;
        if (pyreChargeModifier != null)
        {
            modStackCount = pyreChargeModifier.stackCount;
        }

        float finalDamage = baseDamage + (damagePerStack * modStackCount);
        DamageTarget(caster, hitinfo, finalDamage);
        FireFX(Pyre_Land, proj);

        RemoveModifier(caster, caster, "Pyre_Charge_Modifier");
    }
}

public class Pyre_Charge_Modifier : GenericModifier, IModifier
{
    public string modifierName
    {
        get
        {
            return "Pyre Intensity";
        }
    }

    public float Duration
    {
        get
        {
            return -1f;
        }
    }

    public float FinishTime
    {
        get; set;
    }

    public bool canStack
    {
        get
        {
            return false;
        }
    }

    public int stackCount
    {
        get; set;
    }

    public GameObject modCaster
    {
        get; set;
    }

    public GameObject modTarget
    {
        get; set;
    }

    private float timeTillNextTick;
    private bool isCasterChannelling;
    private VisualEffect[] chargeParticles; //The modifier stores data from the spell, in this case the particle effect, since it has persistant effects

    public Pyre_Charge_Modifier(VisualEffect castersParticle)
    {
        chargeParticles = new VisualEffect[2] { castersParticle, null };
        isCasterChannelling = false;
    }

    public void OnCreate(GameObject caster, GameObject target)
    {
        modCaster = caster;
        modTarget = target;
        timeTillNextTick = Time.time + 0.1f;
        isCasterChannelling = true;
        stackCount = 1;
    }

    public void OnRefresh(GameObject caster)
    {

    }

    public void OnRemove(GameObject caster)
    {

    }

    public void OnTick()
    {
        if (!isCasterChannelling)
        {
            return;
        }
        if (Time.time >= timeTillNextTick)
        {
            stackCount++;
            timeTillNextTick = Time.time + 0.1f;
        }
    }

    public void AddSecondParticle(VisualEffect fx)
    {
        chargeParticles[1] = fx;
    }

    public void OnPyreChannelEnded()
    {
        isCasterChannelling = false;
        stackCount++;

        foreach (VisualEffect effect in chargeParticles)
        {
            if (effect != null)
            {
                effect.SendEvent("OnChannelEnd");
            }
        }        
    }

    public void OnPyreChannelComplete()
    {
        foreach (VisualEffect effect in chargeParticles)
        {
            if (effect != null)
            {
                effect.SendEvent("OnProjectileLaunched");
            }
        }
    }
}
