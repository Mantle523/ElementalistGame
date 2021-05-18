using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController current;

    public bool skipToCombat = false;
    public List<SpellProfile> defaultLoadout = new List<SpellProfile>(5);

    public List<GameObject> playerRoster = new List<GameObject>();

    public Game_State gameState { get; private set; }    

    private GameObject localplayer;
    private GameObject opponent;
    

    void Awake()
    {
        current = this;
    }
    
    void Start()
    {
        gameState = Game_State.GAME_STATE_IN_MENU;

        localplayer = GameObject.Find("Player");
        playerRoster.Add(localplayer);

        opponent = GameObject.Find("Target");
        playerRoster.Add(opponent);

        EventManager.current.GameStateChange(gameState);
        //Temp - Pretend we found a match with another player
        if (skipToCombat)
        {
            List<SpellProfile> refList = localplayer.GetComponent<SpellInventory>().spellRefList;
            foreach (SpellProfile spell in defaultLoadout)
            {
                refList.Add(spell);
            }

            List<SpellProfile> opprefList = opponent.GetComponent<SpellInventory>().spellRefList;
            foreach (SpellProfile spell in defaultLoadout)
            {
                opprefList.Add(spell);
            }
            StartVsMatch();
            //Add the defualt loadout to the player.
        }

    }

    public void StartVsMatch()
    {
        gameState = Game_State.GAME_STATE_IN_COMBAT;

        //opponent = GameObject.Find("Target");        
        //playerRoster.Add(opponent);

        EventManager.current.GameStateChange(gameState);
        SpellManager.current.BuildSpellLibrary(playerRoster);

        //Activate the AI
        opponent.GetComponent<AIAction>().ActivateAI();
    }
}

public enum Game_State
{
    GAME_STATE_IN_MENU,
    GAME_STATE_IN_COMBAT,
    GAME_STATE_POST_COMBAT
}
