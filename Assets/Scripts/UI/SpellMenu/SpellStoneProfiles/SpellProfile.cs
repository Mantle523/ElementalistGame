using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Spell Profile", menuName = "Spell Profile")]
public class SpellProfile : ScriptableObject
{
    public new string name;
    public string description;

    public Sprite sprite;

    public string[] spellStrings; //The strings used to instantiate spell (and combos) used in the spell
}
