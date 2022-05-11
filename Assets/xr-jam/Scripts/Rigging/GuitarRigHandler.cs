using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

/// <summary>
/// Script to handle guitar animation
/// </summary>
public class GuitarRigHandler : MonoBehaviour
{
    // The guitar rig
    [Header("Guitar Rig")]
    [Tooltip("Guitar Rig")]
    public Rig GuitarRig;
    
    // Left hand IK target
    [SerializeField]
    [Tooltip("Left Hand IK Target")]
    private GameObject _leftHandIK;
    
    // Left hand end position
    [SerializeField]
    [Tooltip("Left Hand end position")]
    private GameObject _leftHandEnd;

    // Right Hand IK Target
    [SerializeField]
    [Tooltip("Right Hand IK Target")]
    private GameObject _rightHandIK;

    // Hand position
    [Header("Hand Positions")]
    [Tooltip("Position of left hand along neck")]
    [Range(0f, 1f)]
    public float LeftHandPosition = 0f;

    // Hand movement speed
    [Header("Movement")]
    [Tooltip("Hand movement speed")]
    [SerializeField]
    private float _movementSpeed = 0.5f;

    // Strum movement angle
    [Header("Hand Rotation")]
    [SerializeField]
    [Tooltip("Strum angle")]
    private float _angle;

    // Guitar animation holder variables
    private Vector3 _leftHandStartPos;
    private Quaternion _rightStartRot;
    private Quaternion _rightEndRot;
    private Vector3 _leftVelocity = Vector3.zero;
    private Vector3 _leftTarget;
    private Vector3 _displacement;


    void Start()
    {
        // Initialize baseline positions and rotations for all targets and hands.
        _leftHandStartPos = _leftHandIK.transform.localPosition;
        _displacement = _leftHandEnd.transform.localPosition - _leftHandStartPos;

        _rightStartRot = _rightHandIK.transform.localRotation;
        _rightEndRot = _rightStartRot * Quaternion.Euler(0, _angle,0);

    }

    void Update()
    {
        if (GuitarRig.weight == 1)
        {
            // Smoothly move the left hand based on the target position (on a scale of 0 to 1)
            _leftTarget = _leftHandStartPos + _displacement * LeftHandPosition;
            _leftHandIK.transform.localPosition = Vector3.SmoothDamp(_leftHandIK.transform.localPosition, _leftTarget, ref _leftVelocity, _movementSpeed);

            /*
            //DEBUG ANIMATION
            if (Input.GetKeyDown(KeyCode.R))
            {
                Strum(true);
            }

            if (Input.GetKeyUp(KeyCode.R))
            {
                Strum(false);
            }
            */
        }
    }

    /// <summary>
    /// Method to move the right hand up or down to simulate a guitar strum
    /// </summary>
    /// <param name="state"></param>
    public void Strum(bool state)
    {
        _rightHandIK.transform.localRotation = state ? _rightEndRot : _rightStartRot;
    }

}
