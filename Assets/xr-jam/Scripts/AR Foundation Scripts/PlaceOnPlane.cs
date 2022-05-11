using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation.Samples
{
    /// <summary>
    /// Listens for touch events and performs an AR raycast from the screen mid point.
    /// AR raycasts will only hit detected trackables like feature points and planes.
    ///
    /// If a raycast hits a trackable, the room is placed.
    /// and moved to the hit position.
    /// </summary>
    [RequireComponent(typeof(ARRaycastManager))]
    public class PlaceOnPlane : MonoBehaviour
    {
        // Room object (central table + UI)
        [SerializeField]
        [Tooltip("Room to be placed at the screen midpoint.")]
        GameObject RoomHolder;

        // AR Session Object
        ARRaycastManager m_RaycastManager;

        // List of raycast hits
        static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

        void Awake()
        {
            // Grabs the Raycast Manager from the gameobject
            m_RaycastManager = GetComponent<ARRaycastManager>();

            // Deactivates the Room (mobile AR, show room only when placed)
            RoomHolder.SetActive(false);
        }


        /// <summary>
        /// Method to activate and place the Room objects on an AR Plane.
        /// Called when the on-screen "Place" button is pressed.
        /// </summary>
        public void PlaceObject()
        {
            // Calculate the screen's midpoint.
            Vector2 _placementPosition = new Vector2(Screen.width/2, Screen.height/2);

            // Start a raycast from the midpoint
            if (m_RaycastManager.Raycast(_placementPosition, s_Hits, TrackableType.PlaneWithinPolygon))
            {
                // Raycast hits are sorted by distance, so the first one
                // will be the closest hit.
                var hitPose = s_Hits[0].pose;

                // If a raycast intersects with an AR Plane, move the Room to the position of the hit.
                RoomHolder.transform.position = hitPose.position;
                
                // If it's the first time placing the room, activate it so that it can be seen.
                if(!RoomHolder.activeSelf)
                {
                    RoomHolder.SetActive(true);
                }
            }
        }


        /// <summary>
        /// Method to get the current screen Touch position
        /// </summary>
        /// <param name="touchPosition"></param>
        /// <returns></returns>
        bool TryGetTouchPosition(out Vector2 touchPosition)
        {
            if (Input.touchCount > 0)
            {
                touchPosition = Input.GetTouch(0).position;
                return true;
            }

            touchPosition = default;
            return false;
        }

    }


}