using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;

public class MoltenBurst : GenericSpell, ISpell
{
    private static MoltenBurst _instance;
    private MoltenBurst()
    {

    }
    public static MoltenBurst GetInstance()
    {
        if (_instance != null) return _instance;
        return _instance = ScriptableObject.CreateInstance(typeof(MoltenBurst)) as MoltenBurst;
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

    public SpawnPos spawnPos
    {
        get
        {
            return SpawnPos.Pos_Barrier;
        }
    }

    public float manacost
    {
        get
        {
            return 35;
        }
    }

    private Object MoltenBurst_Launch;
    private float duration = 2.0f;
    private float width = 2.0f;
    private float height = 1.0f;
    private float health = 20;

    public void PreLoad()
    {
        MoltenBurst_Launch = Resources.Load("Prefabs/ParticleSystems/MoltenBurst/MoltenBurst_Launch");
    }

    public void Cast(GameObject caster, GameObject target, GameObject hand, string gesture)
    {
        //Create the barrier
        GameObject Barrier = CreateSimpleBarrier(caster, spawnPos, hand, width, height, duration, health);

        VisualEffect lava = FireFX( MoltenBurst_Launch, Barrier);
        lava.SetFloat("Duration", duration);
        var script = Barrier.GetComponentInChildren<SimpleBarrier>();
        script.RegisterDelegate(OnBarrierCollision);
    }

    public void OnBarrierCollision(GameObject Barrier, GameObject hitinfo)
    {

    }
}
