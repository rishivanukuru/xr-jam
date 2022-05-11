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
/// Script to trigger the Haptic Mat based on the position of a given local and remote player.
/// To be run in MIDI mode in the editor.
/// </summary>
public class BasicHapticManager : MonoBehaviour
{
    // The Ardity Serial Controller
    [SerializeField]
    [Tooltip("The In-Scene Ardity Serial Controller")]
    private SerialController _serialController;

    // Flag to determine if haptic messages should be sent to the mat
    [SerializeField]
    [Tooltip("Flag to determine if haptic messages should be sent to the mat")]
    private bool _shouldHaptics = false;

    // Flag to know if the script is running in a room where the Photon Voice Network is Active
    [SerializeField]
    [Tooltip("Flag to know if the script is running in a room where the Photon Voice Network is Active")]
    private bool _isPhotonActive = false;

    // The player who is supposed to be using the mat.
    [SerializeField]
    [Tooltip("The player using the mat")]
    private GameObject _localPlayer;

    // The player whose motion is being tracked and conveyed via the mat
    [SerializeField]
    [Tooltip("The player whose motion is being tracked and conveyed via the mat")]
    private GameObject _trackedPlayer;

    // Holder variable for the vector from the tracked player to the mat player
    Vector3 _displacement;
    
    // Holder variable for the forward direction of the mat player (always facing the room reference object)
    Vector3 _forwardProjection;

    // Angle between displacement and forward projection
    float _forwardAngle;

    // Holder variables for haptic motor calculations
    int _motorInt;
    float _motorFloat;
    int _motorVal;

    private void Update()
    {
        // Toggle haptic behaviour on pressing the key M
        if (Input.GetKeyDown(KeyCode.M))
        {
            _shouldHaptics = !_shouldHaptics;
        }

        // Only transmit haptics if both local and tracked players have been assigned
        if(_localPlayer!=null && _trackedPlayer!=null)
        {
            // If we are in photon mode, and the tracked player is not speaking, no haptics are transmitted
            if(_isPhotonActive && _trackedPlayer.GetComponent<PhotonVoiceView>().IsSpeaking == false)
            {
                return;
            }

            // Calculate displacement
            _displacement = _trackedPlayer.transform.position - _localPlayer.transform.position;

            // Project onto the XZ plane
            _displacement = Vector3.ProjectOnPlane(_displacement, Vector3.up);

            // Calculate forward vector
            _forwardProjection = BasicNetworkingManager.Instance.ReferenceObject.transform.position - _localPlayer.transform.position;

            // Project onto the XZ plane
            _forwardProjection = Vector3.ProjectOnPlane(_forwardProjection, Vector3.up);

            // Calculate angle between Displacement and Forward
            _forwardAngle = Vector3.SignedAngle(_displacement, _forwardProjection, Vector3.up);

            // Convert angle from (-180, 180) to (0, 360)
            if(_forwardAngle<0)
            {
                _forwardAngle = _forwardAngle * -1;
            }
            else
            {
                _forwardAngle = 360 - _forwardAngle;
            }

            // Dividing by 18 to get 20 equal segments corresponding to the 20 motors
            _motorFloat = _forwardAngle / 18;

            // Floor the value
            _motorInt = Mathf.FloorToInt(_motorFloat);
            
            // When between two motors, choose to actuate the one it is closer to
            _motorVal = _motorInt + ((_motorFloat - _motorInt) > 0.5 ? 1 : 0);

            // Offset for current mat
            _motorVal += 3;

            // Wrap-around control
            if (_motorVal >= 20)
            {
                _motorVal -= 20;
            }

            // Send message if in haptics mode
            if (_shouldHaptics)
            {
                _serialController.SendSerialMessage(_motorVal.ToString() + ',');
            }
        }
    }


}
