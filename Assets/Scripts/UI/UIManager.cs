using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager current;
    void Awake()
    {
        current = this;

        UIRoot = GameObject.Find("UIContainer");
        MainMenu = GameObject.Find("UIContainer/UIObjectMainMenu").GetComponent<UIMainMenu>() as IUILayout;
        SpellMenu = GameObject.Find("UIContainer/UIObjectSpellMenu").GetComponent<UISpellMenu>() as IUILayout;
        RightHandMenu = GameObject.Find("PlayerUI_Right").GetComponent<UIRightHand>() as IUILayout;
        LeftHandMenu = GameObject.Find("PlayerUI_Left").GetComponent<UILeftHand>() as IUILayout;
        EventManager.current.onGameStateChange += onGameStateChange;
    }

    private GameObject UIRoot;
    private IUILayout MainMenu;
    private IUILayout SpellMenu;
    private IUILayout RightHandMenu;
    private IUILayout LeftHandMenu;

    private IUILayout[] UILayouts;
    private UILayout currentLayout = UILayout.UI_LAYOUT_NONE; //The layout we are either currently presesnted to the player, or lerping towards

    public int lerperIterative = 0;
    private List<UILerper> ActiveLerpers = new List<UILerper> { };

    //This class serves to manage which UI layouts are visible / hidden at any time.
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void onGameStateChange(object sender, GameStateEventArgs e)
    {
        switch (e.GameStateEventState)
        {
            case Game_State.GAME_STATE_IN_MENU:                
                ChangeUILayout(UILayout.UI_LAYOUT_MAIN);
                break;
            case Game_State.GAME_STATE_IN_COMBAT:
                ChangeUILayout(UILayout.UI_LAYOUT_IN_COMBAT);
                break;
            default:
                break;
        }
    }

    public void ChangeUILayout( UILayout newLayout)
    {
        if (newLayout == currentLayout)
        {
            return; // No change needed
        }        

        //Change the UI to the new layout
        // Once more layout are made, this needs cleaning up

        // Disable the current ui layout
        switch (currentLayout)
        {
            case UILayout.UI_LAYOUT_MAIN:
                MainMenu.DeactivateUILayout();
                break;
            case UILayout.UI_LAYOUT_SP:
                break;
            case UILayout.UI_LAYOUT_MP:
                break;
            case UILayout.UI_LAYOUT_CHARACTER:
                break;
            case UILayout.UI_LAYOUT_SPELLS:
                SpellMenu.DeactivateUILayout();
                RightHandMenu.DeactivateUILayout();
                break;
            case UILayout.UI_LAYOUT_OPTIONS:
                break;
            case UILayout.UI_LAYOUT_NONE:
                break;
            case UILayout.UI_LAYOUT_IN_COMBAT:
                RightHandMenu.DeactivateUILayout();
                LeftHandMenu.DeactivateUILayout();
                break;
            default:
                break;
        }

        // Enable the new Layout
        switch (newLayout)
        {
            case UILayout.UI_LAYOUT_MAIN:
                MainMenu.ActivateUILayout();
                break;
            case UILayout.UI_LAYOUT_SP:
                break;
            case UILayout.UI_LAYOUT_MP:
                break;
            case UILayout.UI_LAYOUT_CHARACTER:
                break;
            case UILayout.UI_LAYOUT_SPELLS:
                SpellMenu.ActivateUILayout();
                RightHandMenu.ActivateUILayout();
                break;
            case UILayout.UI_LAYOUT_OPTIONS:
                break;
            case UILayout.UI_LAYOUT_NONE:
                break;
            case UILayout.UI_LAYOUT_IN_COMBAT:
                RightHandMenu.ActivateUILayout();
                LeftHandMenu.ActivateUILayout();
                break;
            default:
                break;
        }

        currentLayout = newLayout;
        Debug.Log(newLayout);
    }

    public int GetLerperID(UILerper lerper)
    {
        //If there are any lerpers already operating on the object this lerper want to lerp, 
        // Cancel the first lerp
        GameObject obj = lerper.obj;
        for (int i = 0; i < ActiveLerpers.Count; i++)
        {
            if (ActiveLerpers[i].obj != null)
            {
                if (ActiveLerpers[i].obj == obj)
                {
                    ActiveLerpers[i].EndLerp();
                }
            }
        }

        int id = lerperIterative;
        lerperIterative++;

        ActiveLerpers.Add(lerper);
        return id;
    }

    public void onEndLerp(int lerpID)
    {
        for (int i = 0; i < ActiveLerpers.Count; i++)
        {
            if (ActiveLerpers[i].id == lerpID)
            {
                ActiveLerpers.RemoveAt(i);
                ActiveLerpers.TrimExcess();
            }
        }
    }
}

public interface IUILayout
{
    //Maybe a container for a layouts elements?

    void ActivateUILayout();
    void DeactivateUILayout();
}

public enum UILayout
{
    UI_LAYOUT_MAIN, // Main menu
    UI_LAYOUT_SP, // Single player menu
    UI_LAYOUT_MP, // Multiplayer menu
    UI_LAYOUT_CHARACTER, // Character screen
    UI_LAYOUT_SPELLS, // Spell select screen
    UI_LAYOUT_OPTIONS, // Options menu
    UI_LAYOUT_NONE, // UI disabled / all hidden
    UI_LAYOUT_IN_COMBAT // In combat - HP bars, Mana bars, Status bars
}

public class UILerper
{
    public GameObject obj { get; private set; }
    public int id { get; private set; }

    private Transform parent;

    private Vector3 start;
    private Vector3 end;

    private Quaternion startR;
    private Quaternion endR;

    private bool useRotation = false;

    private float duration;
    private float startTime;

    private bool inLerp = false;
    /*
    // Delegates
    public delegate void LerpCompleteDelegate();
    private dynamic OnLerpComplete;
    public void RegisterDelegate(LerpCompleteDelegate obj)
    {
        OnLerpComplete = obj;
    }
    */

    public UILerper(GameObject lerpee, Vector3 startPos, Vector3 endPos, float lerpDuration)
    {
        obj = lerpee;
        start = startPos;
        end = endPos;
        duration = lerpDuration;

        parent = obj.transform.parent;

        //Debug.Log(UIManager.current);
        id = UIManager.current.GetLerperID(this);

        startTime = Time.time;

        inLerp = true;
        EventManager.current.onUpdate += onUpdate;
    }

    public UILerper(GameObject lerpee, Vector3 startPos, Vector3 endPos, Quaternion startRot, Quaternion endRot, float lerpDuration)
    {
        obj = lerpee;
        start = startPos;
        end = endPos;
        duration = lerpDuration;
        startR = startRot;
        endR = endRot;

        parent = obj.transform.parent;

        id = UIManager.current.GetLerperID(this);

        useRotation = true;

        startTime = Time.time;

        inLerp = true;
        EventManager.current.onUpdate += onUpdate;
    }

    void onUpdate(object sender, EventArgs e)
    {
        if (!inLerp)
        {
            return;
        }

        float timelerping = Time.time - startTime;
        float lerpPct = timelerping / duration;

        obj.transform.position = Vector3.Lerp(start + parent.position, end + parent.position, lerpPct);
        if (useRotation)
        {
            obj.transform.rotation = Quaternion.Lerp(startR, endR, lerpPct);
        }

        if (lerpPct >= 1)
        {
            EndLerp();
        }
    }

    public void EndLerp()
    {
        inLerp = false;
        UIManager.current.onEndLerp(id);

        EventManager.current.onUpdate -= onUpdate;
    }


}
