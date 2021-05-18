using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellManager : MonoBehaviour
{
    //Spell Library is a local record of all spells that can appear in the match,
    //This allows us to pre-load any effects to be used by those spells.

    public static SpellManager current;
    public static List<ICastable> spellLibrary = new List<ICastable>();

    void Awake()
    {
        current = this;
    }

    
    public void BuildSpellLibrary(List<GameObject> playerRoster)
    {
        // Takes all spells selected by players and loads them as a singleton instance.
        // These instances are then stored in the player's SpellInventory, and the SpellLibrary.

        foreach (GameObject player in playerRoster)
        {
            
            SpellInventory playerInventory = player.GetComponent<SpellInventory>();           
            foreach (SpellProfile spellProfile in playerInventory.spellRefList)
            {
                if (spellProfile != null)
                {
                    string[] spellStrings = spellProfile.spellStrings;
                    if (spellStrings.Length == 1)
                    {
                        InstantiateSpell(spellStrings[0], playerInventory);
                    }
                    else
                    {
                        foreach (string spellRef in spellStrings)
                        {
                            InstantiateSpell(spellRef, playerInventory);
                        }
                    }
                }                                              
            }
        }

        //Once we've iterated through every castable in the session, we'll send out an event for anything that's waiting for a full list of castables.
        EventManager.current.SpellLibraryComplete();
        PreLoadSpellData();
        //print(spellLibrary.Count);
    }

    private void InstantiateSpell(string spellRef, SpellInventory playerInventory)
    {
        //Turn the given string into the class we're looking for
        dynamic type = Type.GetType(spellRef);
        //print(type);
        if (type.GetMethod("GetInstance") != null)
        {
            dynamic spell = type.GetMethod("GetInstance").Invoke(null, null);

            //Add this Instance to the respective player's SpellInventory or ComboInventory
            if (spell is ISpell)
            {
                playerInventory.spellList.Add(spell);
            }
            else if (spell is ICombo)
            {
                playerInventory.comboList.Add(spell);
            }
            else
            {
                Debug.Log("type error");
            }


            //If it's a new instance, add it to spellLibrary 

            if (CheckNewSpell(spell) == true)
            {
                spellLibrary.Add(spell);
            }
        }
        else
        {
            Debug.Log("Could not find GetInstance method for " + spellRef);
        }
    }

    private bool CheckNewSpell(ICastable spell)
    {
        foreach (ICastable item in spellLibrary)
        {
            if (item == spell )
                return false;
        }
        return true;
    }

    private void PreLoadSpellData()
    {
        foreach (ICastable castable in spellLibrary)
        {
            castable.PreLoad();
        }
    }
}
