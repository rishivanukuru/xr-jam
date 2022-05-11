using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Animations.Rigging;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Script to display profile picture as a player face
/// </summary>
public class PlayerFaceHandler : MonoBehaviourPun
{
    // List of Face Images in the avatar that need to be applied
    [SerializeField]
    [Tooltip("List of Face Images in the avatar that need to be applied")]
    private Image[] _faceImages;

    // Transparent sprite (default)
    [SerializeField]
    [Tooltip("A default transparent sprite")]
    private Sprite _blankSprite;

    // Names of players to cross-reference
    [SerializeField]
    [Tooltip("List of player names who have images")]
    private string[] _peopleNames;

    // Corresponding images to names
    [SerializeField]
    [Tooltip("List of sprites corresponding to each name in the previous list")]
    private Sprite[] _peopleImages;

    // Photon view on this object
    private PhotonView _myPhotonView;

    // Start is called before the first frame update
    void Start()
    {
        // Grab this object's Photon View
        _myPhotonView = GetComponent<PhotonView>();

        // Cycle through all images
        foreach (Image faceImage in _faceImages)
        {
            // Set the face sprite to be blank by default
            faceImage.sprite = _blankSprite;
            //faceImage.enabled = false;

            // If the player name matches one in the list, apply the corresponding player image
            for (int i = 0; i < _peopleImages.Length; i++)
            {
                if (photonView.Owner.NickName.ToLower() == _peopleNames[i].ToString().ToLower() && !photonView.IsMine)
                {
                    faceImage.sprite = _peopleImages[i];
                }
            }

        }

    }

    /// <summary>
    /// Function to toggle between showing or hiding avatar faces
    /// (not currently in use)
    /// </summary>
    /// <param name="isDisplay"></param>
    public void DisplayAvatarFace(bool isDisplay)
    {
        if (isDisplay)
        {
            foreach (Image faceImage in _faceImages)
            {
                faceImage.enabled = true;
            }

        }
        else
        {
            foreach (Image faceImage in _faceImages)
            {
                faceImage.enabled = false;
            }

        }
    }
}
