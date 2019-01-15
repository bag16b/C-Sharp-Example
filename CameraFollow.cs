using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

//This Script is for setting the PlayerCamera to Follow the Player
public class CameraFollow : MonoBehaviourPun {

    public GameObject player;
    [SerializeField] Vector3 offset;

	// Use this for initialization
	void Start () {
        
        offset = new Vector3(0, 0, -6);
    }

    // LateUpdate is called after Update each frame
    void Update () {
        if (player)
        {
            transform.position = player.transform.position + offset;
        }
        
    }
}
