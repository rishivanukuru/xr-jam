using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script to rotate the room's center - so as to reposition the other players.
/// </summary>
public class ARRoomRotationHandler : MonoBehaviour
{
    // Speed at which the rotation takes place on button press
    [SerializeField]
    [Tooltip("Speed at which the rotation takes place on button press")]
    private float _rotationSpeed = 20f;

    // Flages to hold whether the center is rotating in one direction or the other.
    private bool _isRotatingClockwise;
    private bool _isRotatingAnticlockwise;

    void Update()
    {

        // Rotate the center based on the state of the directional flags.
        if (_isRotatingClockwise)
        {
            this.transform.RotateAround(this.transform.position, Vector3.up, _rotationSpeed * Time.deltaTime);
        }
        else
        if (_isRotatingAnticlockwise)
        {
            this.transform.RotateAround(this.transform.position, Vector3.up, -_rotationSpeed * Time.deltaTime);
        }

    }

    /// <summary>
    /// Attached to the OnPointerDown and OnPointerUp methods, to initiate rotation in the clockwise direction
    /// </summary>
    /// <param name="state"></param>
    public void RotateClockwise(bool state)
    {
        _isRotatingClockwise = state;
    }

    /// <summary>
    /// Attached to the OnPointerDown and OnPointerUp methods, to initiate rotation in the anticlockwise direction
    /// </summary>
    /// <param name="state"></param>
    public void RotateAnticlockwise(bool state)
    {
        _isRotatingAnticlockwise = state;
    }

}
