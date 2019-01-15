using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

using UnityEngine;
using UnityEngine.SceneManagement;


using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;


public class GameManager : MonoBehaviourPunCallbacks
{
    public const string CountdownStartTime = "StartTime";

    public static GameManager instance = null;

    private bool isTimerRunning;

    private float startTime;

    [Header("Reference to a Text component for visualizing the countdown")]
    public Text Text;

    [Header("Countdown time in seconds")]
    public float Countdown = 15.0f;

    public Transform[] spawnPoints;

    [Tooltip("The prefab to use for representing the player")]
    public GameObject playerPrefab;

    bool doOnce = false;
    bool doOnce2 = false;
    bool doOnce3 = false;

    float countdown = 0;
    float readyToStartTimer = 0;

    public GameObject zone;

    public int randomSpawnPoint;

    private Transform savedObjectPosition;

    private GameObject[] players;

    public List<GameObject> playerList;

    public GameObject VictoryScreen;

    private Text currentHealth;
    private GameObject Crosshair;
    private GameObject HealthText;

    private float localCountdownStartTime = 0;


    /// <summary>
    /// Called when the local player left the room. We need to load the launcher scene.
    /// </summary>
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    public override void OnPlayerEnteredRoom(Player other)
    {

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom


            //LoadArena("Base");
        }
    }


    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects


        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom


            //LoadArena("Offline");
        }

        playerList.Clear();
        players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            playerList.Add(player);
        }
        Debug.Log("Players = " + players.Length);

        if (PhotonNetwork.PlayerList.Length <= 1)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LeaveRoom();
            }
            else
            {
                Debug.Log("no master");
            }
        }

        if(playerList.Count <= 1)
        {
            if(PhotonNetwork.IsMasterClient)
            {
                LoadArena();
            }
            
        }
    }


    private void Awake()
    {
        //Check if instance already exists
        if (instance == null)

            //if not, set instance to this
            instance = this;

        //If instance already exists and it's not this:
        else if (instance != this)

            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);
    }



    public void Start()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "StartTime", (float)PhotonNetwork.Time }, { "isGameStarted", false } });
            }

            // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
            //PhotonNetwork.Instantiate("PlayerInstance", spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)].position, Quaternion.identity, 0);
            object startTimeFromProps;
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("StartTime", out startTimeFromProps))
            {


                startTime = (float)startTimeFromProps;

            }
            randomSpawnPoint = UnityEngine.Random.Range(0, spawnPoints.Length-1);

            savedObjectPosition = spawnPoints[randomSpawnPoint];



            photonView.RPC("syncSpawnPoints", RpcTarget.AllBufferedViaServer, randomSpawnPoint);

            //prepare HUD
            currentHealth = GameObject.Find("CurrentHealth").GetComponent<Text>();
            Crosshair = GameObject.Find("CrosshairUI");
            HealthText = GameObject.Find("HealthUI");
            Crosshair.SetActive(false);
            HealthText.SetActive(false);

        }
    }

    public void Update()
    {
        float timer;

        timer = (float)PhotonNetwork.Time - startTime;
       
        
        //Debug.Log(readyToStartTimer);
        if (localCountdownStartTime > 0)
        {
            readyToStartTimer = (float) PhotonNetwork.Time - localCountdownStartTime;
            countdown = Countdown - readyToStartTimer;
           
            //Debug.Log(countdown);
        }
        else
        {
            countdown = 1;
           
            Text.text = string.Format("Waiting for players");
        }

        if (((timer > 15 || PhotonNetwork.PlayerList.Length > 1) || (localCountdownStartTime != 0)) && !doOnce)
        {
            if(PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("syncCountdownStart", RpcTarget.AllBuffered, (float) PhotonNetwork.Time);
            }
            isTimerRunning = true;
            doOnce = true;
            //Countdown = readyToStartTimer + 5;
            //Debug.Log(Countdown + " " + readyToStartTimer);
        }

        if (countdown < -2f && !doOnce3 && doOnce2)
        {
            Debug.Log("waited 2 extra seconds");
            playerList.Clear();
            players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                playerList.Add(player);
            }
            Debug.Log("Players = " + players.Length);
            doOnce3 = true;
        }
        

        if (!isTimerRunning)
        {
            return;
        }

        //Debug.Log(countdown);
        if (countdown > 0.0f)
        {
            
            Text.text = string.Format("Game starts in {0} seconds", countdown.ToString("n2"));
            return;
        }


        isTimerRunning = false;
        if (!doOnce2)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "StartTime", startTime }, { "isGameStarted", true } });
            }
            PhotonNetwork.Instantiate("PlayerInstance", savedObjectPosition.position, Quaternion.identity, 0);

            zone.GetComponent<ZoneMaster>().canMove = true;
            doOnce2 = true;
            
            //enable HUD
            Crosshair.SetActive(true);
            HealthText.SetActive(true);
        }
        Text.text = string.Empty;
        
    }

    public void changeCrosshairUI(bool state)
    {
        Crosshair.SetActive(state);
    }
    public void destroyPlayerHUD()
    {
        Crosshair.SetActive(false);
        HealthText.SetActive(false);
    }
    public void changePlayerHealthUI(PlayerHealth p)
    {
        currentHealth.GetComponent<Text>().text = p.health + " / " + p.maxHealth + " : HP";
    }
    public void changeDroneHealthUI(DroneHealth d)
    {
        currentHealth.GetComponent<Text>().text = d.health + " / " + d.maxHealth + " : HP";
    }

    [PunRPC]
    void syncSpawnPoints(int randomSpawnPoint)
    {
        Transform selectedSpawn = spawnPoints[randomSpawnPoint];
        List<Transform> list = new List<Transform>(spawnPoints);
        list.Remove(selectedSpawn);
        spawnPoints = list.ToArray();
    }

    [PunRPC]
    void syncCountdownStart(float countdownStartTime)
    {
        localCountdownStartTime = countdownStartTime;
    }


    public void LeaveRoom()
    {
        //need to remove player from playerlist somehow
        
        foreach (GameObject player in playerList)
        {
            if (player)
            {
                if (player.GetPhotonView().IsMine)
                {
                    PhotonNetwork.Destroy(player);

                }
            }
            
        }
       
        PhotonNetwork.LeaveRoom();
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue(true))
        {
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue(false))
        {
            PhotonNetwork.CurrentRoom.IsVisible = true;
        }

    }


    void LoadArena()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
        }
        Debug.LogFormat("PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
        if(SceneManager.GetActiveScene().name == "Base")
        {
            PhotonNetwork.LoadLevel("Base2");
           
        }
        if (SceneManager.GetActiveScene().name == "Base2")
        {
            PhotonNetwork.LoadLevel("Base");
            
        }
        //PhotonNetwork.LoadLevel(scene);//"Room for " + PhotonNetwork.CurrentRoom.PlayerCount);
    }

    public GameObject removePlayerFromPlayerList(GameObject player)
    {
        playerList.Remove(player);
        GameObject newPlayerTarget;
        if (playerList.Count > 0)
        {
            int random = UnityEngine.Random.Range(0, (playerList.Count));

            newPlayerTarget = playerList[random];

            Debug.Log(newPlayerTarget.name);
        }
        else
        {
            newPlayerTarget = null;
        }
        if(playerList.Count <= 1)
        {
            if(playerList.Count > 0)
            {
                if (playerList[0].GetPhotonView().IsMine)
                {
                    VictoryScreen.SetActive(true);
                }
            }
            if (PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(WaitAFewSeconds());
            }
        }
            
        return newPlayerTarget;
        
    }

    IEnumerator WaitAFewSeconds()
    {
        yield return new WaitForSeconds(3f);
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "StartTime", (float)PhotonNetwork.Time }, { "isGameStarted", false } });
        LoadArena();
    }



}