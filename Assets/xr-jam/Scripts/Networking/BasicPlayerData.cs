using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Script to store basic player information
/// </summary>
public class BasicPlayerData : MonoBehaviour
{
    // Input field from which to derive the player name
    [SerializeField]
    [Tooltip("Input field from which to derive the player name")]
    public TMP_InputField _playerNameInput;

    // Join room button
    [SerializeField]
    [Tooltip("Join room button")]
    public Button _joinRoomButton;

    // The local player's name
    [Tooltip("The local player's name")]
    public string PlayerName;

    void Start()
    {
        // Add the join room function to the join room button
        _joinRoomButton.onClick.AddListener(JoinRoom);
    }

    void Update()
    {
        // Only allow a player to join the room with a non-empty name
        if (_playerNameInput.text == "")
        {
            _joinRoomButton.interactable = false;
        }
        else
        {
            _joinRoomButton.interactable = true;
        }
    }

    /// <summary>
    /// Joins room by setting the player name to be what is provided in the input field.
    /// </summary>
    public void JoinRoom()
    {
        PlayerName = _playerNameInput.text;
        BasicNetworkingManager.Instance.SetPlayerName(PlayerName);
        BasicNetworkingManager.Instance.AttemptCreateOrJoin();
    }

}
