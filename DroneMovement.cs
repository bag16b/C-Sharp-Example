using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
//This script is for DroneMovement / for when not in PlayerForm
public class DroneMovement :  MonoBehaviourPun{

    public Transform SpawnPoint;
    public GameObject DroneProjectile;
    public GameObject DroneMesh;
    public GameObject DroneCamera;

    public Collider myHitbox;

    private GameObject justSpawned;

    public float sensitivity = 150.0f;
    public float smoothing = 2.0f;

    public float movementSpeed = 5f;

    public float coolDownFrames;
    private float coolDown;
    public bool canMove;

    Vector2 mouseLook;
    Vector2 smoothV;


    public Rigidbody rb;

    public float xAxisClamp;

    //[SerializeField] private Transform CameraTransform;

    //this is the answer but I haven't found pretty much ANYTHING online about how to use it so stay tuned
    

    // Use this for initialization
    void Start () {
        coolDown = 0f;
        xAxisClamp = 0.0f;
        object[] data = photonView.InstantiationData;
        int parentID = (int)data[0];
        Debug.Log(data.Length);
        var parent = PhotonView.Find(parentID);
        transform.parent = parent.transform;
        parent.GetComponent<CameraManager>().createDrone(gameObject);

    }

    // Update is called once per frame
    void Update () {
        if (photonView.IsMine)
        {
            if (coolDown > 0f)
            {
                coolDown -= Time.deltaTime;
            }
            if (canMove)
            {
                CameraRotation();
                CameraMovement();
                if (coolDown <= 0f && Input.GetMouseButton(0))
                {
                    //photonView.RPC("Rpc_spawnLaser", RpcTarget.All, SpawnPoint.position, SpawnPoint.rotation);
                    justSpawned = PhotonNetwork.Instantiate(DroneProjectile.name, SpawnPoint.position, gameObject.transform.rotation);
                    coolDown = coolDownFrames; // because of else if's delay

                    //Debug.Log("Fire!");
                }
            }
            else
            {
                if (rb.velocity.magnitude > 0f)
                {
                    rb.velocity = new Vector3(0f, 0f, 0f);
                }
            }
        }
    }

    private void CameraMovement()
    {
        //vertical and horizontal are flipped for whatever reason
        float xInput = Input.GetAxis("DroneVertical") * movementSpeed;
        float yInput = Input.GetAxis("DroneHorizontal") * movementSpeed;

        if(transform.eulerAngles.y < 179)
        {
            yInput = -yInput;
        }
        

        rb.velocity = new Vector3(-yInput, xInput, 0);
    }

    void CameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        xAxisClamp += mouseY;

        if (xAxisClamp > 90)
        {
            xAxisClamp = 90;
            mouseY = 0;
        }
        else if (xAxisClamp < -90)
        {
            xAxisClamp = -90;
            mouseY = 0;
        }

        transform.rotation = Quaternion.Euler(transform.eulerAngles.x + -1 * mouseY, transform.eulerAngles.y + mouseX, 0f);//transform.eulerAngles.z);

        //rotation insurance
        if (GetComponent<Rigidbody>().angularVelocity.magnitude > 0)
        {
            Debug.Log("Drone Rotation Error! Attempting Fix...");
            GetComponent<Rigidbody>().angularVelocity = new Vector3(0f, 0f, 0f);
        }
        //Dog
    }

    

    public void drone_deactivate()
    {
        photonView.RPC("RPC_drone_deactivate", RpcTarget.All);
    }

    public void drone_activate()
    {
        photonView.RPC("RPC_drone_activate", RpcTarget.All);
    }

    [PunRPC]
    public void RPC_drone_deactivate()
    {
        myHitbox.enabled = false;
        canMove = false;
        Renderer renderer = DroneMesh.GetComponent<Renderer>();
        Color col = renderer.material.color;
        col.a = 0.25f;
        renderer.material.color = col;

    }

    [PunRPC]
    public void RPC_drone_activate()
    {
        myHitbox.enabled = true;
        canMove = true;
        Renderer renderer = DroneMesh.GetComponent<Renderer>();
        Color col = renderer.material.color;
        col.a = 1f;
        renderer.material.color = col;

    }

    public void setColor(Color c)
    {
        //the # is added because it cannot be parsed otherwise
        photonView.RPC("Rpc_SetActive", RpcTarget.All, "#"+ColorUtility.ToHtmlStringRGB(c));
    }

    [PunRPC]
    private void Rpc_SetActive(String c)
    {
        Renderer renderer = DroneMesh.GetComponent<Renderer>();
        Color col;// = new Color(0f,0f,0f);
        if (ColorUtility.TryParseHtmlString(c, out col)) {
            col.a = 1f;
            renderer.material.color = col;
            Debug.Log("Drone Should be recolored");
        }
        else
        {
            Debug.Log("OOOHHHHHHNNNNNNNNNNNOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO");
        }

        
        
    }


    
}
