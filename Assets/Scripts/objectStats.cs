using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objectStats : MonoBehaviour
{
    // Integer Stats
    private float hpBase = 100f;
    private float hpBonus = 0f;
    [SerializeField] private float hpCurrent;
    private float manaBase = 100f;
    private float manaBonus = 0f;
    [SerializeField] private float manaCurrent;

    // Boolean States
    public bool castRestricted;
    public bool castRestrictedOffence;
    public bool castRestrictedDefence;
    public bool castRestirctedUtility;

    // Modifier container
    public List<IModifier> modifierList = new List<IModifier>();    

    // Start is called before the first frame update
    void Start()
    {
        hpCurrent = hpBase;
        manaCurrent = manaBase;

        //Events
        EventManager.current.onBarrierHealthChange += onBarrierBlock;        

        //AddModifier("TestModifier", gameObject);
        //RemoveModifier("TestModifier", gameObject);
    }

    void OnDestroy()
    {
        EventManager.current.onBarrierHealthChange -= onBarrierBlock;
    }
        
    // Update is called once per frame
    void FixedUpdate()
    {
        if (hpCurrent <= 0)
        {
            //print(gameObject.name + " has died");
        }

        if (hpCurrent > hpBase)
        {
            hpCurrent = hpBase;
        }

        //Apply manaregen for this frame
        float manaThisFrame = 5 * Time.fixedDeltaTime;
        if ((manaCurrent + manaThisFrame) <= manaBase)
        {
            GainMana(manaThisFrame, null);
        }

        // Go through any applied modifiers to proc OnTick events, and remove them if they have run their full duration
        for (int i = 0; i < modifierList.Count; i++)
        {
            modifierList[i].OnTick();
            if (Time.time >= modifierList[i].FinishTime && modifierList[i].Duration != -1) //if the duration == -1, the modifier will not expire naturally
            {
                EventManager.current.ModifierRemoved(gameObject, null, modifierList[i]);
                modifierList[i].OnRemove(null);
                modifierList.Remove(modifierList[i]);
                print("Removed Modifier from slot " + i);
            }
        }
    }
    // Setting Base HP
    public void SetBaseHp(float newbase)
    {
        hpBase = newbase;
    }

    // Retrieving current hp and mana

    public float GetCurrentHP()
    {
        return hpCurrent;
    }

    public float GetCurrentMana()
    {
        return manaCurrent;
    }

    // Getting and Setting Bonus HP and Mana
    public float GetBonusHP()
    {
        return hpBonus;
    }

    public float GetBonusMana()
    {
        return manaBonus;
    }

    public void SetBonusHP(float bonus)
    {
        hpBonus = hpBonus + bonus;
        RecalculateCurrentHP(bonus);
    }

    public void SetBonusMana(float bonus)
    {
        manaBonus = manaBonus + bonus;
        RecalculateCurrentMana(bonus);
    }

    void RecalculateCurrentHP(float bonus)
    {
        // Get old hp percentage
        var hp_pct = hpCurrent / (hpBase + hpBonus - bonus);
        // Set Current hp to match old percentage
        hpCurrent = (hpBase + hpBonus) * hp_pct;
    }

    void RecalculateCurrentMana(float bonus)
    {
        // Get old mana percentage
        var mana_pct = manaCurrent / (manaBase + manaBonus - bonus);
        // Set Current mana to match old percentage
        manaCurrent = (manaBase + manaBonus) * mana_pct;
    }

    //Damage and Heal methods

    public void Damage(float damage, GameObject source)
    {
        hpCurrent = hpCurrent - damage;
        //print(gameObject.name + " received " + damage + " damage, credited to " + source.name);
        EventManager.current.HealthChange(gameObject, source, hpCurrent);
    }

    public void Heal(float heal, GameObject source)
    {
        hpCurrent = hpCurrent + heal;
        EventManager.current.HealthChange(gameObject, source, hpCurrent);
    }

    public void SpendMana(float manaLoss, GameObject source)
    {
        manaCurrent = manaCurrent - manaLoss;
        EventManager.current.ManaChange(gameObject, source, manaCurrent);
        //print(gameObject.name + " lost " + manaLoss + " mana, credited to " + source.name);
    }

    public void GainMana(float manaGain, GameObject source)
    {
        manaCurrent = manaCurrent + manaGain;
        EventManager.current.ManaChange(gameObject, source, manaCurrent);
    }

    //Events    
    private void onBarrierBlock(object sender, HealthChangeEventArgs e)
    {
        if (e.HealthChangeEventRecipient == gameObject)
        {
            GainMana(e.HealthChangeEventValue, e.HealthChangeEventSource);
        }
    }

    //Modifier Methods
    public void AddModifier(IModifier modifier, GameObject caster, SpellProfile spellProfile) //Takes a freshly instanced modifier, sorts out its duration etc
    {
        //Type type = Type.GetType(mod);
        //dynamic modifier = Activator.CreateInstance(type);
        if (modifier.Duration != -1)
        {
            modifier.FinishTime = Time.time + modifier.Duration;
        }
        else
        {
            modifier.FinishTime = -1;
        }
        // Check to see if there is already an instance of this debuff on the target
        for (int i = 0; i < modifierList.Count; i++)
        {
            if (modifierList[i].modifierName == modifier.modifierName)
            {
                // If it is present, refresh the existing modifier's duration                
                RefreshModifier(modifierList[i], caster);
                //return modifierList[i];
                return;
            }
        }
        modifierList.Add(modifier);
        modifier.modCaster = caster;
        modifier.modTarget = gameObject;
        if (modifier.canStack == true)
        {
            modifier.stackCount = 1;
        }
        else
        {
            modifier.stackCount = 0;
        }

        modifier.OnCreate(caster, gameObject);

        EventManager.current.ModifierCreated(gameObject, caster, modifier, spellProfile);

        //print(modifier.modifierName);
        //print("Added new modifier");

        //return modifier;
        return;
    }

    protected void RefreshModifier(IModifier modifier, GameObject caster)
    {
        if (modifier.Duration == -1 || modifier.FinishTime == -1)
        {
            //Panic
            Debug.Log("Attempted to refresh a duration-less modifier");
            return;
        }
        modifier.FinishTime = Time.time + modifier.Duration;
        if (modifier.canStack == true)
        {
            //If the modifier stacks, increase the stackcount by 1
            IncrementModifierStack(modifier);
        }
        modifier.OnRefresh(caster);

        EventManager.current.ModifierRefreshed(gameObject, caster, modifier);
    }

    protected void IncrementModifierStack(IModifier modifier)
    {
        modifier.stackCount = modifier.stackCount + 1;
    }

    public void RemoveModifier(string mod, GameObject caster)
    {
        Type searchType = Type.GetType(mod);        
        for (int i = 0; i < modifierList.Count; i++)
        {
            if (modifierList[i].GetType() == searchType)
            {
                modifierList[i].OnRemove(caster);
                modifierList.Remove(modifierList[i]);

                EventManager.current.ModifierRemoved(gameObject, caster, modifierList[i]);
            }
        }
    }

    public IModifier FindModifier(string mod)
    {
        Type searchType = Type.GetType(mod);
        for (int i = 0; i < modifierList.Count; i++)
        {
            if (modifierList[i].GetType() == searchType)
            {
                return modifierList[i];
            }
        }
        return null;
    }
}
