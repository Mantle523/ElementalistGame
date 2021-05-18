using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;

public class Excalibur_Wave : GenericSpell, ICombo
{
    private static Excalibur_Wave _instance;
    private Excalibur_Wave()
    {

    }
    public static Excalibur_Wave GetInstance()
    {
        if (_instance != null) return _instance;
        return _instance = ScriptableObject.CreateInstance(typeof(Excalibur_Wave)) as Excalibur_Wave;
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

    public float endTime
    {
        get
        {
            return Time.time + comboDuration;
        }
    }

    public SpellType spellType
    {
        get
        {
            return SpellType.SPELL_TYPE_OFFENCE;
        }
    }

    public string[] comboTriggerGesture
    {
        get
        {
            return new string[] { "swipe_down_R", "swipe_up_R", "swipe_down_L", "swipe_up_L", "swipe_down_M", "swipe_up_M", "swipe_U", "swipe_C", "swipe_L", "diagonal_up_RL", "diagonal_up_LR", "diagonal_down_RL", "diagonal_down_LR" };
        }
    }

    public float comboDuration
    {
        get
        {
            return 10f;
        }
    }
    public bool comboPersists
    {
        get
        {
            return true;
        }
    }
    public RequireHand castHand
    {
        get
        {
            return RequireHand.REQUIRE_HAND_MATCH;
        }
    }

    public SpawnPos spawnPos
    {
        get
        {
            return SpawnPos.Pos_Forward;
        }
    }

    public float manacost
    {
        get
        {
            return 0;
        }
    }

    private float speed = 25f;
    private float damage = 6f;

    private Object Wave_Trail;
    private Object Wave_Land;

    public void PreLoad()
    {
        Wave_Trail = Resources.Load("Prefabs/ParticleSystems/Excalibur/Excalibur_Trail");
        Wave_Land = Resources.Load("Prefabs/ParticleSystems/Excalibur/Excalibur_Land");
    }

    public void Cast(GameObject caster, GameObject target, GameObject hand, string gesture)
    {
        //Create the projectile
        GameObject Projectile = CreateLinearProjectile(caster, target, spawnPos, hand, speed);
        LinearProjectile projscript = Projectile.GetComponent<LinearProjectile>();

        //The projectile can be launched from multiple swings, we need to rotate the projectile / fx to match this.
        float offsetRotation = GetWaveRotationOffset(gesture);
        Projectile.transform.Rotate(new Vector3(0, 0, offsetRotation));

        AttachFX(Wave_Trail, Projectile);

        projscript.RegisterDelegate(OnProjectileCollision);
    }

    public void OnProjectileCollision(GameObject proj, GameObject hitinfo, GameObject caster)
    {
        Debug.Log(proj.name + " has collided with " + hitinfo.name);
        //print("Now casting " + spellName + " effects!");        
        DamageTarget(caster, hitinfo, damage);
        FireFX(Wave_Land, proj);

        Excalibur_Wave_Modifier waveMod = new Excalibur_Wave_Modifier();
        CreateModifier(caster, hitinfo, waveMod, _spellProfile);
    }

    private float GetWaveRotationOffset(string gesture)
    {
        Dictionary<string, float> offsetDict = new Dictionary<string, float>();
        offsetDict.Add("swipe_down_R", 0f);
        offsetDict.Add("swipe_up_R", 0f);
        offsetDict.Add("swipe_down_L", 0f);
        offsetDict.Add("swipe_up_L", 0f);
        offsetDict.Add("swipe_down_M", 0f);
        offsetDict.Add("swipe_up_M", 0f);
        offsetDict.Add("swipe_U", 90f);
        offsetDict.Add("swipe_C", 90f);
        offsetDict.Add("swipe_L", 90f);
        offsetDict.Add("diagonal_up_RL", 45f);
        offsetDict.Add("diagonal_up_LR", -45f);
        offsetDict.Add("diagonal_down_RL", -45f);
        offsetDict.Add("diagonal_down_LR", 45f);

        return offsetDict[gesture];
    }
}

public class Excalibur_Wave_Modifier : GenericModifier, IModifier
{
    public string modifierName
    {
        get
        {
            return "Excalibur's Flame";
        }
    }

    public float Duration
    {
        get
        {
            return 2.0f;
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
            return true;
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

    private float damage = 1f;
    private float timeTillNextTick;

    public void OnCreate(GameObject caster, GameObject target)
    {
        modCaster = caster;
        modTarget = target;

        DamageTarget(modCaster, modTarget, damage);
        timeTillNextTick = Time.time + 1.0f;
    }

    public void OnRefresh(GameObject caster)
    {

    }

    public void OnRemove(GameObject caster)
    {

    }

    public void OnTick()
    {
        if (Time.time >= timeTillNextTick)
        {
            //Deal damage based on how many stacks the player has.
            float tickDamage = damage * stackCount;
            DamageTarget(modCaster, modTarget, tickDamage);
            timeTillNextTick = Time.time + 1.0f;
        }
    }
}
