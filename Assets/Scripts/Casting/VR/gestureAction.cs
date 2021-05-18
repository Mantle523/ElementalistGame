using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR;
using System;
using UnityEngine.Experimental.VFX;

// ty to "competent_tech" on stack overflow for providing a starting point here
// Equality Comparer to check to see if the contents of two arrays are the same
public class ArrayComparer : IEqualityComparer<string[]>
{
    public bool Equals(string[] item1, string[] item2)
    {
        if (item1.Length != item2.Length)
        {
            return false;
        }

        for (int i = 0; i < item1.Length; i++)
        {
            if (item1[i] != item2[i])
            {
                return false;
            }
        }

        return true;
    }


    public int GetHashCode(string[] item)
    {
        if (item.Length == 0)
        {
            return 0;
        }
        return item[0].GetHashCode();
    }
}

//https://medium.com/@sarthakghosh/a-complete-guide-to-the-steamvr-2-0-input-system-in-unity-380e3b1b3311
// Above used as template for VR gesture controls

public enum VR_INPUT_TYPE
{
    VR_INPUT_TYPE_GESTURE,
    VR_INPUT_TYPE_POINTER
}

public class gestureAction : castingAction
{

    // Register VR inputs for Left and Right Hands
    public SteamVR_Action_Boolean LeftGestureOnOff;
    public SteamVR_Action_Boolean RightGestureOnOff;
    public SteamVR_Action_Boolean ToggleNodeVisibility;

    public SteamVR_Input_Sources LeftHandType;
    public SteamVR_Input_Sources RightHandType;

    public GameObject leftHand;
    public GameObject rightHand;

    private VR_INPUT_TYPE input_type = VR_INPUT_TYPE.VR_INPUT_TYPE_POINTER;

    // UI interaction

    [SerializeField] private GameObject selectedUI = null;//Assigned in Inspector
    [SerializeField] private GameObject highlightVFX_Hand = null;//Assigned in Inspector
    private VisualEffect handVFX;    
    [SerializeField] private GameObject highlightVFX_Object = null;//Assigned in Inspector
    private VisualEffect objectVFX;
    private bool highlightVFX_Hand_on = false;

    // Basic Casting

    private bool leftGesture; //Gesture state for left hand
    private bool rightGesture; //Gesture state for right hand

    private string leftNode;
    private string rightNode;    

    private List<string> leftNodeOrder = new List<string>();   //Records the order in which nodes are touched during a gesture with the left hand.
    private List<string> rightNodeOrder = new List<string>();   //Records the order in which nodes are touched during a gesture with the right hand.

    // Channelling 

    private bool isLeftChannelling = false;
    private float channelStartTimeLeft;
    private IChannel channelledSpellLeft;

    private bool isRightChannelling = false;
    private float channelStartTimeRight;
    private IChannel channelledSpellRight;

    // Dual-casting
    private float dualCastWindow = 0.5f; //How long we wait for part 2 of a dualcast

    private bool isDualCasting = false; //Are we waiting for part 2 of a dual cast?
    private float dualCastDeadline; //The latest point in time we would wait before cancelling a dualcast
    private IDualCastable dualCastSpell; //The spell we are waiting to dualCast
    private GameObject dualCastFirstHand; //Simple bool to make sure we can't dualcast with the same hand

    // Dual-channelling - If one hand interrupts, both channels are ended
    private bool isDualChannelling = false;
    private float channelStartTimeDual;
    private IDualChannel channelledSpellDual;
    private GameObject[] channelHands;

    private Dictionary<string[], string> gestureDict = new Dictionary<string[], string>(new ArrayComparer());

    // Start is called before the first frame update
    void Start()
    {
        leftGesture = false;
        rightGesture = false;

        BuildSpellDictionary();
        GetComponents();

        LeftGestureOnOff.AddOnStateDownListener(LeftTriggerDown, LeftHandType);
        LeftGestureOnOff.AddOnStateUpListener(LeftTriggerUp, LeftHandType);

        RightGestureOnOff.AddOnStateDownListener(RightTriggerDown, RightHandType);
        RightGestureOnOff.AddOnStateUpListener(RightTriggerUp, RightHandType);

        ToggleNodeVisibility.AddOnStateDownListener(LeftGripDown, LeftHandType);

        //Registering for Events
        EventManager.current.onGameStateChange += onGameStateChange;        
        
        EventManager.current.onSpellInventoryCast += InitiateCast;

        //Gesture FX
        handVFX = highlightVFX_Hand.GetComponent<VisualEffect>();
        objectVFX = highlightVFX_Object.GetComponent<VisualEffect>();

    }   

    public void LeftTriggerUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        //Debug.Log("Left Trigger is up");
        if (input_type == VR_INPUT_TYPE.VR_INPUT_TYPE_GESTURE)
        {
            OnGestureEnd("left");
        }
        if (input_type == VR_INPUT_TYPE.VR_INPUT_TYPE_POINTER)
        {
            EventManager.current.UIDeselected(gameObject, selectedUI, leftHand); //Arguments are likely never relevant
        }
    }

    public void LeftTriggerDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        //Debug.Log("Left Trigger is down");
        if (input_type == VR_INPUT_TYPE.VR_INPUT_TYPE_GESTURE)
        {
            OnGestureStart("left");
        }

        if (input_type == VR_INPUT_TYPE.VR_INPUT_TYPE_POINTER && selectedUI != null)
        {
            EventManager.current.UISelected(gameObject, selectedUI, leftHand);
        }        
    }

    public void RightTriggerUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        //Debug.Log("Right Trigger is up");
        if (input_type == VR_INPUT_TYPE.VR_INPUT_TYPE_GESTURE)
        {
            OnGestureEnd("right");
        }
        
    }

    public void RightTriggerDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        //Debug.Log("Right Trigger is down");
        if (input_type == VR_INPUT_TYPE.VR_INPUT_TYPE_GESTURE)
        {
            OnGestureStart("right");
        }        
    }

    public void LeftGripDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        //Debug.Log("Grip Pressed");
        EventManager.current.ToggleNodeVisibility();
    }

    void BuildSpellDictionary()
    {
        //gestureDict.Add(new string[] { "", "" },);
        // Swipes - Long, sweeping arcs in a straight line
        // Vertical Swipes
        gestureDict.Add(new string[] { "node_RU", "node_RC", "node_RL" }, "swipe_down_R");
        gestureDict.Add(new string[] { "node_RL", "node_RC", "node_RU" }, "swipe_up_R");
        gestureDict.Add(new string[] { "node_Reach","node_RU", "node_RC", "node_RL" }, "swipe_down_R");
        gestureDict.Add(new string[] { "node_RL", "node_RC", "node_RU", "node_Reach" }, "swipe_up_R");
        gestureDict.Add(new string[] { "node_LU", "node_LC", "node_LL" }, "swipe_down_L");
        gestureDict.Add(new string[] { "node_LL", "node_LC", "node_LU" }, "swipe_up_L");
        gestureDict.Add(new string[] { "node_Reach", "node_LU", "node_LC", "node_LL" }, "swipe_down_L");
        gestureDict.Add(new string[] { "node_LL", "node_LC", "node_LU", "node_Reach" }, "swipe_up_L");
        gestureDict.Add(new string[] { "node_MU", "node_MC", "node_ML", }, "swipe_down_M");
        gestureDict.Add(new string[] { "node_ML", "node_MC", "node_MU", }, "swipe_up_M");
        gestureDict.Add(new string[] { "node_Reach","node_MU", "node_MC", "node_ML", }, "swipe_down_M");
        gestureDict.Add(new string[] { "node_ML", "node_MC", "node_MU","node_Reach" }, "swipe_up_M");
        // Horizontal Swipes
        gestureDict.Add(new string[] { "node_LU", "node_MU", "node_RU" }, "swipe_U");
        gestureDict.Add(new string[] { "node_RU", "node_MU", "node_LU" }, "swipe_U");
        gestureDict.Add(new string[] { "node_LC", "node_MC", "node_RC" }, "swipe_C");
        gestureDict.Add(new string[] { "node_RC", "node_MC", "node_LC" }, "swipe_C");
        gestureDict.Add(new string[] { "node_LL", "node_ML", "node_RL" }, "swipe_L");
        gestureDict.Add(new string[] { "node_RL", "node_ML", "node_LL" }, "swipe_L");
        // Diagonal Swipes
        gestureDict.Add(new string[] { "node_RL", "node_MC", "node_LU" }, "diagonal_up_RL");
        gestureDict.Add(new string[] { "node_RL", "node_MC", "node_LU","node_LeftReach" }, "diagonal_up_RL");
        gestureDict.Add(new string[] { "node_LL", "node_MC", "node_RU" }, "diagonal_up_LR");
        gestureDict.Add(new string[] { "node_LL", "node_MC", "node_RU", "node_RightReach" }, "diagonal_up_LR");
        gestureDict.Add(new string[] { "node_RU", "node_MC", "node_LL" }, "diagonal_down_RL");
        gestureDict.Add(new string[] { "node_RightReach", "node_RU", "node_MC", "node_LL" }, "diagonal_down_RL");
        gestureDict.Add(new string[] { "node_LU", "node_MC", "node_RL" }, "diagonal_down_LR");
        gestureDict.Add(new string[] { "node_LeftReach", "node_LU", "node_MC", "node_RL" }, "diagonal_down_LR");

        // Long Swipe - Horizontal Swipe with additional followthrough
        gestureDict.Add(new string[] { "node_LC", "node_MC", "node_RC", "node_RightRear" }, "long_swipe_C");
        gestureDict.Add(new string[] { "node_RC", "node_MC", "node_LC", "node_LeftRear" }, "long_swipe_C");
        // Full Swipe - 180 degree swipe
        gestureDict.Add(new string[] { "node_LeftRear","node_LC", "node_MC", "node_RC", "node_RightRear" }, "full_swipe_C");
        gestureDict.Add(new string[] { "node_RightRear","node_RC", "node_MC", "node_LC", "node_LeftRear" }, "full_swipe_C");
        // Power Swipe - Horizontal Swipe with a greater swing
        gestureDict.Add(new string[] { "node_LeftRear", "node_LC", "node_MC", "node_RC" }, "power_swipe_C");
        gestureDict.Add(new string[] { "node_RightRear", "node_RC", "node_MC", "node_LC" }, "power_swipe_C");
        gestureDict.Add(new string[] { "node_LeftRear", "node_LC", "node_MC" }, "power_swipe_C");
        gestureDict.Add(new string[] { "node_RightRear", "node_RC", "node_MC" }, "power_swipe_C");

        // Punches - Short, precise gestures. Should be quick to perform
        // Short Punch
        gestureDict.Add(new string[] { "node_RightPit", "node_MC", }, "short_punch_R");
        gestureDict.Add(new string[] { "node_LeftPit", "node_MC", }, "short_punch_L");
        // Long Punch
        gestureDict.Add(new string[] { "node_RightRear", "node_MC" },"long_punch_R");
        gestureDict.Add(new string[] { "node_LeftRear", "node_MC" },"long_punch_L");
        // Low Punch
        gestureDict.Add(new string[] { "node_RightRear", "node_ML" }, "low_punch_R");
        gestureDict.Add(new string[] { "node_LeftRear", "node_ML" }, "low_punch_L");
        gestureDict.Add(new string[] { "node_RightRear","node_RightPit", "node_ML" }, "low_punch_R");
        gestureDict.Add(new string[] { "node_LeftRear","node_LeftPit", "node_ML" }, "low_punch_L");

        // Full Body - Tall gestures. Should use node_Feet, node_Reach(L/R), or both. Expect many permutations for each gesture.
        // Sun Praise
        gestureDict.Add(new string[] { "node_Feet", "node_ML", "node_MC", "node_RU" },"sun_praise_R");
        gestureDict.Add(new string[] { "node_Feet", "node_ML", "node_RC", "node_RU" }, "sun_praise_R");
        gestureDict.Add(new string[] { "node_Feet", "node_RL", "node_RC", "node_RU" }, "sun_praise_R");
        gestureDict.Add(new string[] { "node_Feet", "node_ML", "node_RU" }, "sun_praise_R");
        gestureDict.Add(new string[] { "node_Feet", "node_RC", "node_RU" }, "sun_praise_R");
        gestureDict.Add(new string[] { "node_Feet", "node_RU" }, "sun_praise_R");
        gestureDict.Add(new string[] { "node_Feet", "node_ML", "node_MC", "node_LU" }, "sun_praise_L");
        gestureDict.Add(new string[] { "node_Feet", "node_ML", "node_LC", "node_LU" }, "sun_praise_L");
        gestureDict.Add(new string[] { "node_Feet", "node_LL", "node_LC", "node_LU" }, "sun_praise_L");
        gestureDict.Add(new string[] { "node_Feet", "node_ML", "node_LU" }, "sun_praise_L");
        gestureDict.Add(new string[] { "node_Feet", "node_LC", "node_LU" }, "sun_praise_L");
        gestureDict.Add(new string[] { "node_Feet", "node_LU" }, "sun_praise_L");        
        gestureDict.Add(new string[] { "node_Feet", "node_ML", "node_MC", "node_MU" }, "sun_praise_M");
        gestureDict.Add(new string[] { "node_Feet", "node_ML", "node_MC", "node_MU", "node_Reach" }, "sun_praise_M");        
    }

    // Update is called once per frame
    void Update()
    {
        if (input_type == VR_INPUT_TYPE.VR_INPUT_TYPE_GESTURE)
        {
            UpdateGesture();
        }
        else
        {
            UpdatePointer();
        }       
    }

    private void UpdateGesture()
    {
        ProcessChannel();
        if (Time.time > dualCastDeadline && isDualCasting == true)
        {
            ResetDualCast();
        }

        //Check both hands, if this is new add it.
        if (leftGesture == true && leftNode != null)
        {
            string lastEntry = leftNodeOrder.LastOrDefault();

            if (lastEntry != leftNode)
            {
                leftNodeOrder.Add(leftNode);
                //print(leftNodeOrder.Count);
            }
        }

        if (rightGesture == true && rightNode != null)
        {
            string lastEntry = rightNodeOrder.LastOrDefault();

            if (lastEntry != rightNode)
            {
                rightNodeOrder.Add(rightNode);
                //print(rightNodeOrder.Count);
            }
        }
    }

    private void UpdatePointer()
    {
        Vector3 pointerStartPos = leftHand.transform.position;
        Vector3 pointerDirection = leftHand.transform.TransformDirection(Vector3.forward + new Vector3(0, -1, 0));

        RaycastHit hit;
        bool rayHand = Physics.Raycast(pointerStartPos, pointerDirection, out hit, Mathf.Infinity, ~5, QueryTriggerInteraction.Collide); 
        if (rayHand)
        {
            //highlightVFX_Object.transform.position = hit.point;
            MovePointer(hit.point);
            if (selectedUI != hit.collider.gameObject)
            {
                UpdateUISelection(hit.collider.gameObject);
            }
        }
        else
        {
            if (selectedUI != null)
            {
                UpdateUISelection(null);
            }
        }        
    }

    private void MovePointer(Vector3 point)
    {
        Vector3 currentPos = highlightVFX_Object.transform.position;

        //How far away is the pointer from the particle? - This will impact the particle's speed
        float dist = Vector3.Distance(currentPos, point);
        if (dist > 0.2f) //If the pointer is too far away to move smoothly
        {
            highlightVFX_Object.transform.position = point;
        }
        else
        {
            highlightVFX_Object.transform.position = Vector3.MoveTowards(currentPos, point, 8 * dist * Time.deltaTime);
        }

    }

    private void UpdateUISelection(GameObject selection)
    {
        selectedUI = selection;        

        if (selection != null)
        {
            if (!highlightVFX_Hand_on)
            {
                handVFX.SendEvent("OnHighlightStart");
                objectVFX.SendEvent("OnHighlightStart");
                highlightVFX_Hand_on = true;
            }            
        }
        else
        {
            if (highlightVFX_Hand_on)
            {
                handVFX.SendEvent("OnHighlightEnd");
                objectVFX.SendEvent("OnHighlightEnd");
                highlightVFX_Hand_on = false;
            }            
        }
    }

    private void onGameStateChange(object sender, GameStateEventArgs e)
    {
        //Enable / Disable the node collision events when we are / arent using them.
        if (e.GameStateEventState == Game_State.GAME_STATE_IN_COMBAT)
        {
            input_type = VR_INPUT_TYPE.VR_INPUT_TYPE_GESTURE;
            UpdateUISelection(null);

            EventManager.current.onGestureNodeEnter += onNodeEnter;
            EventManager.current.onGestureNodeExit += onNodeExit;
        }
        else
        {
            EventManager.current.onGestureNodeEnter -= onNodeEnter;
            EventManager.current.onGestureNodeExit -= onNodeExit;
        }

        //Enable / Disable UI controls when we're interacting with the UI elements.
        if (e.GameStateEventState == Game_State.GAME_STATE_IN_MENU)
        {
            input_type = VR_INPUT_TYPE.VR_INPUT_TYPE_POINTER;
        }
    }

    private void onNodeEnter(object sender, GestureNodeTouchedEventArgs e)
    {
        switch (e.GestureNodeTouchedEventHand)
        {
            case "LeftHand":
                leftNode = e.GestureNodeTouchedEventNode;
                break;
            case "RightHand":
                rightNode = e.GestureNodeTouchedEventNode;
                break;
            default:
                Debug.Log("Event Error - Unrecognised Hand");
                break;
        }           
    }

    private void onNodeExit(object sender, GestureNodeTouchedEventArgs e)
    {
        switch (e.GestureNodeTouchedEventHand)
        {
            case "LeftHand":
                leftNode = null;
                break;
            case "RightHand":
                rightNode = null;
                break;
            default:
                Debug.Log("Event Error - Unrecognised Hand");
                break;
        }

        CheckChannelling(e);        
    }   

    void OnGestureStart(string hand)
    {
        if (hand == "left")
        {
            leftGesture = true;
        }
        if (hand == "right")
        {
            rightGesture = true;
        }

        //print( hand +" gesture started");
    }

    void OnGestureEnd(string hand)
    {
        string performedGesture;
        string[] nodeArray;

        switch (hand)
        {
            case "left":
                leftGesture = false;
                //Convert the List of activated nodes into an Array
                nodeArray = leftNodeOrder.ToArray();
                leftNodeOrder.Clear();

                //Turn the player's movement into a gesture
                performedGesture = ProcessGesture(nodeArray);
                print(performedGesture);

                if (performedGesture != "No matching gesture found")
                {
                    //Broadcast the gesture through event. (Does not garuntee a spell has been / will be cast)            
                    EventManager.current.GesturePerformed(gameObject, performedGesture, leftHand);
                }
                break;
            case "right":
                rightGesture = false;
                //Convert the List of activated nodes into an Array
                nodeArray = rightNodeOrder.ToArray();
                rightNodeOrder.Clear();

                //Turn the player's movement into a gesture
                performedGesture = ProcessGesture(nodeArray);
                print(performedGesture);

                if (performedGesture != "No matching gesture found")
                {
                    //Broadcast the gesture through event. (Does not garuntee a spell has been / will be cast)            
                    EventManager.current.GesturePerformed(gameObject, performedGesture, rightHand);
                }
                break;
            default:
                Debug.Log("Gesture Error - Unrecognised Hand - " + hand);
                break;
        }

        //print(hand + " gesture ended");

        /*
        //Convert the List of activated nodes into an Array
        string[] nodeArray = nodeOrder.ToArray();
        nodeOrder.Clear();

        //Turn the player's movement into a gesture
        string performedGesture = ProcessGesture(nodeArray);  
        print(performedGesture);

        if (performedGesture != "No matching gesture found")
        {
            //Broadcast the gesture through event. (Does not garuntee a spell has been / will be cast)            
            EventManager.current.GesturePerformed(gameObject, performedGesture);            
        }
        */
    }

    string ProcessGesture(string[] nodeArray)
    {
        /* [TEMP] Print the order in which the nodes were activated, then empty list
        foreach (string n in nodeArray)
            print(n);
        */

        // Big ol' Dictionary of all the different permutations of each gesture        
        string result;
        if (gestureDict.TryGetValue(nodeArray, out result))
        {
            return(result);
        }
        else
        {
            string errorReport = " ";
            foreach (string item in nodeArray)
            {
                errorReport = errorReport + ", " + item;
            }
            if (nodeArray.Length != 0)
            {
                Debug.Log(errorReport);
            }            
            //Debug.Log(nodeArray.Length);
            return ("No matching gesture found");
        }
    }

    private bool IsLeftHand(GameObject hand)
    {
        if (hand.name == "LeftHand")
        {
            return true;
        }
        if (hand.name == "RightHand")
        {
            return false;
        }
        Debug.Log("Error, unexpected object passed to IsLeftHand()");
        return false;
    }

    private bool CanCast(SpellType type, float cost)
    {
        //A series of checks of the caster's stacks.
        // If the caster is channelling, immidiately reject additional casts
        if (isChannelling())
        {
            return false;
        }
        // Mana check
        float casterCurrentMana = GetCurrentMana();
        if (casterCurrentMana < cost)
        {
            print("Cast rejected - Not enough Mana!");
            return false;
        }
        // General Silence check
        if (casterStats.castRestricted == true)
        {
            print("Cast rejected - Caster Silenced! (General)");
            return false;
        }
        // Specific silence check
        switch (type)
        {
            case SpellType.SPELL_TYPE_OFFENCE:
                if (casterStats.castRestrictedOffence == true)
                {
                    print("Cast rejected - Caster Silenced! (Offence)");
                    return false;
                }
                break;
            case SpellType.SPELL_TYPE_DEFENCE:
                if (casterStats.castRestrictedOffence == true)
                {
                    print("Cast rejected - Caster Silenced! (Defence)");
                    return false;
                }
                break;
            case SpellType.SPELL_TYPE_UTILITY:
                if (casterStats.castRestrictedOffence == true)
                {
                    print("Cast rejected - Caster Silenced! (Utility)");
                    return false;
                }
                break;
            default:
                print("Cast rejected - Unknown spell type presented!");
                return false;
        }

        // If all checks are passed
        return true;
    }    

    private void InitiateCast(object sender, SpellInventoryCastArgs e)
    {
        GameObject caster = e.SpellInventoryCastCaster;
        ICastable spell = e.SpellInventoryCastSpell;
        GameObject hand = e.SpellInventoryCastHand;
        string gesture = e.SpellInventoryCastGesture;

        GameObject target = localTarget; //local target might be obscurred, so prepare for that here

        if (CanCast(spell.spellType, spell.manacost))
        {
            switch (spell.spellCastType)
            {
                case SpellCastType.SPELL_CAST_SIMPLE: //Simple Gesture 'n' go spell
                    spell.Cast(caster, target, hand, gesture);
                    EventManager.current.SpellCast(caster, target, spell);
                    SpendResources(spell.manacost);
                    Debug.Log("Casted spell " + spell.GetType().Name);
                    break;

                case SpellCastType.SPELL_CAST_CHANNELLED: //Need to start a channel (on the casting hand)
                    //Begin channelling
                    StartChannelling(hand, spell);
                    spell.Cast(caster, target, hand, gesture);
                    EventManager.current.SpellCast(caster, target, spell);
                    SpendResources(spell.manacost);
                    Debug.Log("Channelling spell " + spell.GetType().Name);
                    break;

                case SpellCastType.SPELL_CAST_DUALCAST: //Wait for other hand to cast
                    if (isDualCasting)
                    {
                        if (CheckDualCast(hand, spell))
                        {
                            GameObject[] hands = new GameObject[] { leftHand, rightHand };
                            dualCastSpell.DualCast(caster, target, hands, gesture);
                            EventManager.current.SpellCast(caster, target, spell);
                            SpendResources(spell.manacost);
                            Debug.Log("Dual-Casted spell " + spell.GetType().Name);
                            ResetDualCast();
                        }
                    }
                    else
                    {
                        StartDualCast(hand, spell);
                    }
                    break;

                case SpellCastType.SPELL_CAST_DUALCHANNELLED: //Wait for other hand to channel + make sure they both continue to channel until the first channel finishes
                    if (isDualCasting)
                    {
                        if (CheckDualCast(hand, spell))
                        {
                            GameObject[] hands = new GameObject[] { leftHand, rightHand };
                            StartDualChannelling(hands, spell);
                            dualCastSpell.DualCast(caster, target, hands, gesture);
                            EventManager.current.SpellCast(caster, target, spell);
                            SpendResources(spell.manacost);
                            Debug.Log("Dual-Channelling spell " + spell.GetType().Name);
                            ResetDualCast();
                        }
                    }
                    else
                    {
                        StartDualCast(hand, spell);
                    }
                    break;

                default: break;
            }
        }
    }

    // CHANNELLING 

    private bool isChannelling() // Yes / No am i channelling with either hand?
    {
        if (isLeftChannelling == true || isRightChannelling == true)
        {
            return true;
        }

        if (isLeftChannelling == true && isRightChannelling == true)
        {
            return true;
        }
        return false;
    }

    private void StartChannelling(GameObject hand, ICastable spell)
    {
        if (IsLeftHand(hand))
        {
            isLeftChannelling = true;
            channelStartTimeLeft = Time.time;
            channelledSpellLeft = spell as IChannel;
            //channelHandLeft = hand;
        }
        else
        {
            isRightChannelling = true;
            channelStartTimeRight = Time.time;
            channelledSpellRight = spell as IChannel;
            //channelHandRight = hand;
        }
    }

    private void StartDualChannelling(GameObject[] hands, ICastable spell)
    {
        //Both hands simultaneously begin channelling
        isLeftChannelling = true;
        isRightChannelling = true;
        isDualChannelling = true;

        channelStartTimeDual = Time.time;
        channelledSpellDual = spell as IDualChannel;

        channelHands = hands;
    }

    private void CheckChannelling(GestureNodeTouchedEventArgs e)
    {
        if (isChannelling())
        {
            float channelTime;
            //If we're dualchannelling, the channel is cast by either hand interrupting
            if (isDualChannelling)
            {
                channelTime = Time.time - channelStartTimeDual;
                channelledSpellDual.onDualChannelInterrupt(gameObject, localTarget, channelHands, channelTime);
                EndDualChannel(channelTime);
                Debug.Log("Channel Interrupted");
                return;
            }

            if (e.GestureNodeTouchedEventHand == "LeftHand" && isLeftChannelling == true)
            {
                channelTime = Time.time - channelStartTimeLeft;
                channelledSpellLeft.onChannelInterrupt(gameObject, localTarget, leftHand, channelTime);
                EndChannel(channelTime, true);
                Debug.Log("Channel Interrupted");
            }
            else if (e.GestureNodeTouchedEventHand == "RightHand" && isRightChannelling == true)
            {
                channelTime = Time.time - channelStartTimeRight;
                channelledSpellRight.onChannelInterrupt(gameObject, localTarget, rightHand, channelTime);
                EndChannel(channelTime, false);
                Debug.Log("Channel Interrupted");
            }

        }
    }

    private void ProcessChannel()
    {
        if (isChannelling())
        {
            float channelTime;
            if (isDualChannelling)
            {
                channelTime = channelledSpellDual.channelDuration;
                if ((Time.time - channelStartTimeDual) > channelTime)
                {
                    channelledSpellDual.onDualChannelComplete(gameObject, localTarget, channelHands);
                    EndDualChannel(channelTime);
                    Debug.Log("Channel complete");
                }
                else
                {
                    channelledSpellDual.onDualChannelTick(gameObject, localTarget, channelHands, channelTime);
                }
                return;
            }

            if (isLeftChannelling)
            {
                channelTime = channelledSpellLeft.channelDuration;
                if ((Time.time - channelStartTimeLeft) > channelTime)
                {
                    channelledSpellLeft.onChannelComplete(gameObject, localTarget, leftHand);
                    EndChannel(channelTime, true);
                    Debug.Log("Channel complete");
                }
                else
                {
                    channelledSpellLeft.onChannelTick(gameObject, localTarget, leftHand, channelTime);
                }
            }
            else if (isRightChannelling)
            {
                channelTime = channelledSpellRight.channelDuration;
                if ((Time.time - channelStartTimeRight) > channelTime)
                {
                    channelledSpellRight.onChannelComplete(gameObject, localTarget, rightHand);
                    EndChannel(channelTime, false);
                    Debug.Log("Channel complete");
                }
                else
                {
                    channelledSpellRight.onChannelTick(gameObject, localTarget, rightHand, channelTime);
                }
            }

        }
    }

    private void EndChannel(float time, bool isLeft)
    {
        if (isLeft)
        {
            channelledSpellLeft.onChannelEnd(gameObject, localTarget, leftHand, time);
            isLeftChannelling = false;
            channelStartTimeLeft = 0;
            channelledSpellLeft = null;
            //channelHandLeft = null;
        }
        else
        {
            channelledSpellRight.onChannelEnd(gameObject, localTarget, rightHand, time);
            isRightChannelling = false;
            channelStartTimeRight = 0;
            channelledSpellRight = null;
            //channelHandRight = null;
        }

    }

    private void EndDualChannel(float time)
    {
        channelledSpellDual.onDualChannelEnd(gameObject, localTarget, channelHands, time);
        isLeftChannelling = false;
        isRightChannelling = false;
        isDualChannelling = false;

        channelStartTimeDual = 0f;
        channelledSpellDual = null;

        channelHands = null;
    }

    // DUALCASTING

    private void StartDualCast(GameObject hand, ICastable spell)
    {
        isDualCasting = true;
        dualCastDeadline = Time.time + dualCastWindow;
        dualCastSpell = spell as IDualCastable;
        dualCastFirstHand = hand;
    }

    private bool CheckDualCast(GameObject hand, ICastable spell)
    {
        if (hand == dualCastFirstHand)
        {
            return false;
        }
        if (spell as IDualCastable == dualCastSpell)
        {
            return true;
        }
        return false;
    }

    private void ResetDualCast()
    {
        isDualCasting = false;
        dualCastDeadline = 0;
        dualCastSpell = null;
        dualCastFirstHand = null;
    }
}
