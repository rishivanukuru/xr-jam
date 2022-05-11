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
/// Script to control the Photon Voice Network Transmission
/// </summary>
public class MuteButtonHandler : MonoBehaviour
{
    // Visual action text on the button
    [SerializeField]
    [Tooltip("Mute Button TMP")]
    private TMP_Text _buttonText;

    // Button element
    [SerializeField]
    [Tooltip("Mute Button")]
    private Button _muteButton;

    void Start()
    {
        // Set the initial state, and attach the toggle function to the Mute button
        _buttonText.text = "Mute";
        _muteButton.onClick.AddListener(ToggleMute);
    }

    /// <summary>
    /// Check the current status of the voice network, and toggle mute accordingly, while also changing the button text
    /// </summary>
    public void ToggleMute()
    {
        if(BasicNetworkingManager.Instance.LocalPlayer.GetComponent<PhotonVoiceView>().RecorderInUse.TransmitEnabled)
        {
            BasicNetworkingManager.Instance.LocalPlayer.GetComponent<PhotonVoiceView>().RecorderInUse.TransmitEnabled = false;
            _buttonText.text = "Speak";
        }
        else
        {
            BasicNetworkingManager.Instance.LocalPlayer.GetComponent<PhotonVoiceView>().RecorderInUse.TransmitEnabled = true;
            _buttonText.text = "Mute";
        }
    }

}
