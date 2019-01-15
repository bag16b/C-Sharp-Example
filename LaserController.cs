using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LaserController : MonoBehaviourPun {

    public float Speed;

    public GameObject createEffect;
    public GameObject destroyEffect;
    private GameObject justSpawned;

	// Use this for initialization
	void Start () {
        //photonView.RPC("Rpc_spawnCreate", RpcTarget.All);
        if(PhotonNetwork.IsMasterClient)
            PhotonNetwork.Instantiate(createEffect.name, transform.position, transform.rotation);
    }

    private void OnCollisionEnter(Collision other)
    {
        //Debug.Log("Hit solid thing");
        if (other.gameObject.layer == 13 || other.gameObject.layer == 9)
        {
            gameObject.GetComponent<CapsuleCollider>().enabled = false;
            //Debug.Log("Hit a block");
            if(PhotonNetwork.IsMasterClient)
                PhotonNetwork.Instantiate(destroyEffect.name, transform.position, new Quaternion(-transform.rotation.x, -transform.rotation.y, -transform.rotation.z, 1f));
            if (photonView.IsMine)
                PhotonNetwork.Destroy(gameObject);
            //photonView.RPC("Rpc_spawnDestroy", RpcTarget.All);
            
        }
        else if (other.gameObject.layer == 10)
        {
            gameObject.GetComponent<CapsuleCollider>().enabled = false;
            //Debug.Log("Hit a drone");
            //photonView.RPC("Rpc_spawnDestroy", RpcTarget.All);
            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.Instantiate(destroyEffect.name, transform.position, new Quaternion(-transform.rotation.x, -transform.rotation.y, -transform.rotation.z, 1f));
            if (photonView.IsMine)
                PhotonNetwork.Destroy(gameObject);
            other.gameObject.GetComponent<DroneHealth>().takeDamage(1f);
           
        }


    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 17)
        {
            gameObject.GetComponent<CapsuleCollider>().enabled = false;
            //Debug.Log("Hit a player");
            //photonView.RPC("Rpc_spawnDestroy", RpcTarget.All);
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Instantiate(destroyEffect.name, transform.position, new Quaternion(-transform.rotation.x, -transform.rotation.y, -transform.rotation.z, 1f));
                other.GetComponentInParent<PlayerHealth>().reduceHealth();
            }
            if (photonView.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
            
        }
        else
        {
            //Debug.Log(other.gameObject.layer);
        }
       

    }

// Update is called once per frame
    void Update () {

        transform.position += transform.forward * Speed * Time.deltaTime;
	}

    public void reflect(float a)
    {
        photonView.RPC("newVector", RpcTarget.All, a);
    }

    [PunRPC]
    public void newVector(float a)
    {
        transform.rotation = Quaternion.Euler(new Vector3(-a, 90, 0));
    }

    [PunRPC]
    public void Rpc_spawnCreate()
    {
        justSpawned = PhotonNetwork.Instantiate(createEffect.name, transform.position, transform.rotation);
    }

    [PunRPC]
    public void Rpc_spawnDestroy()
    {
        //justSpawned = PhotonNetwork.Instantiate(destroyEffect.name, transform.position, new Quaternion(-transform.rotation.x,-transform.rotation.y,-transform.rotation.z,1f));
        if (photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }

}
