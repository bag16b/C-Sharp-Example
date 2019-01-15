using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerHealth : MonoBehaviourPun {

    public int health;
    public int maxHealth;

    private void Start()
    {
        maxHealth = health;
    }

    public void reduceHealth()
    {
            photonView.RPC("ReduceHealthClients", RpcTarget.All);
    }

    [PunRPC]
    public void ReduceHealthClients()
    {
        if (health <= 1)
        {
            GameObject playerToLockTo = GameManager.instance.removePlayerFromPlayerList(gameObject);

            if (photonView.IsMine)
            {
                Camera.main.GetComponent<CameraFollow>().player = playerToLockTo;
                GameManager.instance.destroyPlayerHUD();
                StartCoroutine(WaitASecond());
            }

            return;
        }
        health -= 1;
        //Debug.Log(health);
    }

    IEnumerator WaitASecond()
    {
        yield return new WaitForSeconds(.5f);
        //called check for camera2 to avoid synomous name in PlayerCollisions.cs
        photonView.RPC("CheckForCamera2", RpcTarget.All);
        PhotonNetwork.Destroy(transform.parent.gameObject);
    }

    [PunRPC]
    void CheckForCamera2()
    {
        int ID = Camera.main.GetComponent<CameraFollow>().player.GetInstanceID();
        if (ID == gameObject.GetInstanceID())
        {
            //Debug.Log("About to die");
            int random = UnityEngine.Random.Range(0, (GameManager.instance.playerList.Count));
            Camera.main.GetComponent<CameraFollow>().player = GameManager.instance.playerList[random];
        }
        else
        {
            //Debug.Log("Not gonna die");
        }
    }
}
