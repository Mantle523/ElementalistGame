using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeBuilder : MonoBehaviour
{
    //A quick script to adjust the positions of the nodes around the player.
    // At the moment we're only checking the player's height, we'll likely need to adjust for x and z varience too

    private GameObject gestureNode;
    public float initialHeight = 1.62f; // My height in Unity
    
    // Start is called before the first frame update
    void Start()
    {
        CalibrateNodes(initialHeight);        
    }

    private void CalibrateNodes(float playerHeight)
    {
        gestureNode = Resources.Load("Prefabs/node_prefab") as GameObject;

        // Build an array of nodes, which we can scale to the player's real-world height
        // y = 1.62

        Transform nodeContainer = gameObject.transform.Find("GestureNodes");
        foreach (Transform child in nodeContainer.transform)
        {
            float normalHeight = child.transform.position.y / 1.62f;
            float newHeight = normalHeight * playerHeight;
            //print(normalHeight);
            child.transform.position = new Vector3(child.transform.position.x, newHeight, child.transform.position.z);
        }        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
