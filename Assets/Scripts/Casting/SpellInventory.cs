using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TO DO: MOVE ANYTHING NOT DIRECTLY RELATED TO SPELL / COMBO LIBRARY TO gestureAction.
//This will be needed since we're gonna want to cast spells without gesturing (Network, Enemy AI).
//So having to wade through all this gesture code to do so is asking for trouble.

public class SpellInventory : MonoBehaviour
{
    //public Spell[] spells;

    public List<ISpell> spellList = new List<ISpell>(); // Contains a list of all normal spells this player can use
    public List<ICombo> comboList = new List<ICombo>(); // Contains a list of any temporary / conditional spells the player can use
    public List<SpellProfile> spellRefList = new List<SpellProfile>(); // A list of the names of all spell (permanent and temporary) that the player can use
    public float[] comboTimerList; // An array of times that each temporary spells can be used until
    public GameObject[] comboHandCast; // An array that records which hand activated this combo (using StartCombo();)

    //public GameObject localTarget;        

    public void QuickAddSpells()
    {
        //TEMP - Adding in some placeholder castables for testing
        //spellRefList.Add("FireBall");
        //spellRefList.Add("Excalibur");
        //spellRefList.Add("Excalibur_Wave");
        //spellRefList.Add("MoltenBurst");
        //spellRefList.Add("Pyre");
    }

    //private bool Castoverride = false;

    // Start is called before the first frame update
    void Start()
    {
        //Events
        EventManager.current.onGesturePerformed += onGesturePerformed;
        EventManager.current.onSpellLibraryComplete += onSpellLibraryComplete;
        EventManager.current.onAddSpell += AddSpell;
        EventManager.current.onRemoveSpell += RemoveSpell;
        //EventManager.current.onGestureNodeExit += onGestureNodeExit;
    }

    private void AddSpell(object sender, SpellInventoryChangeArgs e)
    {
        if (gameObject != e.SpellInventoryChangeCaster)
        {
            return;
        }

        SpellProfile profile = e.SpellInventoryChangeSpell;

        if (!spellRefList.Contains(profile))
        {
            spellRefList.Add(profile);
            Debug.Log("Added new spell, ready to be instanciated");
        }
    }

    private void RemoveSpell(object sender, SpellInventoryChangeArgs e)
    {
        //Dont use the index, there's no garuntee that the spell sits in the same slot that it sits in the UI
        if (gameObject != e.SpellInventoryChangeCaster)
        {
            return;
        }

        SpellProfile profile = e.SpellInventoryChangeSpell;
        if (spellRefList.Contains(profile))
        {
            spellRefList.Remove(profile);
            Debug.Log("Removed spell");
            return;
        }

        Debug.Log("Spell was not removed");
        return;
    }

    private void onSpellLibraryComplete(object sender, EventArgs e)
    {
        //Setup some data arrays regarding a player's combo spells

        int size = comboList.Count;
        comboTimerList = new float[size];
        comboHandCast = new GameObject[size];

        for (int i = 0; i < size; i++)
        {
            comboTimerList[i] =  0f;
            comboHandCast[i] = null;
        }
    }

    
    private void onGesturePerformed(GameObject caster, string gesture, GameObject hand)
    {
        if (caster == gameObject)
        {
            ParseSpellList(gesture, hand);
        }
    }
    

    void Update()
    {
        // [EVEN MORE TEMPORARY]
        // Auto-cast a spell for testing purposes
        /*
        if (gameObject.name == "Target" && Castoverride == false && Time.time > 2f)
        {
            InitiateCast(gameObject, localTarget, spellList[0]);
            Castoverride = true;
        }
        */        
    }

    private void ParseSpellList(string gesture, GameObject hand)
    {
        // Combo windows take priority, so we check them first
        for (int c = 0; c < comboList.Count; c++)
        {
            if (comboTimerList[c] >= Time.time) //Check if this combo has expired for this player
            {
                ICombo combo = comboList[c];
                for (int i = 0; i < combo.comboTriggerGesture.Length; i++)
                {
                    if (combo.comboTriggerGesture[i] == gesture)
                    {
                        if (CheckRequireHand(combo, hand, c)) // If we specify a hand, we check for this here
                        {
                            EventManager.current.SpellInventoryCast(gameObject, comboList[c], hand, gesture);
                            //If the combo is a one-and-done cast, and reset its expiry time
                            if (combo.comboPersists == false)
                            {
                                comboTimerList[c] = 0f;
                            }
                            return;
                        }                        
                    }
                }
            }
        }

        foreach (ISpell spell in spellList)
        {
            for (int i = 0; i < spell.spellTriggerGesture.Length; i++)
            {
                if (spell.spellTriggerGesture[i] == gesture)
                {
                    EventManager.current.SpellInventoryCast(gameObject, spell, hand, gesture);
                }
            }
        }
    }

    private bool CheckRequireHand(ICombo combo, GameObject hand, int index)
    {
        switch (combo.castHand)
        {
            case RequireHand.REQUIRE_HAND_NONE:
                return true;
            case RequireHand.REQUIRE_HAND_MATCH:
                if (hand == comboHandCast[index])
                {
                    return true;
                }
                else return false;
            case RequireHand.REQUIRE_HAND_ALTERNATE:
                if (hand == comboHandCast[index])
                {
                    return false;
                }
                else return true;
            case RequireHand.REQUIRE_HAND_LEFT:
                if (IsLeftHand(hand) == true)
                {
                    return true;
                }
                else return false;
            case RequireHand.REQUIRE_HAND_RIGHT:
                if (IsLeftHand(hand) == false)
                {
                    return true;
                }
                else return false;
            default:
                Debug.Log("CheckRequireHand - Rogue entry");
                return false;
        }
    }

    private bool IsLeftHand(GameObject hand)
    {
        if (hand.name == "LeftHand")
        {
            return true;
        }
        if (hand.name == "RightHand")
        {
            return false;
        }
        Debug.Log("Error, unexpected object passed to IsLeftHand()");
        return false;
    }   
}