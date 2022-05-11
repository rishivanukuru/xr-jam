using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.PUN;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Android;
using Photon.Voice.Unity;


/// <summary>
/// Master script to manage the device mode, connection to Photon Servers, as well as the creation/joining of a room.
/// </summary>
public class BasicNetworkingManager : MonoBehaviourPunCallbacks
{
    /// <summary>
    /// This script is meant to be a singleton.
    /// </summary>
    public static BasicNetworkingManager Instance;

    // Enum for the possible modes
    public enum XRJamModes
    {
        Nreal,
        Phone,
        MIDI
    }

    [Header("XR Jam Mode")]
    //Current XR Jam Mode
    [Tooltip("Current XR Jam Mode")]
    public XRJamModes XRJamMode;

    // In-scene objects that should be activated based on the mode
    [Header("Mode Objects")]
    [Tooltip("Nreal Objects")]
    public GameObject[] NrealObjects;
    [Tooltip("Phone Objects")]
    public GameObject[] PhoneObjects;

    [Header("Other players")]
    /// <summary>
    /// A list of all other players in the room.
    /// </summary>
    public List<BasicPlayerHandler> OtherPlayers = new List<BasicPlayerHandler>();

    /// <summary>
    /// A list of all other MIDI clients in the room.
    /// </summary>
    public List<MIDIPlayerHandler> OtherMIDIPlayers = new List<MIDIPlayerHandler>();

    [Header("Camera Rigs")]
    /// <summary>
    /// The main camera object that represents the Nreal glasses.
    /// </summary>
    [Tooltip("The Nreal Camera Rig.")]
    public GameObject NrealCamera;

    /// <summary>
    /// The main camera object that represents the AR Phone Camera.
    /// </summary>
    [Tooltip("The Phone Camera Rig.")]
    public GameObject PhoneCamera;

    [Header("Nreal Hand Parents")]
    // The Nreal Left Hand Parent
    [Tooltip("The Nreal Left Hand Parent")]
    public GameObject LeftHandParent;
    // The Nreal Right Hand Parent
    [Tooltip("The Nreal Right Hand Parent")]
    public GameObject RightHandParent;

    [Header("Room Objects")]

    // Central object that acts as a reference for all Photon operations
    [Tooltip("The reference point for spatial interactions.")]
    public Transform ReferenceObject;

    // Login menu
    [SerializeField]
    [Tooltip("Login Menu gameobject")]
    private GameObject _introMenu;

    // Room menu
    [SerializeField]
    [Tooltip("Room menu gameobject")]
    private GameObject _roomMenu;

    [Header("Photon Objects")]
    /// <summary>
    /// The game's voice network.
    /// </summary>
    [Tooltip("The Photon Voice network holder.")]
    public PhotonVoiceNetwork VoiceNetwork;

    // Flag to know if the local player has been instantiated or not.
    public bool IsPlayerInstantiated { get; private set; }

    // Reference to the local player.
    public GameObject LocalPlayer { get; private set; }

    // Reference to the current player's name.
    public string PlayerName;
    
    // Room Options control the room size and the time for which rooms should be kept active if empty, among other properties.
    RoomOptions _roomOptions;

    // Index to hold the current microphone - for mic testing on Nreal
    int _currentMicrophoneIndex = 0;

    private void Awake()
    {
        // If phone mode
        if(XRJamMode == XRJamModes.Phone)
        {
            // Force app orientation to be landscape left
            Screen.orientation = ScreenOrientation.LandscapeLeft;

            // Activate appropriate scene objects
            foreach (GameObject g in NrealObjects)
            {
                g.SetActive(false);
            }
            foreach(GameObject g in PhoneObjects)
            {
                g.SetActive(true);
            }
        }
        else
        {
            // Force app orientation to be portrait
            Screen.orientation = ScreenOrientation.Portrait;

            // Activate appropriate objects
            foreach (GameObject g in NrealObjects)
            {
                g.SetActive(true);
            }
            foreach (GameObject g in PhoneObjects)
            {
                g.SetActive(false);
            }
        }

        // Set all menus to be inactive
        _introMenu.SetActive(false);
        _roomMenu.SetActive(false);

        // Set the instatiation flag to false on awake.
        IsPlayerInstantiated = false;

        // Setting singleton.
        if (Instance is null)
            Instance = this;
        else if (Instance != this)
            Destroy(this);

        // Checking for Camera and Microphone permissions on Android.
#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }
#endif

        Debug.Log("Trying to connect to server");

        // Initializing room options.
        _roomOptions = new RoomOptions();
        _roomOptions.CleanupCacheOnLeave = true;

        // Setting the room timeout durations.
        if (!Application.isEditor)
        {
            _roomOptions.EmptyRoomTtl = 3600;
            _roomOptions.PlayerTtl = 3600;
        }

        // Attempt to connect to Photon servers.
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }


    }


    #region PunCallbacks

    /// <summary>
    /// Join lobby once Photon Connection is established
    /// </summary>
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to server");
        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("OnDisconnected() was called by PUN with reason {0}", cause);
    }

    /// <summary>
    /// Set intro menu to be active once a Lobby is joined
    /// </summary>
    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
        _introMenu.SetActive(true);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Room Created");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogWarningFormat("Create Room failed with reason {0}", message);
    }

    /// <summary>
    /// Once a room is joined, toggle the active menus, and spawn the local player
    /// </summary>
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room");
        _introMenu.SetActive(false);
        _roomMenu.SetActive(true);

        StartCoroutine(SpawnPlayer());
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarningFormat("Join room failed with reason {0}", message);
    }

    #endregion

    /// <summary>
    /// Coroutine to create or join a new XR Jam room
    /// </summary>
    /// <returns></returns>
    IEnumerator CreateOrJoinRoomCoroutine()
    {
        yield return new WaitForSeconds(1);
        PhotonNetwork.JoinOrCreateRoom("xrjam", _roomOptions, TypedLobby.Default);
    }

    /// <summary>
    ///  Function to call the create or join coroutine
    /// </summary>
    public void AttemptCreateOrJoin()
    {
        StartCoroutine(CreateOrJoinRoomCoroutine());
    }

    /// <summary>
    /// Spawns the player once a room has been joined
    /// </summary>
    /// <returns></returns>
    IEnumerator SpawnPlayer()
    {
        yield return new WaitForSeconds(1);
        // Spawn player here.

        if (!IsPlayerInstantiated)
        {
            // Instantiates player based on mode
            switch(XRJamMode)
            {
                case XRJamModes.Nreal:
                    LocalPlayer = PhotonNetwork.Instantiate("AvatarAnimV1", NrealCamera.transform.position, NrealCamera.transform.rotation);
                    break;
                case XRJamModes.Phone:
                    LocalPlayer = PhotonNetwork.Instantiate("AvatarAnimV1", PhoneCamera.transform.position, PhoneCamera.transform.rotation);
                    break;
                case XRJamModes.MIDI:
                    VoiceNetwork.enabled = false;
                    LocalPlayer = PhotonNetwork.Instantiate("BasicMIDISender", NrealCamera.transform.position, NrealCamera.transform.rotation);
                    break;
                default:
                    break;
            }
            
            IsPlayerInstantiated = true;
        }
    }

    /// <summary>
    /// Switches the current debug echo mode
    /// (Not currently in use)
    /// </summary>
    public void DebugPhotonSpeechToggle()
    {
        LocalPlayer.GetComponent<Recorder>().DebugEchoMode = !LocalPlayer.GetComponent<Recorder>().DebugEchoMode;
    }

    /// <summary>
    /// Activates Debug Speech
    /// (Not currently in use)
    /// </summary>
    public void DebugPhotonSpeechOn()
    {
        LocalPlayer.GetComponent<Recorder>().DebugEchoMode = true;
    }

    /// <summary>
    /// Deactivates Debug Speech
    /// (Not currently in use)
    /// </summary>
    public void DebugPhotonSpeechOff()
    {
        LocalPlayer.GetComponent<Recorder>().DebugEchoMode = false;
    }

    /// <summary>
    /// Changes active photon microphone
    /// </summary>
    public void ChangeMicrophone()
    {
        Recorder _recorder = LocalPlayer.GetComponent<Recorder>();
        int nextIndex = (_currentMicrophoneIndex + 1 >= Microphone.devices.Length) ? 0 : _currentMicrophoneIndex + 1;
        _currentMicrophoneIndex = nextIndex;
        _recorder.UnityMicrophoneDevice = Microphone.devices[_currentMicrophoneIndex];
        _recorder.RestartRecording();
    }

    /// <summary>
    /// Turns local avatar on or off
    /// </summary>
    public void ToggleAvatar()
    {
        LocalPlayer.GetComponent<BasicPlayerHandler>().ToggleRender();
    }

    /// <summary>
    /// Sets player name based on input field, assigns as photon network nickname as well
    /// </summary>
    /// <param name="name"></param>
    public void SetPlayerName(string name)
    {
        PlayerName = name;
        // Adding MIDI suffix to avoid nickname clash
        if (XRJamMode == XRJamModes.MIDI)
        {
            PhotonNetwork.NickName = PlayerName + "MIDI";
        }
        else
        {
            PhotonNetwork.NickName = PlayerName;
        }
    }

    /// <summary>
    /// Calls the MIDI assignment function on all players in the room
    /// </summary>
    /// <param name="midiObject"></param>
    public void AssignMIDIPlayer(GameObject midiObject)
    {
        foreach(BasicPlayerHandler p in OtherPlayers)
        {
            p.AssignMIDItoPlayer(midiObject);
        }
        LocalPlayer.GetComponent<BasicPlayerHandler>().AssignMIDItoPlayer(midiObject);
    }


    /// <summary>
    /// Singleton closure.
    /// </summary>
    private void OnDestroy()
    {
        Instance = null;
    }

}
