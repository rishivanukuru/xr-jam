using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

/// <summary>
/// Script to handle Keyboard rigged animation
/// </summary>
public class KeyboardRigHandler : MonoBehaviour
{
    [Header("Keyboard Rig")]
    // Keyboard Rig
    [Tooltip("Keyboard Rig Object")]
    public Rig KeyboardRig;

    // Left hand target
    [SerializeField]
    [Tooltip("Left Hand Target")]
    private GameObject _leftHandIK;
    
    // Right hand target
    [SerializeField]
    [Tooltip("Right Hand Target")]
    private GameObject _rightHandIK;

    // Hand Spread Extent
    [Tooltip("Hand Spread Extent")]
    public float HandSpread;

    // Hand positions
    [Header("Hand Positions")]

    [Tooltip("Left Hand Position")]
    [Range(0f, 1f)]
    public float LeftHandPosition = 0f;

    [Tooltip("Left Hand Position")]
    [Range(0f, 1f)]
    public float RightHandPosition = 0f;

    [Header("Movement")]

    // Movement speed
    [SerializeField]
    [Tooltip("Hand Travel Movement Speed")]
    private float _movementSpeed = 0.5f;

    [Header("Hand Rotation")]

    //Key press hand rotation
    [SerializeField]
    [Tooltip("Key press hand rotation")]
    private float _angle;

    // Keyboard Animation Holder Variables
    private Vector3 _leftHandStartPos;
    private Vector3 _rightHandStartPos;
    private Quaternion _leftStartRot;
    private Quaternion _leftEndRot;
    private Quaternion _rightStartRot;
    private Quaternion _rightEndRot;
    private Vector3 _rightVelocity = Vector3.zero;
    private Vector3 _leftVelocity = Vector3.zero;
    Vector3 _leftTarget;
    Vector3 _rightTarget;
    Vector3 _displacement;

    // Start is called before the first frame update
    void Start()
    {
        // Initializing start positions and rotations of all targets and rig elements
        _leftHandStartPos = _leftHandIK.transform.localPosition;
        _rightHandStartPos = _rightHandIK.transform.localPosition;
        _displacement = Vector3.Normalize(_rightHandStartPos - _leftHandStartPos);

        _leftStartRot = _leftHandIK.transform.localRotation;
        _rightStartRot = _rightHandIK.transform.localRotation;

        _leftEndRot = _leftStartRot * Quaternion.Euler(0,0,-_angle);
        _rightEndRot = _rightStartRot * Quaternion.Euler(0,0,-_angle);
    }

    void Update()
    {
        if (KeyboardRig.weight == 1)
        {
            // Calculate and apply hand motion in a smooth manner
            _leftTarget = _leftHandStartPos - _displacement * HandSpread * LeftHandPosition;
            _rightTarget = _rightHandStartPos + _displacement * HandSpread * RightHandPosition;

            _leftHandIK.transform.localPosition = Vector3.SmoothDamp(_leftHandIK.transform.localPosition, _leftTarget, ref _leftVelocity, _movementSpeed);
            _rightHandIK.transform.localPosition = Vector3.SmoothDamp(_rightHandIK.transform.localPosition, _rightTarget, ref _rightVelocity, _movementSpeed);

            /*
            // DEBUG KEY CODES FOR ANIMATION
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
            */
        }


    }

    /// <summary>
    /// Handles right hand key press state
    /// </summary>
    /// <param name="state"></param>
    public void RightHit(bool state)
    {
       _rightHandIK.transform.localRotation = state ? _rightEndRot : _rightStartRot;
    }

    /// <summary>
    /// Handles left hand key press state
    /// </summary>
    public void LeftHit(bool state)
    {
       _leftHandIK.transform.localRotation = state ? _leftEndRot : _leftStartRot;
    }


}
