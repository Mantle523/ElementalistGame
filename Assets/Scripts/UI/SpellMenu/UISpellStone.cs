using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISpellStone : MonoBehaviour
{
    public SpellProfile spellProfile { get; private set; }

    [SerializeField] private TextMeshProUGUI nameDisplay = null;//Assigned in Inspector
    [SerializeField] private Image imageDisplay = null;//Assigned in Inspector

    public bool isHidden { get; private set; } = false;
    private bool isTextHidden = false;
    
    // Start is called before the first frame update
    void Start()
    {
        HideName();

        if (spellProfile)
        {
            UpdateDisplay(spellProfile);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateDisplay(SpellProfile newSpellProfile)
    {
        if (!newSpellProfile)
        {
            HideStone();
            return;
        }

        spellProfile = newSpellProfile;
        nameDisplay.text = newSpellProfile.name;
        imageDisplay.sprite = newSpellProfile.sprite;
    }

    public void OnSelection()
    {
        ShowName();
    }

    public void OnDeselection()
    {
        HideName();
    }

    public SpellProfile OnInspection()
    {
        HideName();
        return spellProfile;
    }

    private void HideName()
    {
        if (isTextHidden)
        {
            return;
        }

        Color32 oldColor = nameDisplay.color;
        nameDisplay.color = new Color32(oldColor.r, oldColor.g, oldColor.b, 0);
        isTextHidden = true;
    }

    private void ShowName()
    {
        if (!isTextHidden)
        {
            return;
        }

        Color32 oldColor = nameDisplay.color;
        nameDisplay.color = new Color32(oldColor.r, oldColor.g, oldColor.b, 255);
        isTextHidden = false;
    }

    private void HideStone()
    {
        MeshRenderer mr = GetComponent<MeshRenderer>();
        mr.enabled = false;
        imageDisplay.enabled = false;
        isHidden = true;
        HideName();
    }

    private void ShowStone()
    {
        MeshRenderer mr = GetComponent<MeshRenderer>();
        mr.enabled = true;
        imageDisplay.enabled = true;
        isHidden = false;
        ShowName();
    }

    //When a stone is grabbed by the player, we show the name of the spell.
}
