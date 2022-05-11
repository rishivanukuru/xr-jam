using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.PUN;
using Photon.Voice.Unity;

/// <summary>
/// Script to handle the avatar information display
/// </summary>
public class BasicPlayerInfoDisplay : MonoBehaviourPun
{
    // Player name text display
    [SerializeField]
    [Tooltip("Player name TMP")]
    private TMP_Text _playerNameText;

    // Speaking indicator image
    [SerializeField]
    [Tooltip("Speaking indicator image")]
    private Image _speakingIndicator;

    // Object's photon voice view
    PhotonVoiceView _photonVoiceView;

    void Start()
    {
        // Grab the voice view component
        _photonVoiceView = this.GetComponent<PhotonVoiceView>();

        // Assign the nickname to the name text
        _playerNameText.text = photonView.Owner.NickName;
    }

    void Update()
    {
        // Display speaking indicator if the player is speaking
        if (_photonVoiceView.IsSpeaking)
        {
            _speakingIndicator.enabled = true;
        }
        else
        {
            _speakingIndicator.enabled = false;
        }
    }
}
