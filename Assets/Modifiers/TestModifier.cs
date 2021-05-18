using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestModifier : GenericModifier, IModifier
{
    public string modifierName
    {
        get
        {
            return "Test_Modifier";
        }
    }

    public float Duration
    {
        get
        {
            return 8.0f;
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

    public void OnCreate(GameObject caster, GameObject target)
    {
        modCaster = caster;
        modTarget = target;
    }

    public void OnRefresh(GameObject caster)
    {

    }

    public void OnRemove(GameObject caster)
    {

    }

    public void OnTick()
    {

    }
}
