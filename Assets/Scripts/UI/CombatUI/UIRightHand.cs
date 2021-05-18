using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRightHand : MonoBehaviour, IUILayout
{
    [SerializeField] private Slider slider = null;//Assigned in Inspector
    [SerializeField] private GameObject subject = null;//Assigned in Inspector //To whom the bar refers to  

    private bool isActive = false;

    void Start()
    {
        
    }

    public void ActivateUILayout()
    {
        Debug.Log("Hand on");

        if (isActive)
        {
            return;
        }

        isActive = true;

        EventManager.current.onManaChange += onManaChange;
        Calibrate(100);

        EventManager.current.onAddSpell += AddSpell;
        EventManager.current.onRemoveSpell += RemoveSpell;

        //Read the spellRefList, set up inventory for editing
        ReadSpellRefList();
    }

    public void DeactivateUILayout()
    {
        if (!isActive)
        {
            return;
        }

        isActive = false;

        EventManager.current.onManaChange -= onManaChange;
        EventManager.current.onAddSpell -= AddSpell;
        EventManager.current.onRemoveSpell -= RemoveSpell;
    }

    //Mana Bar

    private void onManaChange(object sender, ManaChangeEventArgs e)
    {
        if (e.ManaChangeEventRecipient != subject)
        {
            return;
        }

        //Round the float value to the nearest int for the UI
        SetMana((int)Math.Round(e.ManaChangeEventValue, 0));
    }

    private void Calibrate(int max)
    {
        slider.maxValue = max;
        slider.value = max;
    }

    private void SetMana(float mana)
    {
        slider.value = mana;
    }

    // Spell Inventory
    // It feels wierd to say, but we only care about which slot a spell is in on the front-end.
    [SerializeField] private static int inventorySize = 5;
    [SerializeField] private GameObject[] inventorySlots = new GameObject[inventorySize];
    private SpellProfile[] inventoryContent = new SpellProfile[inventorySize];

    public int GetInventorySlot(GameObject obj)
    {
        for (int i = 0; i < inventorySize; i++)
        {
            if (inventorySlots[i] == obj)
            {
                return i;
            }
        }

        return -1;
    }

    public SpellProfile GetSpellInSlot(int slot)
    {
        return inventoryContent[slot];
    }

    private int GetFirstEmptySlot()
    {
        for (int i = 0; i < inventoryContent.Length; i++)
        {
            if (inventoryContent[i] == null)
            {
                return i;
            }
        }

        return -1;
    }

    private void AddSpell(object sender, SpellInventoryChangeArgs e)
    {
        SpellProfile newProfile = e.SpellInventoryChangeSpell;
        int slot = GetFirstEmptySlot();
        if (slot == -1)
        {
            Debug.Log("Error, no empty slots available!");
            return;
        }

        UpdateSlot(newProfile, slot);
    }

    private void RemoveSpell(object sender, SpellInventoryChangeArgs e)
    {
        SpellProfile profileToRemove = e.SpellInventoryChangeSpell;
        int slot = e.SpellInventoryChangeSlot;

        UpdateSlot(null, slot);
    }

    private void UpdateSlot(SpellProfile spell, int slot)
    {
        inventoryContent[slot] = spell;
        Image slotImage = inventorySlots[slot].GetComponent<Image>();

        if (spell == null)
        {
            slotImage.sprite = null;
        }
        else
        {
            slotImage.sprite = spell.sprite;
        }
        
    }

    private void ReadSpellRefList() //Read the spell list and set the UI's content to match
    {
        List<SpellProfile> spellRefList = subject.GetComponent<SpellInventory>().spellRefList;
        for (int i = 0; i < inventorySize-1; i++)
        {
            if (spellRefList.Count >= (i +1) )
            {
                UpdateSlot(spellRefList[i], i);
            }
            else
            {
                UpdateSlot(null, i);
            }
            
        }
    }
}
