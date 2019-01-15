using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class Menu : MonoBehaviourPunCallbacks
{

    
    [Tooltip("The UI Label to inform the user that the connection is in progress")]
    [SerializeField]
    private GameObject progressLabel;

    [Header("Login Panel")]
    public GameObject LoginPanel;

    [Header("Selection Panel")]
    public GameObject SelectionPanel;


    /// <summary>
    /// The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created.
    /// </summary>
    [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
    [SerializeField]
    private byte maxPlayersPerRoom = 0;

    private string gameVersion = "1";
    private bool isConnecting;
    private Dictionary<string, RoomInfo> cachedRoomList;
    private Dictionary<string, GameObject> roomListEntries;

    public GameObject RoomListEntryPrefab;
    public GameObject RoomListContent;

    public GameObject RoomNameField;
    public InputField PlayerNameInput;



    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
    /// </summary>
    void Awake()
    {
        // #Critical
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        roomListEntries = new Dictionary<string, GameObject>();
        cachedRoomList = new Dictionary<string, RoomInfo>();
        
       
    }

    void Start()
    {
        progressLabel.SetActive(false);
        LoginPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        // #Critical, we must first and foremost connect to Photon Online Server.
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("RoomListUpdated");
        foreach (RoomInfo room in roomList)
        {
            //GameObject newAnimal = Instantiate(roomListPrefab) as GameObject;
            //newAnimal.transform.parent = gameObject.transform;
        }
        ClearRoomListView();
        UpdateCachedRoomList(roomList);
        UpdateRoomListView();
    }


    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        
        foreach (RoomInfo info in roomList)
        {
            // Remove room from cached room list if it got closed, became invisible or was marked as removed
            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList.Remove(info.Name);
                }

                continue;
            }

            // Update cached room info
            if (cachedRoomList.ContainsKey(info.Name))
            {
                cachedRoomList[info.Name] = info;
            }
            // Add new room info to cache
            else
            {
                cachedRoomList.Add(info.Name, info);
            }
        }
    }

    private void UpdateRoomListView()
    {
        foreach (RoomInfo info in cachedRoomList.Values)
        {
            GameObject entry = Instantiate(RoomListEntryPrefab);
            entry.transform.SetParent(RoomListContent.transform);
            entry.transform.localScale = Vector3.one;
            entry.GetComponent<MyRoomList>().Initialize(info.Name, (byte)info.PlayerCount, info.MaxPlayers);

            roomListEntries.Add(info.Name, entry);
        }
    }

    private void ClearRoomListView()
    {
        foreach (GameObject entry in roomListEntries.Values)
        {
            Destroy(entry.gameObject);
        }

        roomListEntries.Clear();
    }

    public void OnLoginButtonClicked()
    {
        string playerName = PlayerNameInput.text;

        if (!playerName.Equals(""))
        {
            PhotonNetwork.LocalPlayer.NickName = playerName;
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.LogError("Player Name is invalid.");
        }
    }




    


    


    /// <summary>
    /// Start the connection process.
    /// - If already connected, we attempt joining a random room
    /// - if not yet connected, Connect this application instance to Photon Cloud Network
    /// </summary>
    public void Connect()
    {
        isConnecting = true;
        progressLabel.SetActive(true);
        LoginPanel.SetActive(false);
        // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
        if (PhotonNetwork.IsConnected)
        {
            RoomOptions roomOptions = new RoomOptions();
            //roomOptions.CustomRoomProperties = new Hashtable() { { "StartTime", 10 } };
            //roomOptions.CustomRoomPropertiesForLobby = new string[] { "StartTime" };
            //roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "StartTime", (float)PhotonNetwork.Time} };
            //roomOptions.CustomRoomProperties.Add("StartTime", PhotonNetwork.Time);
            // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
            PhotonNetwork.CreateRoom(RoomNameField.GetComponent<InputField>().text, roomOptions, null, null);
        }
        else
        {

            Debug.Log("Not connected!!!");
        }


    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        Debug.Log("Room props changed");
        object isGameStartedFromProps;
        if (propertiesThatChanged.TryGetValue("isGameStarted", out isGameStartedFromProps))
        {
            if((bool) isGameStartedFromProps)
            {
                
            }
        }
    }



    public override void OnConnectedToMaster()
    {
        Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
        this.SetActivePanel(SelectionPanel.name);
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined the lobby");
    }

    private void SetActivePanel(string activePanel)
    {
        LoginPanel.SetActive(activePanel.Equals(LoginPanel.name));
        SelectionPanel.SetActive(activePanel.Equals(SelectionPanel.name));
    }


    public override void OnDisconnected(DisconnectCause cause)
    {
        progressLabel.SetActive(false);
        LoginPanel.SetActive(true);
        Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom\n\n");

        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        RoomOptions roomOptions = new RoomOptions();
        //roomOptions.CustomRoomProperties = new Hashtable() { { "StartTime", 10 } };
        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
        roomOptions.CustomRoomProperties.Add("StartTime", PhotonNetwork.Time);
        PhotonNetwork.CreateRoom(null, roomOptions, null, null);
        
        
        
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
        // #Critical: We only load if we are the first player, else we rely on `PhotonNetwork.AutomaticallySyncScene` to sync our instance scene.
        
            Debug.Log("We load the 'Room for 1' ");


            // #Critical
            // Load the Room Level.
            if(PhotonNetwork.IsMasterClient)
            {
            PhotonNetwork.LoadLevel("Base");
            }
            
        
    }

}