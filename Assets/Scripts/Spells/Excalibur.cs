using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;

public class Excalibur : GenericSpell, ISpell
{
    private static Excalibur _instance;
    private Excalibur()
    {

    }
    public static Excalibur GetInstance()
    {
        if (_instance != null) return _instance;
        return _instance = ScriptableObject.CreateInstance(typeof(Excalibur)) as Excalibur;
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
            return SpellType.SPELL_TYPE_UTILITY;
        }
    }

    public string[] spellTriggerGesture
    {
        get
        {
            return new string[] { "sun_praise_M" };
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
            return 30;
        }
    }

    private Object Excalibur_Create;
    private float swordDuration = 10.0f;

    public void PreLoad()
    {
        Excalibur_Create = Resources.Load("Prefabs/ParticleSystems/Excalibur/Excalibur_Create");
    }

    public void Cast(GameObject caster, GameObject target, GameObject hand, string gesture)
    {
        StartCombo("Excalibur_Wave", caster, hand);

        //Attach the Sword effect to the hand that casted the spell
        VisualEffect swordParticle = AttachFX(Excalibur_Create, hand);
        if (swordParticle.HasFloat("Duration"))
        {
            swordParticle.SetFloat("Duration", swordDuration);
        }
    }
}