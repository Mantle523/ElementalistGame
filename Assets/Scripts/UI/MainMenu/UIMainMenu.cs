using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMainMenu : MonoBehaviour, IUILayout
{
    // States
    private bool isActive = false;
    private bool isPlayBoxSplit = false;
    private bool isCustomiseBoxSplit = false;

    // Transitions
    private Vector3 InactivePos = new Vector3(0, -2.5f, 0);
    private Vector3 ActivePos = new Vector3(0, 0, 0);

    public float transitionTime;

    private UIMainMenuElement Title;
    private UIMainMenuElement SinglePlayer;
    private UIMainMenuElement MultiPlayer;
    private UIMainMenuElement Character;
    private UIMainMenuElement Spells;

    private UIMainMenuElement[] UIElements;

    // Start is called before the first frame update
    void Start()
    {
        Title = new UIMainMenuElement(gameObject.transform.Find("Title_Object").gameObject, new Vector3(0,2,0), new Vector3(0,2,0))  ;
        SinglePlayer = new UIMainMenuElement(gameObject.transform.Find("SinglePlayer_Object").gameObject, new Vector3(-0.3f, 1.4f, 0), new Vector3(-0.5f, 1.4f, 0) );
        MultiPlayer = new UIMainMenuElement(gameObject.transform.Find("MultiPlayer_Object").gameObject, new Vector3(0.3f, 1.4f, 0), new Vector3(0.5f, 1.4f, 0) );
        Character = new UIMainMenuElement(gameObject.transform.Find("CChar_Object").gameObject, new Vector3(-0.3f, 0.9f, 0), new Vector3(-0.5f, 0.9f, 0) );
        Spells = new UIMainMenuElement(gameObject.transform.Find("CSpell_Object").gameObject, new Vector3(0.3f, 0.9f, 0), new Vector3(0.5f, 0.9f, 0) );
        UIElements = new UIMainMenuElement[] { Title, SinglePlayer, MultiPlayer, Character, Spells };
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

        Debug.Log("Activating");

        isActive = true;

        //Start Listening for UI events
        EventManager.current.onUISelected += onUISelected;

        //Move the layout in front of the player
        new UILerper(gameObject, InactivePos, ActivePos, transitionTime);  
    }

    public void DeactivateUILayout()
    {
        if (!isActive)
        {
            return;
        }

        isActive = false;
        isPlayBoxSplit = false;
        isCustomiseBoxSplit = false;

        //Stop Listening for UI events
        EventManager.current.onUISelected -= onUISelected;

        new UILerper(gameObject, ActivePos, InactivePos, transitionTime);
        new UILerper(SinglePlayer.element, SinglePlayer.endPos, SinglePlayer.startPos, SinglePlayer.element.transform.rotation, Quaternion.Euler(0, 0, 0), transitionTime);
        new UILerper(MultiPlayer.element, MultiPlayer.endPos, MultiPlayer.startPos, MultiPlayer.element.transform.rotation, Quaternion.Euler(0, 0, 0), transitionTime);
        new UILerper(Character.element, Character.endPos, Character.startPos, Character.element.transform.rotation, Quaternion.Euler(0, 0, 0), transitionTime);
        new UILerper(Spells.element, Spells.endPos, Spells.startPos, Spells.element.transform.rotation, Quaternion.Euler(0, 0, 0), transitionTime);
    }

    private void SplitPlayBox()
    {
        isPlayBoxSplit = true;

        new UILerper(SinglePlayer.element, SinglePlayer.startPos, SinglePlayer.endPos, SinglePlayer.element.transform.rotation, Quaternion.Euler(0, 90, 0), transitionTime);
        new UILerper(MultiPlayer.element, MultiPlayer.startPos, MultiPlayer.endPos, MultiPlayer.element.transform.rotation, Quaternion.Euler(0, -90, 0), transitionTime);
    }

    private void SplitCustomiseBox()
    {
        isCustomiseBoxSplit = true;

        new UILerper(Character.element, Character.startPos, Character.endPos, Character.element.transform.rotation, Quaternion.Euler(0, 90, 0), transitionTime);
        new UILerper(Spells.element, Spells.startPos, Spells.endPos, Spells.element.transform.rotation, Quaternion.Euler(0, -90, 0), transitionTime);
    }

    private void onUISelected(object sender, UISelectionEventArgs e)
    {
        //Shouldn't be needed, but just in case
        if (!isActive)
        {
            return;
        }

        GameObject UIobject = e.UISelectionEventObject;        
        
        for (int i = 0; i < UIElements.Length; i++)
        {
            if (UIElements[i].element == UIobject)
            {
                //We've got the element selected now.
                // If its the Title element, we don't care
                // If its the singleplayer or multiplyer buttons, we need to either seperate them or launch the appropriate mode
                // Same for the Customise buttons
                switch (i)
                {
                    case 0:
                        break;
                    case 1:
                        if (!isPlayBoxSplit)
                        {
                            SplitPlayBox();
                        }
                        else
                        {
                            //Launch SinglePlayer screen

                            //Temp launch combat mode
                            GameController.current.StartVsMatch();
                        }
                        break;
                    case 2:
                        if (!isPlayBoxSplit)
                        {
                            SplitPlayBox();
                        }
                        else
                        {
                            //Launch Multiplayer screen
                            Debug.Log("MultiPlayer");
                        }
                        break;
                    case 3:
                        if (!isCustomiseBoxSplit)
                        {
                            SplitCustomiseBox();
                        }
                        else
                        {
                            //Launch Customise character screen
                            Debug.Log("Character");
                        }
                        break;
                    case 4:
                        if (!isCustomiseBoxSplit)
                        {
                            SplitCustomiseBox();
                        }
                        else
                        {
                            //Launch Customise spells screen
                            Debug.Log("Spells");
                            UIManager.current.ChangeUILayout(UILayout.UI_LAYOUT_SPELLS);
                        }
                        break;
                    default:
                        break;
                }


            }
        }
    }

    public class UIMainMenuElement
    {
        public GameObject element { get; private set; } //The element this corresponds to
        public Vector3 startPos { get; private set; } //The position this element takes and returns to, reletive to parent
        public Vector3 endPos { get; private set; } //The position the element moves to, relative to parent

        public UIMainMenuElement(GameObject Element, Vector3 StartPos, Vector3 EndPos)
        {
            element = Element;
            startPos = StartPos;
            endPos = EndPos;
        }
    }
}


