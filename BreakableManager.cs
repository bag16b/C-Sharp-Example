using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Photon.Pun;
using Photon.Realtime;


//namespace Com.MyCompany.MyGame {
    public class BreakableManager : MonoBehaviourPunCallbacks{
        [HideInInspector] public bool inRange;
        private float maxHealth;
        private bool isInZone;

        //[SyncVar]
        public float health;

    
        void Start() {
            inRange = false;
            maxHealth = health;
            isInZone = false;
        }


        //check if local player or zone is in range
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == 11)
            {
                inRange = true;
            }else if (other.gameObject.layer == 15)
            {
                isInZone = true;
            }
        }
        //check if local player or zone is in range
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == 11)
            {
                inRange = false;
            }else if (other.gameObject.layer == 15)
            {
                isInZone = false;
            }
        }
        

        public void ReduceHealth()
        {
            if(health > -1)
                photonView.RPC("OnAwakeRPC",RpcTarget.All);
        
        }

        [PunRPC]
        public void OnAwakeRPC()
        {
            health -= 1;
            updateColor();
        }

        void Update()
        {

            if (isInZone)
            {
                health -= maxHealth/420f * Time.deltaTime * 50;
                updateColor();
            }
            //clicking is done in "OnClickPlayer"
            if (photonView.IsMine) //I got rid of this because I don't think it serves a real purpose.
            {
                if (health < 0)
                {

                    PhotonNetwork.Destroy(gameObject);
                }
            
            }

            
            
        }

        private void updateColor()
        {
            var col = health / maxHealth;
            GetComponent<Renderer>().material.color = new Color(col, col, col);
        }
        /*
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(health);
            }
            else
            {
                this.networkHealth = (int)stream.ReceiveNext();
            }
        }
        */
    }
//}