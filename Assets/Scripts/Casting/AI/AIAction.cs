using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAction : castingAction
{
    //This class will handle AI inputs from the equipped profile

    [SerializeField] private IAI_Profile aiProfile; //The Logic this Opponent uses
    [SerializeField] private bool aiActive = false; //Determines if the AI has its logic running

    private float tickRate = 0.5f;
    private float timeSinceLastTick = 3.0f;

    //Casting Rules
    private bool isCasting = false;
    private bool isChannelling = false;

    private float castCooldown = 3.0f;
    private float timeSinceLastCast = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        GetComponents();

        aiProfile = new Test_Profile();
    }

    public void ActivateAI()
    {
        aiActive = true;
        if (aiProfile == null)
        {
            Debug.Log("Tried to activate an empty Opponent");
            DeactivateAI();
            return;
        }
        EventManager.current.onSpellCast += onSpellCast;


        aiProfile.onActivate(this);
        //Tick();
    }

    public void DeactivateAI()
    {
        aiActive = false;

        EventManager.current.onSpellCast -= onSpellCast;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Debug.Log(Time.time);
        if (casterStats.GetCurrentHP() == 0)
        {
            isDead = true;
        }

        if (aiActive && (Time.time > timeSinceLastTick + tickRate) && !isDead)
        {
            Tick();
        }
    }

    private void Tick()
    {
        timeSinceLastTick = Time.time;
        //Debug.Log("Tick");
        
        //Get Spells that can be cast this tick
        ICastable[] validspells = GetValidSpells();

        //Get the spell that the AIProfile reccommends to cast
        ICastable spell = aiProfile.onTick(validspells, GetCurrentMana(), localTarget.GetComponent<objectStats>().GetCurrentMana());
        //Debug.Log(spell);

        //If we are allowed to cast, do so.
        if (spell != null && Time.time > timeSinceLastCast + castCooldown)
        {
            CastSpell(spell);
        }
    }

    private ICastable[] GetValidSpells()
    {
        List<ICastable> vSpellList = new List<ICastable>();
        // Go through spells and combos available to the agent
        float mana = GetCurrentMana();
        bool cr = casterStats.castRestricted;
        if (mana == 0 || cr == true)
        {
            //If we have no mana or are fully silenced, then there will be no valid spells
            return null;
        }

        bool crO = casterStats.castRestrictedOffence;
        bool crD = casterStats.castRestrictedDefence;
        bool crU = casterStats.castRestirctedUtility;

        //Check comboList for valid spells
        for (int i = 0; i < spellInventory.comboList.Count; i++)
        {
            if (spellInventory.comboTimerList[i] >= Time.time && spellInventory.comboList[i].manacost <= mana) //Is the combo active atm? / Do we have enough mana?
            {
                ICastable combo = spellInventory.comboList[i];
                switch (combo.spellType)
                {
                    case SpellType.SPELL_TYPE_OFFENCE:
                        if (crO == false)
                        {
                            vSpellList.Add(combo);
                        }
                        break;
                    case SpellType.SPELL_TYPE_DEFENCE:
                        if (crD == false)
                        {
                            vSpellList.Add(combo);
                        }
                        break;
                    case SpellType.SPELL_TYPE_UTILITY:
                        if (crU == false)
                        {
                            vSpellList.Add(combo);
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        //Check spellList
        for (int i = 0; i < spellInventory.spellList.Count; i++)
        {
            if (spellInventory.spellList[i].manacost <= mana) //Do we have enough mana?
            {
                ICastable spell = spellInventory.spellList[i];
                switch (spell.spellType)
                {
                    case SpellType.SPELL_TYPE_OFFENCE:
                        if (crO == false)
                        {
                            vSpellList.Add(spell);
                        }
                        break;
                    case SpellType.SPELL_TYPE_DEFENCE:
                        if (crD == false)
                        {
                            vSpellList.Add(spell);
                        }
                        break;
                    case SpellType.SPELL_TYPE_UTILITY:
                        if (crU == false)
                        {
                            vSpellList.Add(spell);
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        return vSpellList.ToArray();
    }

    private void onSpellCast(object sender, SpellCastEventArgs e)
    {
        if (e.SpellCastEventCaster == gameObject) //if the caster is the agent
        {
            aiProfile.onAICastSpell(e.SpellCastEventSpell);
        }
        else if (e.SpellCastEventCaster == localTarget) // if the caster is the player
        {
            aiProfile.onPlayerCastSpell(e.SpellCastEventSpell);
        }
    }

    //Orders from Logic ================================================
    private void CastSpell(ICastable spell)
    {
        spell.Cast(gameObject, localTarget, null, null);
        EventManager.current.SpellCast(gameObject, localTarget, spell);
        timeSinceLastCast = Time.time;

        SpendResources(spell.manacost);
    }
}

public interface IAI_Profile //Interface for AI profiles
{
    void onActivate(AIAction action);
    ICastable onTick(ICastable[] validSpells, float agentMana, float playerMana);

    void onAICastSpell(ICastable spell);
    void onPlayerCastSpell(ICastable spell);
}
