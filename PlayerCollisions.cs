using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class PlayerCollisions : MonoBehaviourPun {

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Reset")
        {
           
            GameObject playerToLockTo = GameManager.instance.removePlayerFromPlayerList(gameObject);
            
            if (photonView.IsMine)
            {
                
                Camera.main.GetComponent<CameraFollow>().player = playerToLockTo;
                GameManager.instance.destroyPlayerHUD();
                StartCoroutine(WaitASecond());
                //PhotonNetwork.Destroy(transform.parent.gameObject);
            }
            
            
            //Debug.Log("Finished");
        }
    }

    IEnumerator WaitASecond()
    {
        yield return new WaitForSeconds(.5f);
        DestroyPlayerMaster();
        
    }

    [PunRPC]
    void CheckForCamera()
    {
        int ID = Camera.main.GetComponent<CameraFollow>().player.GetInstanceID();
        if (ID == gameObject.GetInstanceID())
        {
            Debug.Log("About to die");
            int random = UnityEngine.Random.Range(0, (GameManager.instance.playerList.Count));
            Camera.main.GetComponent<CameraFollow>().player = GameManager.instance.playerList[random];
        }
        else
        {
            Debug.Log("Not gonna die");
        }
    }

    private void DestroyPlayerMaster()
    {
        photonView.RPC("CheckForCamera", RpcTarget.All);
        PhotonNetwork.Destroy(gameObject.transform.parent.gameObject);
    }
}
