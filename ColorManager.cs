using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ColorManager : MonoBehaviourPun {
    public GameObject Miner;
    public GameObject Drone;
    public GameObject DroneMesh;

    //[SyncVar]
    private Color myCol;

    // Use this for initialization
    void Start () {
        //PhotonView.
        if (photonView.IsMine)
        {
            photonView.RPC("setColor", RpcTarget.AllBuffered, Random.Range(0f,1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        }
    }

    [PunRPC]
    void setColor(float r, float g, float b)
    {
        
            Miner.GetComponent<Renderer>().material.color = new Color(r,g,b);
            //DroneMesh.GetComponent<Renderer>().material.color = new Color(r,g,b);
   
    }

    //public override void OnStartClient()
    //{
        //Set the main Color of the Material to somethin'

        //if (isServer)
        //{
            //myCol = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        //}
    //}
}
