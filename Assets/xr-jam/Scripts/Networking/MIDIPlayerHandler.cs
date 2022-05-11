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
/// Script that collects Avatar MIDI animation information sent over the network, and assigns it to the Instrument rig manager of the corresponding player.
/// </summary>
public class MIDIPlayerHandler : MonoBehaviourPunCallbacks, IPunObservable, IPunInstantiateMagicCallback
{
    [Header("Overall Parameters")]
    //Current Instrument Mode
    [Tooltip("Current Instrument Mode")]
    public int InstrumentMode = 0;

    // Current Player MIDI Name
    [Tooltip("Current Player MIDI Name")]
    public string MIDIname = "";

    //Keyboard Values
    [Header("Keyboard")]
    // Keyboard left hand position
    [Tooltip("Keyboard left hand position")]
    public float _leftHandPosKey;

    // Keyboard right hand position
    [Tooltip("Keyboard right hand position")]
    public float _rightHandPosKey;

    // Keyboard left hand hit
    [Tooltip("Keyboard left hand hit")]
    public bool _leftHandKeyHit = false;

    // Keyboard right hand hit
    [Tooltip("Keyboard right hand hit")]
    public bool _rightHandKeyHit = false;

    // Drum Values
    [Header("Drum")]

    // Drum left hit
    [Tooltip("Drum left hit")]
    public bool _leftDrumHit = false;

    // Drum right hit
    [Tooltip("Drum right hit")]
    public bool _rightDrumHit = false;

    //Guitar Values
    [Header("Guitar")]

    // Guitar left hand position
    [Tooltip("Guitar left hand position")]
    public float _leftHandPosGuitar;

    // Guitar strum state
    [Tooltip("Guitar strum state")]
    public bool _strum = false;

    // Reference to the MIDI processor
    private MIDIProcessor _midiProcessor;

    private void Awake()
    {
        // Grab local Midi Processor
        _midiProcessor = this.GetComponent<MIDIProcessor>();
    }

    void IPunInstantiateMagicCallback.OnPhotonInstantiate(PhotonMessageInfo info)
    {
        if (photonView.IsMine)
        {
            // Activate MIDI processor and setup OSC only if the MIDI player is the local player
            _midiProcessor.enabled = true;
            _midiProcessor.SetupOSC();
        }
        else
        {
            // Add to other MIDI players list
            BasicNetworkingManager.Instance.OtherMIDIPlayers.Add(this);

            // Check if any of the other players matches the current MIDI player, if yes, assign the two to each other
            BasicNetworkingManager.Instance.AssignMIDIPlayer(this.gameObject);
        }

    }

    void Update()
    {
        /*
         * Keyboard controls for changing the MIDI mode
         * H - Hands
         * K - Keyboard
         * D - Drums
         * G - Guitar        
         */

        if(photonView.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                SetInstrumentRig(0);
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                SetInstrumentRig(1);
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                SetInstrumentRig(2);
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                SetInstrumentRig(3);
            }
        }



    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Player Name
            stream.SendNext(BasicNetworkingManager.Instance.PlayerName);

            // Instrument Mode
            stream.SendNext(InstrumentMode);
            
            // Key Hand Positions
            stream.SendNext(_midiProcessor.KeyLeftHand);            
            stream.SendNext(_midiProcessor.KeyRightHand);

            // Key Hit States
            stream.SendNext(_midiProcessor._leftKeyHit);
            stream.SendNext(_midiProcessor._rightKeyHit);

            // Guitar Position
            stream.SendNext(_midiProcessor.GuitarLeftHand);

            // Guitar Strum
            stream.SendNext(_midiProcessor._shouldStrum);

            // Drum Hits
            stream.SendNext(_midiProcessor._leftDrumHit);
            stream.SendNext(_midiProcessor._rightDrumHit);
            
        }
        else
        {

            // Player Name
            MIDIname = (string)stream.ReceiveNext();

            // Instrument Mode
            InstrumentMode = (int)stream.ReceiveNext();
            _midiProcessor.InstrumentMode = InstrumentMode;
            
            // Keyboard
            _leftHandPosKey = (float)stream.ReceiveNext();
            _rightHandPosKey = (float)stream.ReceiveNext();

            _leftHandKeyHit = (bool)stream.ReceiveNext();
            _rightHandKeyHit = (bool)stream.ReceiveNext();
            
            // Guitar
            _leftHandPosGuitar = (float)stream.ReceiveNext();

            _strum = (bool)stream.ReceiveNext();

            // Drum
            _leftDrumHit = (bool)stream.ReceiveNext();
            _rightDrumHit = (bool)stream.ReceiveNext();

        }
    }

    /// <summary>
    /// Set the appropriate instrument rig across all versions of the player
    /// </summary>
    /// <param name="index"></param>
    public void SetInstrumentRig(int index)
    {
        InstrumentMode = index;
        photonView.RPC("RPC_SetInstrumentRig", RpcTarget.OthersBuffered, index, BasicNetworkingManager.Instance.PlayerName);
    }

    [PunRPC]
    public void RPC_SetInstrumentRig(int index, string playerName)
    {

        InstrumentMode = index;

    }

    /* 
     * 
     * Photon RPC Section (not actively in use)
     * 
     * 
     */

    public void LeftHitKey(bool state)
    {
        photonView.RPC("RPC_LeftHitKey", RpcTarget.OthersBuffered, state, BasicNetworkingManager.Instance.PlayerName);
    }

    [PunRPC]
    public void RPC_LeftHitKey(bool state, string playerName)
    {
        if(BasicNetworkingManager.Instance.PlayerName == playerName)
        {
            BasicNetworkingManager.Instance.LocalPlayer.GetComponent<InstrumentRigManager>().LeftHitKey(state);
        }

    }

    public void RightHitKey(bool state)
    {
        photonView.RPC("RPC_RightHitKey", RpcTarget.OthersBuffered, state, BasicNetworkingManager.Instance.PlayerName);

    }

    [PunRPC]
    public void RPC_RightHitKey(bool state, string playerName)
    {
        if (BasicNetworkingManager.Instance.PlayerName == playerName)
        {
            BasicNetworkingManager.Instance.LocalPlayer.GetComponent<InstrumentRigManager>().RightHitKey(state);

        }
        
    }

    public void LeftHitDrum(bool state)
    {
        photonView.RPC("RPC_LeftHitDrum", RpcTarget.OthersBuffered, state, BasicNetworkingManager.Instance.PlayerName);

    }

    [PunRPC]
    public void RPC_LeftHitDrum(bool state, string playerName)
    {
        if (BasicNetworkingManager.Instance.PlayerName == playerName)
        {
            BasicNetworkingManager.Instance.LocalPlayer.GetComponent<InstrumentRigManager>().LeftHitDrum(state);

        }

    }

    public void RightHitDrum(bool state)
    {
        photonView.RPC("RPC_RightHitDrum", RpcTarget.OthersBuffered, state, BasicNetworkingManager.Instance.PlayerName);

    }

    [PunRPC]
    public void RPC_RightHitDrum(bool state, string playerName)
    {
        if (BasicNetworkingManager.Instance.PlayerName == playerName)
        {
            BasicNetworkingManager.Instance.LocalPlayer.GetComponent<InstrumentRigManager>().RightHitDrum(state);

        }

    }

    public void Strum(bool state)
    {
        photonView.RPC("RPC_Strum", RpcTarget.OthersBuffered, state, BasicNetworkingManager.Instance.PlayerName);

    }

    [PunRPC]
    public void RPC_Strum(bool state, string playerName)
    {
        if (BasicNetworkingManager.Instance.PlayerName == playerName)
        {
            BasicNetworkingManager.Instance.LocalPlayer.GetComponent<InstrumentRigManager>().Strum(state);
        }
        
    }

}
