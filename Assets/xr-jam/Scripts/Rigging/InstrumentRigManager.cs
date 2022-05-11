using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// Script that handles the active instrument Rig, and acts as a bridge for MIDI information from clients to rigs
/// </summary>
public class InstrumentRigManager : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("State handlers")]
    // Assigned automatically in Run Time - The MIDI Player corresponding to the local player
    [Tooltip("Assigned automatically in Run Time - The MIDI Player corresponding to the local player")]
    public MIDIPlayerHandler _midiPlayerHandler = null;

    // Currently Active Rig Index
    [Tooltip("Currently Active Rig Index")]
    public int CurrentRig;
 
    [Header("Hands")]
    // Nreal hand-tracking animation rig
    [Tooltip("Nreal hand-tracking animation rig")]
    public Rig HandsRig;

    [Header("Keyboard")]
    // Keyboard animation rig
    [Tooltip("Keyboard animation rig")]
    public Rig KeyboardRig;

    // Keyboard animation rig handler
    [Tooltip("Keyboard animation rig handler")]
    public KeyboardRigHandler KeyboardRigHandler;
    
    // Keyboard visual gameobject
    [Tooltip("Keyboard visual gameobject")]
    public GameObject Keyboard;

    [Header("Drum")]
    // Drum Rig
    [Tooltip("Drum Rig")]
    public Rig DrumRig;

    // Drum Rig Handler
    [Tooltip("Drum Rig Handler")]
    public DrumRigHandler DrumRigHandler;
    
    // Drum Object
    [Tooltip("Drum Object")]
    public GameObject Drum;

    [Header("Guitar")]
    // Guitar Rig
    [Tooltip("Guitar Rig")]
    public Rig GuitarRig;
    
    // Guitar Rig Handler
    [Tooltip("Guitar Rig Handler")]
    public GuitarRigHandler GuitarRigHandler;
    
    // Guitar Object
    [Tooltip("Guitar Object")]
    public GameObject Guitar;
    

    void Start()
    {
        // Initialize nreal hand rig to be active, all others inactive.
        // Deactivate the instrument objects

        HandsRig.weight = 1;
        KeyboardRig.weight = 0;
        Keyboard.SetActive(false);

        DrumRig.weight = 0;
        Drum.SetActive(false);

        GuitarRig.weight = 0;
        Guitar.SetActive(false);

        CurrentRig = 0;
    }

    void Update()
    {
        // As long as an midi player for this current player exists
        if(_midiPlayerHandler!=null)
        {
            // Hand mode
            if(_midiPlayerHandler.InstrumentMode == 0)
            {
                // Switch Rigs
                if (CurrentRig != 0)
                {
                    SetInstrumentRigs(0);
                }
            }
            else
            // Keyboard mode
            if (_midiPlayerHandler.InstrumentMode == 1)
            {
                // Switch Rigs
                if (CurrentRig != 1)
                {
                    SetInstrumentRigs(1);
                }

                // Apply the keyboard position and hit variables
                KeyboardRigHandler.LeftHandPosition = _midiPlayerHandler._leftHandPosKey;
                KeyboardRigHandler.RightHandPosition = _midiPlayerHandler._rightHandPosKey;

                KeyboardRigHandler.LeftHit(_midiPlayerHandler._leftHandKeyHit);
                KeyboardRigHandler.RightHit(_midiPlayerHandler._rightHandKeyHit);


            }
            else
            // Drum Mode
            if (_midiPlayerHandler.InstrumentMode == 2)
            {
                // Switch Rigs
                if (CurrentRig != 2)
                {
                    SetInstrumentRigs(2);
                }

                // Apply drum hit values
                DrumRigHandler.LeftHit(_midiPlayerHandler._leftDrumHit);
                DrumRigHandler.RightHit(_midiPlayerHandler._rightDrumHit);

            }
            else
            // Guitar Mode
            if (_midiPlayerHandler.InstrumentMode == 3)
            {
                // Switch Rigs
                if (CurrentRig != 3)
                {
                    SetInstrumentRigs(3);
                }

                // Apply guitar position and strum values
                GuitarRigHandler.LeftHandPosition = _midiPlayerHandler._leftHandPosGuitar;
                GuitarRigHandler.Strum(_midiPlayerHandler._strum);
            }
        }
    }

    /// <summary>
    /// Switch rigs and instruments based on a given index
    /// </summary>
    /// <param name="index"></param>
    public void SetInstrumentRigs(int index)
    {
        CurrentRig = index;
        switch(index)
        {
            case 0:
                HandsRig.weight = 1;
                KeyboardRig.weight = 0;
                DrumRig.weight = 0;
                GuitarRig.weight = 0;
                Keyboard.SetActive(false);
                Drum.SetActive(false);
                Guitar.SetActive(false);
                return;
            case 1:
                KeyboardRig.weight = 1;
                DrumRig.weight = 0;
                GuitarRig.weight = 0;
                Keyboard.SetActive(true);
                Drum.SetActive(false);
                Guitar.SetActive(false);
                return;
            case 2:
                KeyboardRig.weight = 0;
                DrumRig.weight = 1;
                GuitarRig.weight = 0;
                Keyboard.SetActive(false);
                Drum.SetActive(true);
                Guitar.SetActive(false);
                return;
            case 3:
                KeyboardRig.weight = 0;
                DrumRig.weight = 0;
                GuitarRig.weight = 1;
                Keyboard.SetActive(false);
                Drum.SetActive(false);
                Guitar.SetActive(true);
                return;
        }
    }

    /// <summary>
    /// Set real handtracking according to a given state
    /// </summary>
    /// <param name="state"></param>
    public void RealHands(bool state)
    {
        if(state)
        {
            HandsRig.weight = 1;
            KeyboardRig.gameObject.SetActive(false);
            DrumRig.gameObject.SetActive(false);
            GuitarRig.gameObject.SetActive(false);

            switch (CurrentRig)
            {
                case 1:
                    KeyboardRig.weight = 0;
                    return;
                case 2:
                    DrumRig.weight = 0;
                    return;
                case 3:
                    GuitarRig.weight = 0;
                    return;
            }
        }
        else
        {
            HandsRig.weight = 0;
            KeyboardRig.gameObject.SetActive(true);
            DrumRig.gameObject.SetActive(true);
            GuitarRig.gameObject.SetActive(true);
            switch (CurrentRig)
            {
                case 1:
                    KeyboardRig.weight = 1;
                    return;
                case 2:
                    DrumRig.weight = 1;
                    return;
                case 3:
                    GuitarRig.weight = 1;
                    return;
            }
        }
    }

    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        /*
        
        // OLD CODE - Do not delete

        if (stream.IsWriting)
        {
            stream.SendNext(KeyboardRigHandler.LeftHandPosition);
            stream.SendNext(KeyboardRigHandler.RightHandPosition);
            stream.SendNext(GuitarRigHandler.LeftHandPosition);
        }
        else
        {
            KeyboardRigHandler.LeftHandPosition = (float)stream.ReceiveNext();
            KeyboardRigHandler.RightHandPosition = (float)stream.ReceiveNext();
            GuitarRigHandler.LeftHandPosition = (float)stream.ReceiveNext();
        }

        */
    }
    
    /*
     RPC SECTION
     Photon Messages to transmit actions across clients.     
     */
    public void SetInstrumentRig(int index)
    {
        photonView.RPC("RPC_SetInstrumentRig", RpcTarget.AllBuffered, index);
    }

    [PunRPC]
    public void RPC_SetInstrumentRig(int index)
    {
        Debug.Log("Changed Rig");
        SetInstrumentRigs(index);
    }

    public void LeftHitKey(bool state)
    {
        photonView.RPC("RPC_LeftHitKey", RpcTarget.AllBuffered, state);
    }

    [PunRPC]
    public void RPC_LeftHitKey(bool state)
    {
        KeyboardRigHandler.LeftHit(state);
    }

    public void RightHitKey(bool state)
    {
        photonView.RPC("RPC_RightHitKey", RpcTarget.AllBuffered, state);

    }

    [PunRPC]
    public void RPC_RightHitKey(bool state)
    {

        KeyboardRigHandler.RightHit(state);
    }

    public void LeftHitDrum(bool state)
    {
        photonView.RPC("RPC_LeftHitDrum", RpcTarget.AllBuffered, state);

    }

    [PunRPC]
    public void RPC_LeftHitDrum(bool state)
    {
        DrumRigHandler.LeftHit(state);
    }

    public void RightHitDrum(bool state)
    {
        photonView.RPC("RPC_RightHitDrum", RpcTarget.AllBuffered, state);

    }

    [PunRPC]
    public void RPC_RightHitDrum(bool state)
    {
        DrumRigHandler.RightHit(state);
    }

    public void Strum(bool state)
    {
        photonView.RPC("RPC_Strum", RpcTarget.AllBuffered, state);

    }

    [PunRPC]
    public void RPC_Strum(bool state)
    {
        GuitarRigHandler.Strum(state);
    }


}
