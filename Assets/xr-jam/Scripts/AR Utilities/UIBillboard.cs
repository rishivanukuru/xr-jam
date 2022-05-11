using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rotates world-space UI elements to face the viewer (main camera)
/// </summary>
public class UIBillboard : MonoBehaviour
{
    // Enum holding the two types of rotations
    public enum RotationType
    {
        YAxis,
        AllAxes
    }

    [SerializeField]
    [Tooltip("The current rotation behavior - clamped along Y, or always parallel")]
    private RotationType _rotationType;

    [SerializeField]
    [Tooltip("Y Angle Offset for in-scene adjustments - often 0 or 180")]
    private float yOffset = 0;

    // Holder variable for the vector from the viewer to the UI element
    private Vector3 _lookVector;

    void Update()
    {
        // Calculate the look vector
        // The camera to consider is different based on whether the app is in Nreal mode or Phone mode
        if (BasicNetworkingManager.Instance.XRJamMode == BasicNetworkingManager.XRJamModes.Phone)
        {
            _lookVector = BasicNetworkingManager.Instance.PhoneCamera.transform.position - transform.position;
        }
        else
        {
            _lookVector = BasicNetworkingManager.Instance.NrealCamera.transform.position - transform.position;
        }


        // In case of Y axis, project the look vector on the XZ plane
        if (_rotationType == RotationType.YAxis)
        {
            _lookVector.y = 0;
        }

        // If the UI element coincides with the camera, do not compute
        if (_lookVector == Vector3.zero) 
            return;

        // Assign rotation
        transform.rotation = Quaternion.LookRotation(_lookVector);
        
        // Clamp rotation in case of Y axis
        transform.Rotate(Vector3.up, yOffset);
    }
}
