using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.PUN;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System;
using Photon.Voice.Unity;

/// <summary>
/// Deals with the networked player (Nreal and AR Phone)
/// </summary>
public class BasicPlayerHandler : MonoBehaviourPunCallbacks, IPunObservable, IPunInstantiateMagicCallback
{
    //Smoothing for player movement and rotation
    [Tooltip("Smoothing for player movement and rotation")]
    [SerializeField]
    private float _lerpMultiplier = 80.0f;

    //Holder variables Used to get new transform vlaues through network
    private Vector3 _newPos;
    private Quaternion _newRot;

    // The Avatar gameobject
    [SerializeField]
    [Tooltip("The Avatar gameobject")]
    private GameObject _avatar;

    // The head target of the avatar
    [SerializeField]
    [Tooltip("The head target of the avatar")]
    private GameObject _head;

    // Local photon view
    [Tooltip("Local Photon View")]
    public PhotonView PhotonView;

    // All objects to be rendered/hidden in self view
    [Tooltip("All parent objects to be rendered/hidden in self view")]
    public GameObject[] _renderedObjects;

    [Tooltip("Flag to know if self avatar should be rendered")]
    public bool _renderObjects = true;

    // Photon Voice View
    [Tooltip("Photon Voice View")]
    public PhotonVoiceView photonVoiceView;
    
    // Audio Reactive Avatar Parameters
    // (not currently in use)
    private AudioSource AudioSrc;
    private float[] AudioData;
    private int SampleSize = 1024;

    // Generated color for current player
    public Color PlayerColor { get; private set; }

    private void Awake()
    {
        // Set the initial transform values.
        _newPos = transform.position;
        _newRot = transform.rotation;

        // Audio Reactive Avatar assignment (not in use)
        // AudioSrc = GetComponent<AudioSource>();
        // AudioData = new float[SampleSize];

        // Grab local photon view component
        PhotonView = this.GetComponent<PhotonView>();

    }

    /// <summary>
    /// Callback function for once the player has been instatiated on the Photon Network.
    /// </summary>
    /// <param name="info"></param>
    void IPunInstantiateMagicCallback.OnPhotonInstantiate(PhotonMessageInfo info)
    {

        if (photonView.IsMine)
        {
            // Move local avatar along with the Nreal.
            transform.parent = BasicNetworkingManager.Instance.XRJamMode == BasicNetworkingManager.XRJamModes.Phone ? BasicNetworkingManager.Instance.PhoneCamera.transform : BasicNetworkingManager.Instance.NrealCamera.transform;

            // Attach to Photon Voice Network
            BasicNetworkingManager.Instance.VoiceNetwork.PrimaryRecorder = this.gameObject.GetComponent<PhotonVoiceView>().RecorderInUse;
        }
        else
        {
            Debug.Log("ADDED PLAYER TO OTHER PLAYERS - " + PhotonView.Owner.NickName);
            
            // Add the player to the OtherPlayers list.
            BasicNetworkingManager.Instance.OtherPlayers.Add(this);
        }

        // Generat unique player color
        CreatePlayerColor();

        // Broadcast color to all receivers in the gameobject hierarchy
        BroadcastMessage("SetPlayerColor", PlayerColor, SendMessageOptions.DontRequireReceiver);
    }

    private void Update()
    {
        // For all non-local players, modify their transforms based on the information received from Photon.
        if (!photonView.IsMine)
        {
            // Avatar is placed at the position
            transform.position = Vector3.Lerp(transform.position, _newPos, Time.deltaTime * _lerpMultiplier);
            
            // Only head is rotated
            _head.transform.rotation = _newRot;

            // Calculate whether the body should face towards or away from the central reference object
            Vector3 _toCenter = transform.position - BasicNetworkingManager.Instance.ReferenceObject.transform.position;
            _toCenter = new Vector3(_toCenter.x, 0, _toCenter.z);

            Vector3 _headForward = new Vector3(_head.transform.forward.x, 0, _head.transform.forward.z);

            if(Vector3.Dot(_headForward,_toCenter)>0)
            {
                transform.rotation = Quaternion.LookRotation(_toCenter, Vector3.up);
            }
            else
            {
                transform.rotation = Quaternion.LookRotation(-1*_toCenter, Vector3.up);
            }

        }
        else
        {
            // For local players, similarly apply position to body, and rotation to head
            _avatar.transform.position = transform.position;
            _head.transform.rotation = transform.rotation;


            // Similarly calculate body look direction
            Vector3 _toCenter = transform.position - BasicNetworkingManager.Instance.ReferenceObject.transform.position;
            _toCenter = new Vector3(_toCenter.x, 0, _toCenter.z);

            Vector3 _headForward = new Vector3(_head.transform.forward.x, 0, _head.transform.forward.z);

            if (Vector3.Dot(_headForward, _toCenter) > 0)
            {
                _avatar.transform.rotation = Quaternion.LookRotation(_toCenter, Vector3.up);
            }
            else
            {
                _avatar.transform.rotation = Quaternion.LookRotation(-1 * _toCenter, Vector3.up);
            }


        }

        /*
        // For Audio Reactive Avatar (not in use)
        if (photonVoiceView.RecorderInUse != null)
        {
            if (photonVoiceView.RecorderInUse.TransmitEnabled)
            {
                float rms = GetRmsValue();
                // Debug.Log("RMS: " + rms.ToString());
                
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Lerp(1.0f, 2.0f, rms);
                scale.y = Mathf.Lerp(1.0f, 2.0f, rms);
                scale.z = Mathf.Lerp(1.0f, 2.0f, rms);
                transform.localScale = scale;
            }
        }
        */

    }

    /// <summary>
    /// Sends the position and rotation of a player relative to the reference object.
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="info"></param>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(BasicNetworkingManager.Instance.ReferenceObject.InverseTransformPoint(transform.position));
            stream.SendNext(Quaternion.Inverse(BasicNetworkingManager.Instance.ReferenceObject.rotation) * transform.rotation);
        }
        else
        {
            _newPos = BasicNetworkingManager.Instance.ReferenceObject.TransformPoint((Vector3)stream.ReceiveNext());
            _newRot = BasicNetworkingManager.Instance.ReferenceObject.rotation * (Quaternion)stream.ReceiveNext();
        }
    }

    /// <summary>
    /// Calculates RMS value of an Audioclip
    /// (not in use)
    /// </summary>
    /// <returns></returns>
    private float GetRmsValue()
    {
        AudioSrc.GetOutputData(AudioData, 0);
        float sum = 0;

        for (int i = 0; i < SampleSize; i++)
            sum += AudioData[i] * AudioData[i];

        sum = Mathf.Sqrt(sum / SampleSize);
        return sum;
    }

    /// <summary>
    /// Checks if a newly entered MIDI player corresponds to this player
    /// If yes, assigns the MIDI player to the Instrument Rig of this player
    /// </summary>
    /// <param name="midiObject"></param>
    public void AssignMIDItoPlayer(GameObject midiObject)
    {
        if(midiObject.GetComponent<PhotonView>().Owner.NickName == (photonView.Owner.NickName + "MIDI"))
        {
            Debug.Log("Assigned MIDI to Player: " + photonView.Owner.NickName);
            this.gameObject.GetComponent<InstrumentRigManager>()._midiPlayerHandler = midiObject.GetComponent<MIDIPlayerHandler>();
        }
    }

    /// <summary>
    /// Toggles rendering self avatar objects
    /// </summary>
    public void ToggleRender()
    {
        _renderObjects = !_renderObjects;
        // Toggles renderers in all children of the objects in the parent list
        foreach(GameObject g in _renderedObjects)
        {
            List<Renderer> _renderers = new List<Renderer>();
            _renderers.AddRange(g.GetComponentsInChildren<Renderer>());
            foreach (Renderer rend in _renderers)
                rend.enabled = _renderObjects;            
        }
    }

    /// <summary>
    /// Generates a unique player color based on the player name.
    /// </summary>
    void CreatePlayerColor()
    {
        // Hue is linked to the first letter of the name
        float myH = ((float)((int)(photonView.Owner.NickName.ToLower()[0]) - 96) / 26f) * 0.8f;
        
        // Saturation is linked to the length of the name
        float myS = 0.5f + ((float)(photonView.Owner.NickName.Length) / 20f) / 3f;

        // Brightness is fixed
        float myV = 0.99f; 
        PlayerColor = Color.HSVToRGB(myH, myS, myV);
    }



}
