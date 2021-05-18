using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;

public abstract class GenericSpell : ScriptableObject
{
    //private Vector3 handSpawnOffsetRot = new Vector3(45, 0, 0);
    //private Vector3 handSpawnOffsetPos = new Vector3(0.0, -0.02f, -0.1f);    

    private static GameObject linearProjectile;
    private static GameObject simpleBarrier;

    void OnEnable()
    {
        linearProjectile = Resources.Load("Prefabs/LinearProjectile") as GameObject;
        simpleBarrier = Resources.Load("Prefabs/SimpleBarrier") as GameObject;
    }

    protected GameObject FindSpawnPosition(GameObject caster, SpawnPos data, GameObject hand)
    {
        if (data == SpawnPos.Hand && hand != null)
        {
            return hand;
        }
        GameObject spawnPosObj;
        switch (data)
        {
            case SpawnPos.Pos_Forward:
                spawnPosObj = caster.transform.Find("Pos_Forward").gameObject;
                return spawnPosObj;
            case SpawnPos.Pos_Origin:
                spawnPosObj = caster.transform.Find("Pos_Origin").gameObject;
                return spawnPosObj;
            case SpawnPos.Pos_Barrier:
                spawnPosObj = caster.transform.Find("Pos_Barrier").gameObject;
                return spawnPosObj;
            case SpawnPos.Pos_Overhead:
                spawnPosObj = caster.transform.Find("Pos_Overhead").gameObject;
                return spawnPosObj;
            default:
                break;
        }
        return null;
    }

    protected Vector3 FindSpawnPosition(GameObject caster, SpawnPos data)
    {        
        Vector3 spawnPosObj;
        switch (data)
        {
            case SpawnPos.Pos_Forward:
                spawnPosObj = caster.transform.position + caster.transform.forward + new Vector3(0,1.5f, 0);
                return spawnPosObj;
            case SpawnPos.Pos_Origin:
                spawnPosObj = caster.transform.position + new Vector3(0, 1, 0);
                return spawnPosObj;
            case SpawnPos.Pos_Barrier:
                spawnPosObj = caster.transform.position + (caster.transform.forward * 2) + new Vector3(0, -1f, 0);
                return spawnPosObj;
            case SpawnPos.Pos_Overhead:
                spawnPosObj = caster.transform.position + new Vector3(0, 2.3f, 0);
                return spawnPosObj;
            default:
                break;
        }
        return caster.transform.position;
    }

    //=================================================================================================
    //================== Common use functions =========================================================
    //=================================================================================================

    protected void DamageTarget(GameObject caster, GameObject target, float damage)
    {
        //Target could either be the a player, or a barrier, so we need to account for that.
        var targetStats = target.GetComponent<objectStats>();
        if (targetStats == null)
        {
            targetStats = target.GetComponentInParent<objectStats>();
        }
        if (targetStats != null)
        {
            targetStats.Damage(damage, caster);
        }
    }

    protected GameObject CreateLinearProjectile(GameObject caster, GameObject target, SpawnPos spawnPosData, GameObject hand, float speed)
    {
        //if using hands, return hands
        //if using "spawnPos" object, get the pre-determined vector3
        

        GameObject LinProjectile;
        if (spawnPosData == SpawnPos.Hand && hand != null)
        {
            GameObject LaunchPoint = hand.transform.Find("ParticleLaunchPoint").gameObject;
            LinProjectile = GameObject.Instantiate(linearProjectile, LaunchPoint.transform.position, LaunchPoint.transform.rotation) as GameObject;
        }
        else
        {
            Vector3 spawnPos = FindSpawnPosition(caster, spawnPosData);
            LinProjectile = GameObject.Instantiate(linearProjectile, spawnPos, caster.transform.rotation) as GameObject;
        }
        var lpScript = LinProjectile.GetComponent<LinearProjectile>();

        
        lpScript.rb.AddForce(LinProjectile.transform.forward * speed * 50);

        lpScript.Activate(caster, target, speed);

        return LinProjectile;
    }

    protected GameObject CreateSimpleBarrier(GameObject caster, SpawnPos spawnPosData, GameObject hand,  float width, float height, float duration, float health)
    {
        GameObject SimBarrier;
        if (spawnPosData == SpawnPos.Hand)
        {
            GameObject LaunchPoint = hand.transform.Find("ParticleLaunchPoint").gameObject;
            SimBarrier = GameObject.Instantiate(simpleBarrier, LaunchPoint.transform.position, LaunchPoint.transform.rotation) as GameObject;

        }
        else
        {
            Vector3 spawnPos = FindSpawnPosition(caster, spawnPosData);
            SimBarrier = GameObject.Instantiate(simpleBarrier, spawnPos, caster.transform.rotation) as GameObject;
        } 
        //Debug.Log(spawnPos);
        
        var collider = SimBarrier.GetComponent<BoxCollider>();
        var script = SimBarrier.GetComponent<SimpleBarrier>();
        var stats = SimBarrier.GetComponent<objectStats>();

        // Adjust hitbos to match model
        collider.size = new Vector3(width, 2*height, 1.0f);
        // Setup variables
        script.Activate(caster, duration);
        // Set health
        stats.SetBaseHp(health);

        return SimBarrier;
    }

    protected void StartCombo(string spellClass, GameObject caster, GameObject hand)
    {
        //First, we need to make sure we have the spell we're trying to combo into
        List<ICombo> playerComboList = caster.GetComponent<SpellInventory>().comboList;
        float[] playerComboTimers = caster.GetComponent<SpellInventory>().comboTimerList;
        GameObject[] playerComboHand = caster.GetComponent<SpellInventory>().comboHandCast;
        if (playerComboList == null || playerComboTimers == null)
        {
            Debug.Log("Error, attempted to start a combo, but could not find caster's spellList or the timer list");
            return;
        }
        for (int i = 0; i < playerComboList.Count; i++)
        {
            if (playerComboList[i].GetType().Name == spellClass)
            {
                Debug.Log("Starting Combo!");
                // Now, we need to retrieve the gestures and duration from the spell
                ICombo comboSpell = playerComboList[i];
                // Get the duration this combo lasts for
                float cDuration = comboSpell.comboDuration;
                // Overwrite the timer for this combo to the new expiry time
                playerComboTimers[i] = Time.time + cDuration;
                // Overwrite the recored hand used to start the combo
                if (hand != null)
                {
                    playerComboHand[i] = hand;
                }
            }
        }        
    }

    //================================================================================================
    //============================== Modifier - related functions ====================================
    //================================================================================================
    protected void CreateModifier(GameObject caster, GameObject target, IModifier Modifier, SpellProfile spellProfile)
    {
        //Target could either be the a player, or a barrier, so we need to account for that.
        // This is pretty much just a shell function to make sure we send the modifier to the right objectStats instance
        var targetStats = target.GetComponent<objectStats>();
        if (targetStats == null)
        {
            targetStats = target.GetComponentInParent<objectStats>();
        }
        if (targetStats != null)
        {
            targetStats.AddModifier(Modifier, caster, spellProfile);
            return;
        }
        return;
    }

    protected IModifier FindModifier(GameObject target, string ModifierName)
    {
        var targetStats = target.GetComponent<objectStats>();
        if (targetStats == null)
        {
            targetStats = target.GetComponentInParent<objectStats>();
        }
        if (targetStats != null)
        {
            return targetStats.FindModifier(ModifierName);
        }
        return null;
    }

    protected void RemoveModifier(GameObject caster, GameObject target, string ModifierName)
    {
        //Target could either be the a player, or a barrier, so we need to account for that.
        var targetStats = target.GetComponent<objectStats>();
        if (targetStats == null)
        {
            targetStats = target.GetComponentInParent<objectStats>();
        }
        if (targetStats != null)
        {
            targetStats.RemoveModifier(ModifierName, caster);
        }
    }

    //==================================================================================================
    //======================================== Particle - related functions ============================
    //==================================================================================================

    protected VisualEffect AttachFX(Object fx, GameObject source)
    {
        GameObject particleObject; 
        if (source == null || source.name == "LeftHand" || source.name == "RightHand")
        {
            Transform LaunchPoint = source.transform.Find("ParticleLaunchPoint");
            particleObject = GameObject.Instantiate(fx, LaunchPoint) as GameObject;
        }
        else
        {
            particleObject = GameObject.Instantiate(fx, source.transform) as GameObject;
        }
        return particleObject.GetComponent<VisualEffect>();
    }

    protected VisualEffect FireFX(Object fx, GameObject source)
    {
        //Debug.Log("FireFX at object");
        //Single use event fired from the location of the source at the time the event was called.
        GameObject particleObject;
        if (source == null)
        {
            return null;
        }
        // If the fx are coming from the hands, we need to adjust the position fx.
        if (source.name == "LeftHand" || source.name == "RightHand")
        {
            Transform LaunchPoint = source.transform.Find("ParticleLaunchPoint");
            particleObject = GameObject.Instantiate(fx, LaunchPoint.position, LaunchPoint.rotation) as GameObject;
        }
        else
        {
            particleObject = GameObject.Instantiate(fx, source.transform.position, source.transform.rotation) as GameObject;
        }
        return particleObject.GetComponent<VisualEffect>();
    }
}

public enum SpellCastType
{
    SPELL_CAST_SIMPLE, //Gesture 'n' go
    SPELL_CAST_CHANNELLED, //Hand must remain on the final node in the gesture for a duration before the spell is cast
    SPELL_CAST_DUALCAST, //Spell is cast by simlutaneous, mirrored gestures
    SPELL_CAST_DUALCHANNELLED //As above, but also channelled
}

public enum RequireHand
{
    REQUIRE_HAND_NONE, //Can be cast in either hand
    REQUIRE_HAND_MATCH, //Can only be cast by the hand that started the combo
    REQUIRE_HAND_ALTERNATE, //Can only be cast by hand that did not start the combo
    REQUIRE_HAND_LEFT, //Can only be cast by the left hand
    REQUIRE_HAND_RIGHT //Can only be cast by the right hand
}

public enum SpellType
{
    SPELL_TYPE_OFFENCE, //Attacks,
    SPELL_TYPE_DEFENCE, //Barriers, 
    SPELL_TYPE_UTILITY //Buffs, Debuffs, health and Misc effects
}

public enum SpawnPos
{
    Pos_Forward, //Fixed position just ahead of the player
    Pos_Barrier, //Ideal Location to place a barrier
    Pos_Overhead, // Fixed above the player
    Pos_Origin, //Player Origin, might not be useful
    Hand // Use casting hand's position
}

public interface ICastable
{
    SpellProfile spellProfile { get; }
    SpellCastType spellCastType { get; }
    SpellType spellType { get; }
    SpawnPos spawnPos { get; }

    float manacost { get; }
    void PreLoad();
    void Cast(GameObject caster, GameObject target, GameObject hand, string gesture);
}

public interface ISpell : ICastable
{
    string[] spellTriggerGesture { get; } // Array of all gestures that can cast this spell
}

public interface ICombo : ICastable
{
    string[] comboTriggerGesture { get; } // If a combo object is created, pass these gestures into that object
    float comboDuration { get; } // How long a combo lasts
    bool comboPersists { get; }
    RequireHand castHand { get; }
    float endTime { get; }
}

public interface IChannel
{
    float channelDuration { get; }

    void onChannelTick(GameObject caster, GameObject target, GameObject hand, float channelTime);
    void onChannelComplete(GameObject caster, GameObject target, GameObject hand); //Channelled spell is channelled for the full duration
    void onChannelInterrupt(GameObject caster, GameObject target, GameObject hand, float channelTime); //Channelled spell is not completed
    void onChannelEnd(GameObject caster, GameObject target, GameObject hand, float channelTime); //Channelled spell ends
}

public interface IDualCastable
{
    void DualCast(GameObject caster, GameObject target, GameObject[] hands, string gesture);
}

public interface IDualChannel : IDualCastable
{
    float channelDuration { get; }

    void onDualChannelTick(GameObject caster, GameObject target, GameObject[] hands, float channelTime);
    void onDualChannelComplete(GameObject caster, GameObject target, GameObject[] hands); //Channelled spell is channelled for the full duration
    void onDualChannelInterrupt(GameObject caster, GameObject target, GameObject[] hands, float channelTime); //Channelled spell is not completed
    void onDualChannelEnd(GameObject caster, GameObject target, GameObject[] hands, float channelTime); //Channelled spell ends
}

public interface IModifier
{
    void OnCreate(GameObject caster, GameObject victim);
    void OnRefresh(GameObject caster);
    void OnRemove(GameObject caster);
    void OnTick();

    string modifierName { get; }    
    float Duration { get; }
    float FinishTime { get; set; }
    bool canStack { get; }
    int stackCount { get; set; }
    GameObject modCaster { get; set; }
    GameObject modTarget { get; set; }
}

public class GenericModifier
{
    // Some useful functions likely to be used by many modifiers
    //=================================================================================================
    //================== Common use functions =========================================================
    //=================================================================================================

    protected void DamageTarget(GameObject caster, GameObject target, float damage)
    {
        //Target could either be the a player, or a barrier, so we need to account for that.
        var targetStats = target.GetComponent<objectStats>();
        if (targetStats == null)
        {
            targetStats = target.GetComponentInParent<objectStats>();
        }
        if (targetStats != null)
        {
            targetStats.Damage(damage, caster);
        }
    }

    protected void StartCombo(string spellClass, GameObject caster)
    {
        //  IMPORTANT!!! Combos started through modifiers dont have a hand associated with them. They will only be able to cast-
        //  -Combos that do not require a specific / matching hand
        
        //First, we need to make sure we have the spell we're trying to combo into
        List<ICombo> playerComboList = caster.GetComponent<SpellInventory>().comboList;
        float[] playerComboTimers = caster.GetComponent<SpellInventory>().comboTimerList;
        GameObject[] playerComboHand = caster.GetComponent<SpellInventory>().comboHandCast;
        if (playerComboList == null || playerComboTimers == null)
        {
            Debug.Log("Error, attempted to start a combo, but could not find caster's spellList or the timer list");
            return;
        }
        for (int i = 0; i < playerComboList.Count; i++)
        {
            if (playerComboList[i].GetType().Name == spellClass)
            {
                Debug.Log("Starting Combo!");
                // Now, we need to retrieve the gestures and duration from the spell
                ICombo comboSpell = playerComboList[i];
                // Get the duration this combo lasts for
                float cDuration = comboSpell.comboDuration;
                // Overwrite the timer for this combo to the new expiry time
                playerComboTimers[i] = Time.time + cDuration;
                // Overwrite the recored hand used to start the combo
                 playerComboHand[i] = null;                
            }
        }        
    }

    //================================================================================================
    //============================== Modifier - related functions ====================================
    //================================================================================================
    protected void CreateModifier(GameObject caster, GameObject target, IModifier Modifier, SpellProfile spellProfile)
    {
        //Target could either be the a player, or a barrier, so we need to account for that.
        var targetStats = target.GetComponent<objectStats>();
        if (targetStats == null)
        {
            targetStats = target.GetComponentInParent<objectStats>();
        }
        if (targetStats != null)
        {
            targetStats.AddModifier(Modifier, caster, spellProfile);
        }
    }

    protected IModifier FindModifier(GameObject target, string ModifierName)
    {
        var targetStats = target.GetComponent<objectStats>();
        if (targetStats == null)
        {
            targetStats = target.GetComponentInParent<objectStats>();
        }
        if (targetStats != null)
        {
            return targetStats.FindModifier(ModifierName);
        }
        return null;
    }

    protected void RemoveModifier(GameObject caster, GameObject target, string ModifierName)
    {
        //Target could either be the a player, or a barrier, so we need to account for that.
        var targetStats = target.GetComponent<objectStats>();
        if (targetStats == null)
        {
            targetStats = target.GetComponentInParent<objectStats>();
        }
        if (targetStats != null)
        {
            targetStats.RemoveModifier(ModifierName, caster);
        }
    }
}

/*
public class EXspell : GenericSpell, ISpell
{
    private static EXspell _instance;
    private EXspell()
    {

    }
    public static EXspell GetInstance()
    {
        if (_instance != null) return _instance;
        return _instance = new EXspell();
    }


    public string spellName
    {
        get
        {
            return "EXspell";
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

    public string spawnPos
    {
        get
        {
            return "Pos_Forward";
        }
    }

    public int manacost
    {
        get
        {
            return 0;
        }
    }

    private float speed = 20.0f;
    private int damage = 10;

    private Object FireBall_Trail = Resources.Load("Prefabs/ParticleSystems/FireBall/FireBall_Trail");
    private Object FireBall_Launch = Resources.Load("Prefabs/ParticleSystems/FireBall/FireBall_Launch");
    private Object FireBall_Land = Resources.Load("Prefabs/ParticleSystems/FireBall/FireBall_Land");

    private GameObject Projectile;
    //private LinearProjectle projScript;

    public void Cast(GameObject caster, GameObject target, GameObject hand)
    {
        //Find the start location for the projectile
        GameObject spawnPosObj = FindSpawnPosition(caster,spawnPos);

        Projectile = CreateLinearProjectile(caster, target, hand, speed);
        var projScript = Projectile.GetComponent<LinearProjectile>();

        AttachTrailFX(FireBall_Trail, Projectile);
        FireFX(FireBall_Launch, hand);
        //GameObject SimBarrier = GameObject.Instantiate(Resources.Load("Prefabs/SimpleBarrier"), spawnPos.transform.position, spawnPos.transform.rotation) as GameObject;

        projScript.RegisterDelegate(OnProjectileCollision);

        // Probably need an OnCast event here (Put mana costs etc in said event)
        StartCombo("EXwindow", caster, hand);
    }

    public void OnProjectileCollision(GameObject proj, GameObject hitinfo, GameObject caster)
    {
        Debug.Log(proj.name + " has collided with " + hitinfo.name);
        //print("Now casting " + spellName + " effects!");        

        //Find the target's stats
        var targetStats = hitinfo.GetComponent<objectStats>();
        if (targetStats == null)
        {
            targetStats = hitinfo.GetComponentInParent<objectStats>();
        }
        if (targetStats != null)
        {
            targetStats.Damage(damage, caster);
        }
        FireFX(FireBall_Land, proj);
    }
}

public class EXshield : GenericSpell, ISpell
{
    private static EXshield _instance;
    private EXshield()
    {

    }
    public static EXshield GetInstance()
    {
        if (_instance != null) return _instance;
        return _instance = new EXshield();
    }

    public string spellName
    {
        get
        {
            return "EXshield";
        }
    }

    public SpellType spellType
    {
        get
        {
            return SpellType.SPELL_TYPE_DEFENCE;
        }
    }

    public string[] spellTriggerGesture
    {
        get
        {
            return new string[] { "low_punch_R", "low_punch_L" };
        }
    }
    
    public string spawnPos
    {
        get
        {
            return "Pos_Forward";
        }
    }

    public int manacost
    {
        get
        {
            return 0;
        }
    }

    private float width = 2.0f;
    private float height = 4.0f;
    private float duration = 3.0f;
    private int health = 60;

    private GameObject Shield;

    public void Cast(GameObject caster, GameObject target, GameObject hand)
    {
        GameObject spawnPosObj = FindSpawnPosition(caster, spawnPos);

        Shield = CreateSimpleBarrier(caster, target, spawnPosObj, width, height, duration, health);
        var script = Shield.GetComponentInChildren<SimpleBarrier>();
        script.RegisterDelegate(OnBarrierCollision);

        void OnBarrierCollision(GameObject barrier, GameObject hitInfo)
        {

        }
    }
}

public class EXwindow : GenericSpell, ICombo
{
    private static EXwindow _instance;
    private EXwindow()
    {

    }
    public static EXwindow GetInstance()
    {
        if (_instance != null) return _instance;
        return _instance = new EXwindow();
    }

    public float endTime
    {
        get
        {
            return Time.time + comboDuration;
        }
    }

    public string spellName
    {
        get
        {
            return "EXwindow";
        }
    }

    public SpellType spellType
    {
        get
        {
            return SpellType.SPELL_TYPE_UTILITY;
        }
    }    
    
    public string[] comboTriggerGesture
    {
        get
        {
            return new string[] { "low_punch_R", "low_punch_L" };
        }
    }

    public float comboDuration
    {
        get
        {
            return 3.0f;
        }
    }
    public bool comboPersists
    {
        get
        {
            return false;
        }
    }
    public RequireHand castHand
    {
        get
        {
            return RequireHand.REQUIRE_HAND_NONE;
        }
    }

    public string spawnPos
    {
        get
        {
            return "Pos_Origin";
        }
    }

    public int manacost
    {
        get
        {
            return 20;
        }
    }

    public void Cast(GameObject caster, GameObject target, GameObject hand)
    {
        //print("Casted the window spell");
    }
}


    */
