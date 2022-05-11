using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

/// <summary>
/// Script to handle the drum animation
/// </summary>
public class DrumRigHandler : MonoBehaviour
{
    // Drum Rig
    [Header("Drum Rig")]
    [Tooltip("The Drum Rig")]
    public Rig DrumRig;

    // Hand IK controllers/targets
    [SerializeField]
    [Tooltip("Left Target")]
    private GameObject _leftHandIK;
    [SerializeField]
    [Tooltip("Right Target")]
    private GameObject _rightHandIK;

    // Holder variables for the start position of each target.
    private Vector3 _leftHandStartPos;
    private Vector3 _rightHandStartPos;

    // Distance for hand to move down during each hit.
    [Header("Hit Depth")]
    [Tooltip("Distance for hand to move down during each hit.")]
    public float HitDepth;

    void Start()
    {
        // Assign and store the start positions of the targets
        _leftHandStartPos = _leftHandIK.transform.localPosition;
        _rightHandStartPos = _rightHandIK.transform.localPosition;
    }

    void Update()
    {
        /*
        // DEBUG CODE
        // For debugging hit animation
        if(DrumRig.weight == 1)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                RightHit(true);
            }

            if (Input.GetKeyUp(KeyCode.R))
            {
                RightHit(false);
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                LeftHit(true);
            }

            if (Input.GetKeyUp(KeyCode.T))
            {
                LeftHit(false);
            }
        }
        */
    }

    /// <summary>
    /// Animates the Right Hand
    /// </summary>
    /// <param name="state"></param>
    public void RightHit(bool state)
    {
        _rightHandIK.transform.localPosition = _rightHandStartPos + ((state)? Vector3.up * -1 * HitDepth : Vector3.zero);
    }

    /// <summary>
    /// Animates the left hand
    /// </summary>
    /// <param name="state"></param>
    public void LeftHit(bool state)
    {
        _leftHandIK.transform.localPosition = _leftHandStartPos + ((state) ? Vector3.up * -1 * HitDepth : Vector3.zero);
    }

}
