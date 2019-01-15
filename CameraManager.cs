using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

//This Script is for managing creating, and disabling and reenabling each player's cameras
public class CameraManager : MonoBehaviourPun {

    private Text currentHealth;
    private GameObject Crosshair;
    private GameObject HealthText;
    public GameObject Miner;
    //public GameObject DronePrefab;
    [HideInInspector]
    public GameObject Drone;
    //public GameObject DroneCameraCamera;
    public OnClickPlayer onClickPlayer;
    public ShootReflector shootReflector;
    private GameObject PlayerCamera;
    [HideInInspector]
    public DroneMovement droneMovement;

    private Quaternion droneStartingRotation;

    private bool checkScripts;
    private bool activateScripts;
    public bool firstDroneActivate;
    private bool pressed;

    private float droneCastTime;
    

    // Use this for initialization
    void Start () {

        //droneStartingRotation = DroneCamera.transform.rotation;

        if(photonView.IsMine)
        {
            PlayerCamera = Camera.main.gameObject;
            PlayerCamera.GetComponent<CameraFollow>().player = Miner; //Fix camera on miner
            activateScripts = true;
            checkScripts = false;
            firstDroneActivate = true;
            droneCastTime = 0f;
        }
        else
        {
            onClickPlayer.enabled = false;
            shootReflector.enabled = false;
            Miner.GetComponent<CustomMovementTest>().playerControl = false;
        }
    }

    // Update is called once per frame
    void Update() {

        if (!photonView.IsMine)
        {
            return;
        }

        if (Input.GetKeyDown("q"))
        {
            pressed = true;
        }

        if (pressed && Input.GetKey("q")){
            float castTime = 0f;
            if (firstDroneActivate)
            {
                //spawning drone
                castTime = 0.75f;
            }else if (PlayerCamera.activeInHierarchy)
            {
                //returning to spawned drone
                castTime = 0.5f;
            }
            else
            {
                //returning to player
                castTime = 0.25f;
            }

            droneCastTime += Time.deltaTime;

            if (droneCastTime > castTime)
            {
                pressed = false;
                droneCastTime = 0f;
                activateScripts = !activateScripts;
                if (firstDroneActivate)
                {
                    firstDroneActivate = false;
                    if (photonView.IsMine)
                    {
                        object[] data = new object[1];
                        data[0] = photonView.ViewID;
                        Drone = PhotonNetwork.Instantiate("DroneCamera", gameObject.transform.position, gameObject.transform.rotation, 0, data);
                        droneMovement = Drone.GetComponent<DroneMovement>();
                        droneMovement.setColor(Miner.GetComponent<Renderer>().material.color);
                    }

                    //photonView.RPC("createDrone", RpcTarget.All);

                    //Drone.transform.position = new Vector3(Miner.transform.position.x, Miner.transform.position.y + 0.5f, Miner.transform.position.z);
                    activateScripts = false;
                }

                checkScripts = true;
            }
        }else if(droneCastTime > 0)
        {
            droneCastTime = 0f;
        }

        if (checkScripts)
        {
            checkScripts = false;
            if (activateScripts)
            {
                PlayerCamera.SetActive(true);
                Miner.GetComponent<CustomMovementTest>().playerControl = true;
                onClickPlayer.enabled = true;
                shootReflector.enabled = true;
                droneMovement.drone_deactivate();
            }
            else
            {
                PlayerCamera.SetActive(false);
                Miner.GetComponent<CustomMovementTest>().playerControl = false;
                onClickPlayer.enabled = false;
                shootReflector.enabled = false;
                droneMovement.drone_activate();
            }
        }

        //bad code but it'll work
        if (Miner)
        {
            if (PlayerCamera.activeInHierarchy)
            {
                GameManager.instance.changeCrosshairUI(false);
                GameManager.instance.changePlayerHealthUI(Miner.GetComponent<PlayerHealth>());
            }
            else
            {
                GameManager.instance.changeCrosshairUI(true);
                GameManager.instance.changeDroneHealthUI(Drone.GetComponent<DroneHealth>());
            }
        }
    }

    public void resetDrone()
    {
        //if (photonView.IsMine)
        //{
            firstDroneActivate = true;
            photonView.RPC("destroyDrone", RpcTarget.All);
            PlayerCamera.SetActive(true);
            Miner.GetComponent<CustomMovementTest>().playerControl = true;
            onClickPlayer.enabled = true;
            shootReflector.enabled = true;
            activateScripts = true;
            checkScripts = false;
        //}

    }

    [PunRPC]
    public void createDrone(GameObject d)
    {
        Drone = d;
        Drone.transform.parent = gameObject.transform;
        Drone.transform.position = Miner.transform.position;
        droneMovement = Drone.GetComponent<DroneMovement>();
        droneMovement.setColor(Miner.GetComponent<Renderer>().material.color);
        Drone.GetComponent<DroneHealth>().playerInstance = gameObject;
        if (!photonView.IsMine)
        {
            droneMovement.DroneCamera.SetActive(false);
        }
    }

    [PunRPC]
    public void destroyDrone()
    {
        Destroy(Drone);
    }
}
