using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UILeftHand : MonoBehaviour, IUILayout
{
    [SerializeField] private Slider slider = null;//Assigned in Inspector
    [SerializeField] private GameObject subject = null;//Assigned in Inspector //To whom the bar refers to 

    private bool isActive = false;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < statusBarSize; i++)
        {
            statusSliders[i] = statusSlots[i].GetComponent<Slider>();
            statusSliders[i].value = 1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive)
        {
            return;
        }
        //Iterate through stored modifiers
        //Adjust duration bars
        for (int i = 0; i < statusBarSize; i++)
        {
            if (statusContents[i] != null)
            {
                float modDuration = statusContents[i].Duration;
                float modAge = Time.time - startTimes[i];
                float pct = modAge / modDuration;

                if (pct > 0)
                {
                    statusSliders[i].value = (float)Math.Round(pct, 1);
                }
                else
                {
                    statusSliders[i].value = 0;
                }

                
            }
        }
    }

    public void ActivateUILayout()
    {
        isActive = true;

        EventManager.current.onPlayerHealthChange += onHealthChange;
        EventManager.current.onModifierCreated += onModifierCreated;
        EventManager.current.onModifierRefreshed += onModifierRefreshed;
        EventManager.current.onModifierRemoved += onModifierRemoved;

        //Hide modifier icons - they have a default appearance that isnt accurate to actual modifier state
        for (int i = 0; i < statusBarSize; i++)
        {
            statusSliders[i].value = 0;

            Image slotImage = statusSlots[i].GetComponent<Image>();
            slotImage.sprite = null;
            slotImage.color = Color.clear;

            TextMeshProUGUI stackDisplay = statusSlots[i].transform.Find("StackCounter").gameObject.GetComponent<TextMeshProUGUI>();
            Color32 oldColor = stackDisplay.color;
            stackDisplay.color = new Color32(oldColor.r, oldColor.g, oldColor.b, 0);
        }
    }

    public void DeactivateUILayout()
    {
        isActive = false;

        EventManager.current.onPlayerHealthChange -= onHealthChange;
        EventManager.current.onModifierCreated -= onModifierCreated;
        EventManager.current.onModifierRefreshed -= onModifierRefreshed;
        EventManager.current.onModifierRemoved -= onModifierRemoved;
    }

    //Health Bar management.

    private void onHealthChange(object sender, HealthChangeEventArgs e)
    {
        if (e.HealthChangeEventRecipient != subject)
        {
            return;
        }

        //Round the float value to the nearest int for the UI
        SetHealth((int)Math.Round(e.HealthChangeEventValue, 0));
    }

    private void Calibrate(int max)
    {
        slider.maxValue = max;
        slider.value = max;
    }

    private void SetHealth(float health)
    {
        slider.value = health;
    }

    //Status Bar management
    [SerializeField] private static int statusBarSize = 5; 
    [SerializeField] private GameObject[] statusSlots = new GameObject[statusBarSize];
    private IModifier[] statusContents = new IModifier[statusBarSize];
    private float[] startTimes = new float[statusBarSize]; //Records when each modifier was added.
    private Slider[] statusSliders = new Slider[statusBarSize]; //These will get used per frame, so might as well store them here

    private void onModifierCreated(object sender, ModifierEventArgs e)
    {
        //Modifier was created
        if (e.ModifierEventTarget != subject)
        {
            //Modifier was created on non-subject, so we dont care
            return;
        }

        //Get the first empty slot
        int slot = GetFirstEmptySlot();

        if (slot == -1)
        {
            //panic
            Debug.Log("Error, status bar is full!");
            return;
        }

        //in the selected slot, update the display to show the modifier icon & duration etc
        //Access the profile of the spell that created this modifier
        SpellProfile sourceProfile = e.ModifierEventProfile;
        IModifier modifier = e.ModifierEventModifier;

        statusContents[slot] = modifier;
        startTimes[slot] = Time.time;

        //Update UI
        Image slotImage = statusSlots[slot].GetComponent<Image>();
        slotImage.sprite = sourceProfile.sprite;
        slotImage.color = Color.white;

        //Setup the StackDisplay
        TextMeshProUGUI stackDisplay = statusSlots[slot].transform.Find("StackCounter").gameObject.GetComponent<TextMeshProUGUI>();
        if (!stackDisplay)
        {
            Debug.Log("Error - could not find Stack display");
            return;
        }

        //If the modifier can stack, set the stackDisplay to the modifier's stack count. 
        // Else hide the stack counter
        if (modifier.canStack)
        {
            //Match the modifier's stacks
            stackDisplay.text = modifier.stackCount.ToString();

            //show text
            Color32 oldColor = stackDisplay.color;
            stackDisplay.color = new Color32(oldColor.r, oldColor.g, oldColor.b, 255);
        }
        else //Modifier doesn't stack, dont need a stack counter
        {
            //hide text
            Color32 oldColor = stackDisplay.color;
            stackDisplay.color = new Color32(oldColor.r, oldColor.g, oldColor.b, 0);
        }

        //Setup the duration display
        statusSliders[slot].value = 1;
    }    

    private void onModifierRefreshed(object sender, ModifierEventArgs e)
    {
        //Modifier was created
        if (e.ModifierEventTarget != subject)
        {
            //Modifier was created on non-subject, so we dont care
            return;
        }

        SpellProfile sourceProfile = e.ModifierEventProfile;
        IModifier modifier = e.ModifierEventModifier;

        //Find the modifier that was refreshed
        int slot = FindSlotWithModifier(modifier);
        if (slot == -1)
        {
            //panic
            Debug.Log("Error - could not find refreshed modifier in UI");
            return;
        }

        modifier = statusContents[slot];

        startTimes[slot] = Time.time;

        //If the modifier stacks, the stackCountUI needs updating
        if (modifier.canStack)
        {
            TextMeshProUGUI stackDisplay = statusSlots[slot].transform.Find("StackCounter").gameObject.GetComponent<TextMeshProUGUI>();
            stackDisplay.text = modifier.stackCount.ToString();
        }
    }

    private void onModifierRemoved(object sender, ModifierEventArgs e)
    {
        //Modifier was created
        if (e.ModifierEventTarget != subject)
        {
            //Modifier was created on non-subject, so we dont care
            return;
        }

        SpellProfile sourceProfile = e.ModifierEventProfile;
        IModifier modifier = e.ModifierEventModifier;

        int slot = FindSlotWithModifier(modifier);
        if (slot == -1)
        {
            //panic
            Debug.Log("Error - could not find removed modifier in UI");
            return;
        }

        statusContents[slot] = null;
        statusSliders[slot].value = 0;

        //Remove modifier from UI
        Image slotImage = statusSlots[slot].GetComponent<Image>();
        slotImage.sprite = null;
        slotImage.color = Color.clear;

        //Hide stackCounter
        TextMeshProUGUI stackDisplay = statusSlots[slot].transform.Find("StackCounter").gameObject.GetComponent<TextMeshProUGUI>();
        Color32 oldColor = stackDisplay.color;
        stackDisplay.color = new Color32(oldColor.r, oldColor.g, oldColor.b, 0);
    }
    /*
    private void UpdateSlot(int slot, IModifier modifier, SpellProfile sourceProfile)
    {
        //sourceProfile should be null unless the modifier is being added
        if (sourceProfile != null)
        {
            Image slotImage = statusSlots[slot].GetComponent<Image>();
            slotImage.sprite = sourceProfile.sprite;
            slotImage.color = Color.white;            
        }
        else
        {
            Image slotImage = statusSlots[slot].GetComponent<Image>();
            slotImage.sprite = null;
            slotImage.color = Color.clear;
        }

        //Setup the StackDisplay
        TextMeshProUGUI stackDisplay = statusSlots[slot].transform.Find("StackCounter").gameObject.GetComponent<TextMeshProUGUI>();
        if (!stackDisplay)
        {
            Debug.Log("Error - could not find Stack display");
        }

        if (modifier == null)
        {

        }

        //If the modifier can stack, set the stackDisplay to the modifier's stack count. 
        // Else hide the stack counter
        if (modifier.canStack)
        {
            //Match the modifier's stacks
            stackDisplay.text = modifier.stackCount.ToString();

            //show text
            Color32 oldColor = stackDisplay.color;
            stackDisplay.color = new Color32(oldColor.r, oldColor.g, oldColor.b, 255);
        }
        else //Modifier doesn't stack, dont need a stack counter
        {
            //hide text
            Color32 oldColor = stackDisplay.color;
            stackDisplay.color = new Color32(oldColor.r, oldColor.g, oldColor.b, 0);
        }
    }
    */

    private int GetFirstEmptySlot()
    {
        for (int i = 0; i < statusBarSize; i++)
        {
            if (statusContents[i] == null)
            {
                return i;
            }
        }

        return -1;
    }

    private int FindSlotWithModifier(IModifier modifier)
    {
        for (int i = 0; i < statusBarSize; i++)
        {
            if (statusContents[i] == modifier)
            {
                return i;
            }
        }

        return -1;
    }
}
