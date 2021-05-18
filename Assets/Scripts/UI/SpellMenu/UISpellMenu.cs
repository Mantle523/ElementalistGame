using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISpellMenu : MonoBehaviour, IUILayout
{
    //States
    private bool isActive = false;

    // Transitions
    private Vector3 InactivePos = new Vector3(0, -2.5f, 0);
    private Vector3 ActivePos = new Vector3(0, 0, 0);

    public float transitionTime = 2;

    // Pages - For now only 1
    private int activePage;
    [SerializeField] private SpellProfile[] ProfilesPage1 = null;

    //private SpellProfile[][] ProfilePages = new SpellProfile[][] { ProfilesPage1 };
    [SerializeField] private GameObject ReturnButton = null;

    // GridPositions
    [SerializeField] private GameObject[] GridPositions = null;
    private UISpellStone[] SpellStones = new UISpellStone[9];

    // Selection
    int selectionIndex;
    bool stoneSelected = false; //Stone in hand
    bool stoneInspected = false; //Stone in inspection altar

    Vector3 selectionPos = new Vector3(-0.5f, 1.5f, -0.5f);    
    Vector3 returnPos;

    // Inspection
    Vector3 inspectionPos = new Vector3(0, 0.7f, -0.7f);
    private int inspectedIndex = -1;

    [SerializeField] private TextMeshProUGUI inspectNameDisplay = null; //Assigned in Inspector
    [SerializeField] private TextMeshProUGUI inspectDescDisplay = null;//Assigned in Inspector
    [SerializeField] private Image inspectImageDisplay = null;//Assigned in Inspector
    [SerializeField] private Image inspectImageBackGround = null;//Assigned in Inspector

    // SpellInventory
    [SerializeField] private GameObject inventoryUIObject = null;//Assigned in Inspector
    private UIRightHand inventoryUI;

    // Start is called before the first frame update
    void Start()
    {
        inventoryUI = inventoryUIObject.GetComponent<UIRightHand>();

        for (int i = 0; i < GridPositions.Length; i++)
        {
            SpellStones[i] = GridPositions[i].GetComponent<UISpellStone>();
        }

        HideInspectPanel();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ActivateUILayout()
    {
        if (isActive)
        {
            return;
        }

        isActive = true;

        //Start Listening for UI events
        EventManager.current.onUISelected += onUISelected;
        EventManager.current.onUIDeselected += onUIDeselected;

        //Move the layout in front of the player
        new UILerper(gameObject, InactivePos, ActivePos, transitionTime);

        LoadPage(ProfilesPage1);
    }

    public void DeactivateUILayout()
    {
        if (!isActive)
        {
            return;
        }

        isActive = false;

        //Stop Listening for UI events
        EventManager.current.onUISelected -= onUISelected;
        EventManager.current.onUIDeselected -= onUIDeselected;

        new UILerper(gameObject, ActivePos, InactivePos, transitionTime);
    }

    private void LoadPage( SpellProfile[] Page )
    {
        for (int i = 0; i < GridPositions.Length; i++)
        {
            SpellStones[i].UpdateDisplay(Page[i]);
        }
    }

    private void onUISelected(object sender, UISelectionEventArgs e)
    {
        int indexContainer = -1;

        if (!isActive)
        {
            return;
        }
        
        //Just a quick check to make sure the selected UI is part of the spellUI
        GameObject UIobject = e.UISelectionEventObject;
        if (!UIobject)
        {
            return;
        }

        if (UIobject == ReturnButton)
        {
            UIManager.current.ChangeUILayout(UILayout.UI_LAYOUT_MAIN);
            return;
        }

        //We're selecting a spell in the spell inventory ui
        int slotIndex = inventoryUI.GetInventorySlot(UIobject);
        if (slotIndex != -1) //The highlighted object was a spellInventoryUISlot
        {
            SpellProfile spellToRemove = inventoryUI.GetSpellInSlot(slotIndex);

            EventManager.current.RemoveSpell(e.UISelectionEventCaster, spellToRemove, slotIndex);
            return;
        }

        bool containedInUI = false;

        for (int i = 0; i < GridPositions.Length; i++)
        {
            if (GridPositions[i] == UIobject)
            {
                containedInUI = true;
                indexContainer = i;
            }
        }

        if (!containedInUI)
        {
            Debug.Log("External UI element selected from SpellMenu");
            return;
        }

        // If something is already selected, we first need to move that stone back into position
        // If an object is being inspected, we either return the inspect object and continue as normal
        // or we grab the inspected object from the altar
        if (stoneSelected && !stoneInspected)
        {
            DeselectStone(selectionIndex);
        }

        //if we're inspecting something , return the inspected stone to its og position
        if (stoneInspected)
        {
            UninspectStone(selectionIndex);
        }

        SelectStone(indexContainer);

        selectionIndex = indexContainer;
    }

    private void onUIDeselected(object sender, UISelectionEventArgs e)
    {
        //Grip on object has been released.
        //If we're aiming at the alter, inspect. else return to original position
        //If we're aiming at an inventory slot, add to spellInventoryRefList

        if (!isActive)
        {
            return;
        }

        if (!stoneSelected)
        {
            return;
        }

        if (e.UISelectionEventObject != null)
        {
            if (e.UISelectionEventObject.name == "InspectAltar" || e.UISelectionEventObject.name == "Plinth")
            {
                InspectStone(selectionIndex);
                return;
            }
            int slotIndex = inventoryUI.GetInventorySlot(e.UISelectionEventObject);
            if (slotIndex != -1) //The highlighted object was a spellInventoryUISlot - we don't care which one, but the front-end will
            {
                SpellProfile spellToAdd = SpellStones[selectionIndex].spellProfile;

                EventManager.current.AddSpell(e.UISelectionEventCaster, spellToAdd, slotIndex);
            }
        }
        DeselectStone(selectionIndex);
    }

    private void SelectStone(int index)
    {
        //We may have selected a hidden stone (not active for current page)
        //Let's check for that here
        UISpellStone script = SpellStones[index];
        GameObject stone = GridPositions[index];

        Debug.Log(stone.name);

        if (script.isHidden)
        {
            return;
        }

        //Save the stone's current location for lerpers and for returning it upon deselection
        if (inspectedIndex != index || inspectedIndex == -1)
        {
            returnPos = stone.transform.localPosition;            
        }

        stoneSelected = true;
        //Temp, object will be parented to hand
        new UILerper(stone, stone.transform.localPosition, selectionPos, 0.2f);

        script.OnSelection();

    }

    private void DeselectStone(int index)
    {
        if (!stoneSelected)
        {
            return;
        }

        UISpellStone script = SpellStones[index];
        GameObject stone = GridPositions[index];

        new UILerper(stone, selectionPos, returnPos, 0.2f);

        script.OnDeselection();

        stoneSelected = false;
    }

    private void InspectStone(int index)
    {
        if (stoneInspected)
        {
            return;
        }

        stoneSelected = false;

        UISpellStone script = SpellStones[index];
        GameObject stone = GridPositions[index];

        inspectedIndex = index;

        new UILerper(stone, selectionPos, inspectionPos, 0.2f);

        SpellProfile inspectProfile = script.OnInspection();
        stoneInspected = true;

        UpdateInspectPanel(inspectProfile);
        ShowInspectPanel();
    }

    private void UninspectStone(int index)
    {
        new UILerper(GridPositions[index], inspectionPos, returnPos, 0.2f);
        HideInspectPanel();
        stoneInspected = false;
    }

    private void HideInspectPanel()
    {
        //Hide the contents of the inspection panel
        Color32 oldColor = inspectNameDisplay.color;
        inspectNameDisplay.color = new Color32(oldColor.r, oldColor.g, oldColor.b, 0);
        inspectDescDisplay.color = new Color32(oldColor.r, oldColor.g, oldColor.b, 0);
        inspectImageBackGround.color = new Color32(120, 120, 120, 0);

        inspectImageDisplay.enabled = false;
    }

    private void ShowInspectPanel()
    {
        //Show the contents of the inspection panel
        Color32 oldColor = inspectNameDisplay.color;
        inspectNameDisplay.color = new Color32(oldColor.r, oldColor.g, oldColor.b, 255);
        inspectDescDisplay.color = new Color32(oldColor.r, oldColor.g, oldColor.b, 255);
        inspectImageBackGround.color = new Color32(120, 120, 120, 60);

        inspectImageDisplay.enabled = true;
    }

    private void UpdateInspectPanel( SpellProfile profile )
    {
        if (!profile)
        {
            Debug.Log("Missing profile");
            return;
        }

        inspectNameDisplay.text = profile.name;
        inspectDescDisplay.text = profile.description;
        inspectImageDisplay.sprite = profile.sprite;
    }
}
