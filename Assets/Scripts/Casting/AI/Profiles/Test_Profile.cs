using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_Profile : IAI_Profile
{
    private AIAction agent;

    private int poiseVar = 2; //The profile uses this int to gauge whether / what it should be casting
    private int stanceVar = 8; //The profile uses this int to whether it should act offensively or defensivly

    private int varMax = 10;
    private int varMin = 0;
    private int varStep = 1;

    public void onActivate(AIAction action)
    {
        agent = action;
        //Reset variables
        poiseVar = 5;
        stanceVar = 5;
    }

    public ICastable onTick(ICastable[] validSpells, float agentMana, float playerMana)
    {
        //Debug.Log(validSpells[0] + "" + validSpells[1] + "" + validSpells[2] + "" + validSpells[3] + "");
        //Assesses the agent's options, and reccommends a validSpell (not garunteed to be cast by agent)
        if (validSpells == null || validSpells.Length == 0)
        {
            return null;
        }
        GetAdvantage(agentMana, playerMana); //Adjust poise based on mana advantage

        //Decide if we want to reccommend a cast this tick
        System.Random rnd = new System.Random();

        int rndm = rnd.Next(varMin, varMax+1);
        if (rndm > poiseVar)
        {
            //Nothing this tick
            return null;
        }

        //Decide if we want to cast offensive or defensive spells
        //Divide the valid spells into those categories
        List<ICastable> OSpells = new List<ICastable>();
        List<ICastable> DSpells = new List<ICastable>();

        foreach (ICastable spell in validSpells)
        {
            if (spell.spellType == SpellType.SPELL_TYPE_DEFENCE)
            {
                DSpells.Add(spell);
            }
            else
            {
                OSpells.Add(spell);
            }
        }
        
        int r = rnd.Next(varMin, varMax + 1);
        ICastable cast = GetReccommendation(OSpells, DSpells, r);
        if (cast != null)
        {
            return cast;
        }
        else
        {
            return null;
        }
    }

    public void onAICastSpell(ICastable spell)
    {
        //We cast a spell. If it was defensive, we want to switch back to attacks asap.
        if (spell.spellType == SpellType.SPELL_TYPE_DEFENCE)
        {
            AdjustVar(false, 10);
        }

        //We want to ai to not overwhelm the player with spells, so we'll decay the poise over time
        AdjustVar(true, -8);
    }

    public void onPlayerCastSpell(ICastable spell)
    {
        //Player cast a spell. We need to switch to defence quickly
        if (spell.spellType == SpellType.SPELL_TYPE_OFFENCE)
        {
            AdjustVar(false, -8);
            AdjustVar(true, 5);
        }
    }

    //If the agent has the mana advantage, increase poise, or reverse
    private void GetAdvantage(float a, float p)
    {
        float adv = a - p;
        float abs = Math.Abs(adv);
        //adjust poise, scaling with how large the advantage is
        int adjustFactor = 0;

        switch (adv)
        {
            case var expression when abs >= 50:
                adjustFactor = 1; break;
            case var expression when abs >= 20:
                adjustFactor = 1; break;
            case var expression when abs >= 0:
                adjustFactor = 0; break;
            default:
                break;
        }

        if (adv > 0)
        {
            adjustFactor = adjustFactor * -1;
        }

        //apply adjustment
        AdjustVar(true, adjustFactor);
    }

    private ICastable GetReccommendation(List<ICastable> OSpells, List<ICastable> DSpells, int r)
    {
        //Either list could be empty
        if (OSpells.Count == 0 && DSpells.Count == 0)
        {
            return null;
        }

        if (OSpells.Count == 0)
        {
            return ReccommendFromList(DSpells);
        }

        if (DSpells.Count == 0)
        {
            return ReccommendFromList(OSpells);
        }

        //Both lists have content. Choose one.
        if (r >= stanceVar)
        {
            return ReccommendFromList(DSpells);
        }
        else
        {
            return ReccommendFromList(OSpells);
        }
        
    }

    private ICastable ReccommendFromList(List<ICastable> spells)
    {
        //For now, we'll just return the first spell in the list
        return spells[0];
    }

    private void AdjustVar(bool Var, int i)
    {
        if (Var)
        {
            poiseVar = poiseVar + i;
            if (poiseVar > varMax)
            {
                poiseVar = varMax;
            }
            if (poiseVar < varMin)
            {
                poiseVar = varMin;
            }
        }
        else
        {
            stanceVar = stanceVar + i;
            if (stanceVar > varMax)
            {
                stanceVar = varMax;
            }
            if (stanceVar < varMin)
            {
                stanceVar = varMin;
            }
        }
    }
}