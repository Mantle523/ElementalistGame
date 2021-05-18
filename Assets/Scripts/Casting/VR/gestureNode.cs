using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gestureNode : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        EventManager.current.onToggleNodeVisibility += ToggleNode;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    bool mrenabled = true;

    private void OnTriggerEnter(Collider other)
    {
        //print("collision between " + other.gameObject.name + " and " + gameObject.name);        
        if (other.gameObject.name == "LeftHand" || other.gameObject.name == "RightHand")
        {            
            EventManager.current.GestureNodeEnter(other.gameObject.name, gameObject.name);            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //print("collision between " + other.gameObject.name + " and " + gameObject.name);        
        if (other.gameObject.name == "LeftHand" || other.gameObject.name == "RightHand")
        {
            EventManager.current.GestureNodeExit(other.gameObject.name, gameObject.name);
        }
    }

    private void ToggleNode(object sender, EventArgs e)
    {
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mrenabled == true)
        {
            mr.enabled = false;
            mrenabled = false;
        }
        else
        {
            mr.enabled = true;
            mrenabled = true;
        }

    }
}
