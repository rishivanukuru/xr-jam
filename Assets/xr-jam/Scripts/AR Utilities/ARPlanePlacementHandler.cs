using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NRKernal;

/// <summary>
/// Script to place a given object on an Nreal tracked plane.
/// Also toggles between placement active and deactive states.
/// </summary>
public class ARPlanePlacementHandler : MonoBehaviour
{
    //A model to place when a raycast from a user touch hits a plane.
    [Tooltip("The object holding the room's center.")]
    public GameObject PlacedObject;

    // Transform of the Nreal Controller
    [Tooltip("Transform of the Nreal Controller")]
    public Transform ControllerLaser;

    // Transform of the Nreal Right Hand
    [Tooltip("Transform of the Nreal Right Hand")]
    public Transform HandLaser;

    // Flag to know if planes should be displayed, and process placement events
    private bool _arePlanesActive = true;

    // Array of all AR planes in a given update
    private GameObject[] _planes;

    void Update()
    {
        // If the player doesn't click the trigger button, we are done with this update.
        if (!NRInput.GetButtonDown(ControllerButton.TRIGGER))
        {
            return;
        }

        // If we are supposed to be operating on planes
        if(_arePlanesActive)
        {
            /*
            // OLD CODE
            //Get controller laser origin.
            var handControllerAnchor = NRInput.DomainHand == ControllerHandEnum.Left ? ControllerAnchorEnum.LeftLaserAnchor : ControllerAnchorEnum.RightLaserAnchor;
            Transform laserAnchor = NRInput.AnchorsHelper.GetAnchor(NRInput.RaycastMode == RaycastModeEnum.Gaze ? ControllerAnchorEnum.GazePoseTrackerAnchor : handControllerAnchor);
            */

            // Gets the point from which the raycast should take place - Controller or Hand
            Transform laserAnchor = NRInput.CurrentInputSourceType == InputSourceEnum.Controller ? ControllerLaser : HandLaser;

            // Variable to store the hit
            RaycastHit hitResult;


            // If the raycast intersects an object on layer 10 - Nreal AR Plane
            if (Physics.Raycast(new Ray(laserAnchor.transform.position, laserAnchor.transform.forward), out hitResult, 10))
            {
                if (hitResult.collider.gameObject != null && hitResult.collider.gameObject.GetComponent<NRTrackableBehaviour>() != null)
                {
                    var behaviour = hitResult.collider.gameObject.GetComponent<NRTrackableBehaviour>();
                    if (behaviour.Trackable.GetTrackableType() != TrackableType.TRACKABLE_PLANE)
                    {
                        return;
                    }

                    // Place the room at the new location
                    PlacedObject.transform.position = hitResult.point;
                }
            }
        }

    }

    /// <summary>
    /// Switching between active and deactive modes for Plane Placement
    /// </summary>
    public void TogglePlanes()
    {
        // Toggle flag state
        _arePlanesActive = !_arePlanesActive;
        
        // Find all planes currently in the scene
        _planes = GameObject.FindGameObjectsWithTag("arplane");
        
        // Toggle the state (visual and colliders) of all current planes based on the new flag
        foreach(GameObject currentPlane in _planes)
        {
            currentPlane.GetComponent<Renderer>().enabled = _arePlanesActive;
            currentPlane.GetComponent<MeshCollider>().enabled = _arePlanesActive;
        }
    }
}
