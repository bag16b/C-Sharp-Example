using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DroneHealth : MonoBehaviourPun {
    public float health;
    public float maxHealth;
    [HideInInspector]
    public GameObject playerInstance;
	// Use this for initialization
	void Start () {
        maxHealth = 2;
        health = maxHealth;
	}
	
	// Update is called once per frame
	void Update () {
        if (photonView.IsMine)
        {
            if (health <= 0)
            {
                playerInstance.GetComponent<CameraManager>().resetDrone();
                //make big boom boi
            }
        }
	}

    public void resetHealth()
    {
        health = maxHealth;
    }

    public void takeDamage(float damage)
    {
        photonView.RPC("ReduceHealth", RpcTarget.All, damage);
    }

    [PunRPC]
    void ReduceHealth(float damage)
    {
        health -= damage;
    }
}
