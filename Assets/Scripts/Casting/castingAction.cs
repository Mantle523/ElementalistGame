using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class castingAction : MonoBehaviour
{
    protected SpellInventory spellInventory;
    protected objectStats casterStats;

    protected bool isDead = false; //cant cast while dead

    [SerializeField] protected GameObject localTarget = null;

    // Start is called before the first frame update
    void Start()
    {
                
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected void GetComponents()
    {
        spellInventory = gameObject.GetComponent<SpellInventory>();
        casterStats = gameObject.GetComponent<objectStats>();
    }

    protected void SpendResources(float manacost)
    {
        if (manacost != 0)
        {
            casterStats.SpendMana(manacost, gameObject);
        }
    }

    protected float GetCurrentMana()
    {
        return casterStats.GetCurrentMana();
    }
}
