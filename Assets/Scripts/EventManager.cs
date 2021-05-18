using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    //NOTE When making events for projectiles / modifiers, remember to remove listeners when the subscriber is destroyed

    public static EventManager current;

    private void Awake()
    {
        current = this;
    }

    void Update()
    {
        if (onUpdate != null)
        {
            onUpdate(this, EventArgs.Empty);
        }
    }

    // EVENTS
    // Monobehaviour-less classes need an update function so here's that
    public event EventHandler onUpdate;    

    // Game management events
    public event EventHandler onSpellLibraryComplete;
    public void SpellLibraryComplete()
    {
        if (onSpellLibraryComplete != null)
        {
            onSpellLibraryComplete(this, EventArgs.Empty);
        }
    }

    public event EventHandler<GameStateEventArgs> onGameStateChange;
    public void GameStateChange(Game_State gameState)
    {
        if (onGameStateChange != null)
        {
            onGameStateChange(this, new GameStateEventArgs(gameState));
        }
    }

    // UI events
    public event EventHandler<UISelectionEventArgs> onUISelected;
    public void UISelected(GameObject caster, GameObject ui, GameObject hand)
    {
        if (onUISelected != null)
        {
            onUISelected(this, new UISelectionEventArgs(caster, ui, hand));
        }
    }

    public event EventHandler<UISelectionEventArgs> onUIDeselected;
    public void UIDeselected(GameObject caster, GameObject ui, GameObject hand)
    {
        if (onUIDeselected != null)
        {
            onUIDeselected(this, new UISelectionEventArgs(caster, ui, hand));
        }
    }

    // Casting / Node events

    public event Action<GameObject, string, GameObject> onGesturePerformed;
    public void GesturePerformed(GameObject caster, string gesture, GameObject hand)
    {
        if (onGesturePerformed != null)
        {
            onGesturePerformed(caster, gesture, hand);
        }
    }

    public event EventHandler<GestureNodeTouchedEventArgs> onGestureNodeEnter;
    public void GestureNodeEnter(string hand, string node)
    {
        if (onGestureNodeEnter != null)
        {
            onGestureNodeEnter(this, new GestureNodeTouchedEventArgs(hand, node));
        }
    }
    public event EventHandler<GestureNodeTouchedEventArgs> onGestureNodeExit;
    public void GestureNodeExit(string hand, string node)
    {
        if (onGestureNodeExit != null)
        {
            onGestureNodeExit(this, new GestureNodeTouchedEventArgs(hand, node));
        }
    }

    public event EventHandler onToggleNodeVisibility;
    public void ToggleNodeVisibility()
    {
        if (onToggleNodeVisibility != null)
        {
            onToggleNodeVisibility(this, EventArgs.Empty);
        }
    }

    public event EventHandler<SpellCastEventArgs> onSpellCast;
    public void SpellCast(GameObject caster, GameObject target, ICastable spell)
    {        
        if (onSpellCast != null)
        {
            //print("spell cast");
            onSpellCast(this, new SpellCastEventArgs(caster, target, spell));
        }
    }
    
    public event EventHandler<SpellInventoryCastArgs> onSpellInventoryCast; //Player's SpellInventory has a spell to cast
    public void SpellInventoryCast(GameObject caster, ICastable spell, GameObject hand, string gesture)
    {
        if (onSpellInventoryCast != null)
        {
            onSpellInventoryCast(this, new SpellInventoryCastArgs(caster, spell, hand, gesture));
        }
    }

    //Spell Inventory events

    public event EventHandler<SpellInventoryChangeArgs> onAddSpell;
    public void AddSpell(GameObject caster, SpellProfile profile, int index)
    {
        if (onAddSpell != null)
        {
            onAddSpell(this, new SpellInventoryChangeArgs(caster, profile, index));
        }
    }

    public event EventHandler<SpellInventoryChangeArgs> onRemoveSpell;
    public void RemoveSpell(GameObject caster, SpellProfile profile, int index)
    {
        if (onRemoveSpell != null)
        {
            onRemoveSpell(this, new SpellInventoryChangeArgs(caster, profile, index));
        }
    }

    //Stat - related events

    public event EventHandler<HealthChangeEventArgs> onPlayerHealthChange;
    public event EventHandler<HealthChangeEventArgs> onBarrierHealthChange;
    public void HealthChange(GameObject recipient, GameObject sourcePlayer, float newHealth)
    {
        if (recipient.tag == "Barrier" && onBarrierHealthChange != null)
        {
            //print("barrier event");
            //Once we've confirmed that we're dealing with a barrier, the caster becomes more useful.
            GameObject caster = recipient.GetComponent<SimpleBarrier>().caster;
            onBarrierHealthChange(this, new HealthChangeEventArgs(caster, sourcePlayer, newHealth));
        }
        else
        {
            if (onPlayerHealthChange != null)
            {
                //print("player event");
                onPlayerHealthChange(this, new HealthChangeEventArgs(recipient, sourcePlayer, newHealth));
            }
        }
    }

    public event EventHandler<ManaChangeEventArgs> onManaChange;
    public void ManaChange(GameObject recipient, GameObject source, float newMana)
    {
        if (onManaChange != null)
        {
            onManaChange(this, new ManaChangeEventArgs(recipient, source, newMana));
        }
    }

    //Modifier Events
    public event EventHandler<ModifierEventArgs> onModifierCreated;
    public void ModifierCreated(GameObject Target, GameObject Source, IModifier mod, SpellProfile spellProfile)
    {
        if (onModifierCreated != null)
        {
            onModifierCreated(this, new ModifierEventArgs(Target, Source, mod, spellProfile));
        }
    }

    public event EventHandler<ModifierEventArgs> onModifierRefreshed;
    public void ModifierRefreshed(GameObject Target, GameObject Source, IModifier mod)
    {
        if (onModifierRefreshed != null)
        {
            onModifierRefreshed(this, new ModifierEventArgs(Target, Source, mod, null));
        }
    }

    public event EventHandler<ModifierEventArgs> onModifierRemoved;
    public void ModifierRemoved(GameObject Target, GameObject Source, IModifier mod)
    {
        if (onModifierRemoved != null)
        {
            onModifierRemoved(this, new ModifierEventArgs(Target, Source, mod, null));
        }
    }
}

// EVENT ARGUMENTS

public class GestureNodeTouchedEventArgs : EventArgs
{
    public string GestureNodeTouchedEventHand { get; private set; }
    public string GestureNodeTouchedEventNode { get; private set; }
    public GestureNodeTouchedEventArgs(string hand, string node)
    {
        GestureNodeTouchedEventHand = hand;
        GestureNodeTouchedEventNode = node;
    }
}

public class SpellCastEventArgs : EventArgs
{
    public GameObject SpellCastEventCaster { get; private set; }
    public GameObject SpellCastEventTarget { get; private set; }
    public ICastable SpellCastEventSpell { get; private set; }
    public SpellCastEventArgs(GameObject caster, GameObject target, ICastable spell)
    {
        SpellCastEventCaster = caster;
        SpellCastEventTarget = target;
        SpellCastEventSpell = spell;
    }
}

public class HealthChangeEventArgs : EventArgs
{
    public GameObject HealthChangeEventRecipient { get; private set; }
    public GameObject HealthChangeEventSource { get; private set; }
    public float HealthChangeEventValue { get; private set; }
    public HealthChangeEventArgs(GameObject recipient, GameObject source, float damage)
    {
        HealthChangeEventRecipient = recipient;
        HealthChangeEventSource = source;
        HealthChangeEventValue = damage;
    }
}

public class ManaChangeEventArgs : EventArgs
{
    public GameObject ManaChangeEventRecipient { get; private set; }
    public GameObject ManaChangeEventSource { get; private set; }
    public float ManaChangeEventValue { get; private set; }
    public ManaChangeEventArgs(GameObject recipient, GameObject source, float value)
    {
        ManaChangeEventRecipient = recipient;
        ManaChangeEventSource = source;
        ManaChangeEventValue = value;
    }
}

public class SpellInventoryCastArgs : EventArgs
{
    public GameObject SpellInventoryCastCaster { get; private set; }
    public ICastable SpellInventoryCastSpell { get; private set; }
    public GameObject SpellInventoryCastHand { get; private set; }
    public string SpellInventoryCastGesture { get; private set; }

    public SpellInventoryCastArgs(GameObject caster, ICastable spell, GameObject hand, string gesture)
    {
        SpellInventoryCastCaster = caster;
        SpellInventoryCastSpell = spell;
        SpellInventoryCastHand = hand;
        SpellInventoryCastGesture = gesture;
    }
}

public class GameStateEventArgs : EventArgs
{
    public Game_State GameStateEventState { get; private set; }

    public GameStateEventArgs(Game_State state)
    {
        GameStateEventState = state;
    }
}

public class UISelectionEventArgs : EventArgs
{
    public GameObject UISelectionEventCaster { get; private set; }
    public GameObject UISelectionEventObject { get; private set; }
    public GameObject UISelectionEventHand { get; private set; }

    public UISelectionEventArgs(GameObject caster, GameObject ui, GameObject hand)
    {
        UISelectionEventCaster = caster;
        UISelectionEventObject = ui;
        UISelectionEventHand = hand;
    }
}

public class SpellInventoryChangeArgs : EventArgs
{
    public GameObject SpellInventoryChangeCaster { get; private set; }
    public SpellProfile SpellInventoryChangeSpell { get; private set; }
    public int SpellInventoryChangeSlot { get; private set; }

    public SpellInventoryChangeArgs(GameObject caster, SpellProfile spell, int index)
    {
        SpellInventoryChangeCaster = caster;
        SpellInventoryChangeSpell = spell;
        SpellInventoryChangeSlot = index;
    }
}

public class ModifierEventArgs : EventArgs
{
    public GameObject ModifierEventTarget { get; private set; }
    public GameObject ModifierEventSource { get; private set; }
    public IModifier ModifierEventModifier { get; private set; }
    public SpellProfile ModifierEventProfile { get; private set; }

    public ModifierEventArgs(GameObject target, GameObject source, IModifier mod, SpellProfile spellProfile)
    {
        ModifierEventTarget = target;
        ModifierEventSource = source;
        ModifierEventModifier = mod;
        ModifierEventProfile = spellProfile;
    }
}
