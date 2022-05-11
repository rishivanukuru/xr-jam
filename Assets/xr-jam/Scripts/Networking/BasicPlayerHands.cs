using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.PUN;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System;
using Photon.Voice.Unity;
using UnityEngine.Animations.Rigging;

/// <summary>
/// Script to control Nreal Hand movement transmission over Photon
/// </summary>
public class BasicPlayerHands : MonoBehaviourPunCallbacks, IPunObservable
{
    // Left hand target
    [SerializeField]
    [Tooltip("Left hand target")]
    private GameObject _nrealLeftHandIK;

    // Right hand target
    [SerializeField]
    [Tooltip("Right hand target")]
    private GameObject _nrealRightHandIK;

    // Left Hand Parent (Nreal Hands)
    [SerializeField]
    [Tooltip("Left Hand Parent (Nreal Hands)")]
    private GameObject _nrealLeftHandParent;

    // Right Hand Parent (Nreal Hands)
    [SerializeField]
    [Tooltip("Right Hand Parent (Nreal Hands)")]
    private GameObject _nrealRightHandParent;

    // Overall Instrument Rig Manager
    private InstrumentRigManager _rigManager;

    // Transform holders for the hand rest positions
    private Transform _rightRest;
    private Transform _leftRest;

    // Sphere gameobject lists to hold and display the joints of the Nreal hands
    private GameObject[] _rightHandPoints;
    private GameObject[] _leftHandPoints;

    private void Awake()
    {
        // Grab Instrument Rig Manager Component
        _rigManager = GetComponent<InstrumentRigManager>();
    }

    void Start()
    {
        // Assign start transforms
        _rightRest = _nrealRightHandIK.transform;
        _leftRest = _nrealLeftHandIK.transform;

        // Generate spheres for the right and left hands

        _rightHandPoints = new GameObject[21];

        for(int i = 0; i<21; i++)
        {
            _rightHandPoints[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _rightHandPoints[i].transform.parent = _nrealRightHandParent.transform.parent;
            _rightHandPoints[i].transform.localScale = 0.016f * Vector3.one;
            _rightHandPoints[i].GetComponent<Renderer>().material.color = Color.black;
            _rightHandPoints[i].SetActive(false);
        }

        _leftHandPoints = new GameObject[21];

        for (int i = 0; i < 21; i++)
        {
            _leftHandPoints[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _leftHandPoints[i].transform.parent = _nrealLeftHandParent.transform.parent;
            _leftHandPoints[i].transform.localScale = 0.016f * Vector3.one;
            _leftHandPoints[i].GetComponent<Renderer>().material.color = Color.black;
            _leftHandPoints[i].SetActive(false);
        }
    }

    void Update()
    {
        if(photonView.IsMine)
        {
            // If the local player is an Nreal
            if (BasicNetworkingManager.Instance.XRJamMode == BasicNetworkingManager.XRJamModes.Nreal)
            {
                // The 22nd child of the parent represents the wrists
                Transform _rightWrist = BasicNetworkingManager.Instance.RightHandParent.transform.GetChild(22);
                Transform _leftWrist = BasicNetworkingManager.Instance.LeftHandParent.transform.GetChild(22);

                // Calculating the forward vector of the hands to generate the correct rotation
                _rightWrist.position = BasicNetworkingManager.Instance.RightHandParent.transform.GetChild(22).position;
                Vector3 _rightWristForward = 100f*BasicNetworkingManager.Instance.RightHandParent.transform.GetChild(23).position - 100f*BasicNetworkingManager.Instance.RightHandParent.transform.GetChild(22).position;
                _rightWrist.rotation = Quaternion.LookRotation(_rightWristForward, _nrealRightHandParent.transform.up);

                // Calculating the forward vector of the hands to generate the correct rotation
                _leftWrist.position = BasicNetworkingManager.Instance.LeftHandParent.transform.GetChild(22).position;
                Vector3 _leftWristForward = 100f*BasicNetworkingManager.Instance.LeftHandParent.transform.GetChild(23).position - 100f*BasicNetworkingManager.Instance.LeftHandParent.transform.GetChild(22).position;
                _rightWrist.rotation = Quaternion.LookRotation(_leftWristForward, _nrealLeftHandParent.transform.up);

                // If any one of the hands is active
                if (_rightWrist.gameObject.activeSelf || _leftWrist.gameObject.activeSelf)
                {
                    // Activate the real hand rig
                    _rigManager.RealHands(true);

                    // Transform the IK targets accordingly
                    // If Nreal hand is visible, deactivate avatar fingers and palm, or else, activate them
                    if (_rightWrist.gameObject.activeSelf)
                    {
                        _nrealRightHandParent.transform.localScale = Vector3.zero;
                        _nrealRightHandIK.transform.position = _rightWrist.position;
                        _nrealRightHandIK.transform.rotation = _rightWrist.rotation;
                    }
                    else
                    {
                        _nrealRightHandParent.transform.localScale = Vector3.one;
                        _nrealRightHandIK.transform.position = _rightRest.position;
                        _nrealRightHandIK.transform.rotation = _rightRest.rotation;

                    }

                    // Transform the IK targets accordingly
                    // If Nreal hand is visible, deactivate avatar fingers and palm, or else, activate them
                    if (_leftWrist.gameObject.activeSelf)
                    {
                        _nrealLeftHandParent.transform.localScale = Vector3.zero;
                        _nrealLeftHandIK.transform.position = _leftWrist.position;
                        _nrealLeftHandIK.transform.rotation = _leftWrist.rotation;
                    }
                    else
                    {
                        _nrealLeftHandParent.transform.localScale = Vector3.one;
                        _nrealLeftHandIK.transform.position = _leftRest.position;
                        _nrealLeftHandIK.transform.rotation = _leftRest.rotation;

                    }
                }
                else
                {
                    // If no Nreal hands are detected, return to standard position and release rig control

                    _nrealLeftHandIK.transform.position = _leftRest.position;
                    _nrealRightHandIK.transform.position = _rightRest.position;

                    _nrealLeftHandIK.transform.rotation = _leftRest.rotation;
                    _nrealRightHandIK.transform.rotation = _rightRest.rotation;

                    _nrealRightHandParent.transform.localScale = Vector3.one;
                    _nrealLeftHandParent.transform.localScale = Vector3.one;

                    _rigManager.RealHands(false);



                }
            }
        }

        
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // In phone mode, send the no hand flag
            if(BasicNetworkingManager.Instance.XRJamMode == BasicNetworkingManager.XRJamModes.Phone)
            {
                stream.SendNext(false);
            }
            else
            {
                //
                // Similarly calculate the appropriate hand rotations
                //

                Transform _rightWrist = BasicNetworkingManager.Instance.RightHandParent.transform.GetChild(22);
                Transform _leftWrist = BasicNetworkingManager.Instance.LeftHandParent.transform.GetChild(22);


                _rightWrist.position = BasicNetworkingManager.Instance.RightHandParent.transform.GetChild(22).position;
                Vector3 _rightWristForward = BasicNetworkingManager.Instance.RightHandParent.transform.GetChild(23).position - BasicNetworkingManager.Instance.RightHandParent.transform.GetChild(22).position;
                _rightWrist.rotation = Quaternion.LookRotation(_rightWristForward, _nrealRightHandParent.transform.up);

                _leftWrist.position = BasicNetworkingManager.Instance.LeftHandParent.transform.GetChild(22).position;
                Vector3 _leftWristForward = BasicNetworkingManager.Instance.LeftHandParent.transform.GetChild(23).position - BasicNetworkingManager.Instance.LeftHandParent.transform.GetChild(22).position;
                _rightWrist.rotation = Quaternion.LookRotation(_leftWristForward, _nrealLeftHandParent.transform.up);


                // Send stream values based on which hands are active
                if (_rightWrist.gameObject.activeSelf || _leftWrist.gameObject.activeSelf)
                {
                    stream.SendNext(true);

                    if (_rightWrist.gameObject.activeSelf)
                    {
                        // Send hand transform
                        stream.SendNext(true);
                        stream.SendNext(BasicNetworkingManager.Instance.ReferenceObject.InverseTransformPoint(_rightWrist.position));
                        stream.SendNext(Quaternion.Inverse(BasicNetworkingManager.Instance.ReferenceObject.rotation) * _rightWrist.rotation);

                        // Send joint transforms
                        for (int i = 0; i < 21; i++)
                        {
                            stream.SendNext(BasicNetworkingManager.Instance.ReferenceObject.InverseTransformPoint(BasicNetworkingManager.Instance.RightHandParent.transform.GetChild(24 + i).position));
                        }

                    }
                    else
                    {
                        stream.SendNext(false);
                    }

                    if (_leftWrist.gameObject.activeSelf)
                    {
                        // Send hand transform
                        stream.SendNext(true);
                        stream.SendNext(BasicNetworkingManager.Instance.ReferenceObject.InverseTransformPoint(_leftWrist.position));
                        stream.SendNext(Quaternion.Inverse(BasicNetworkingManager.Instance.ReferenceObject.rotation) * _leftWrist.rotation);

                        // Send joint transforms
                        for (int i = 0; i < 21; i++)
                        {
                            stream.SendNext(BasicNetworkingManager.Instance.ReferenceObject.InverseTransformPoint(BasicNetworkingManager.Instance.LeftHandParent.transform.GetChild(24 + i).position));
                        }
                    }
                    else
                    {
                        stream.SendNext(false);
                    }
                }
                else
                {
                    stream.SendNext(false);
                }

            }




        }
        else
        {

                // If hand data is being transmitted
                if ((bool)stream.ReceiveNext())
                {
                    if(_rigManager!=null)
                    _rigManager.RealHands(true);

                    // Assign the hand position, and move the joint spheres to the right locations
                    if ((bool)stream.ReceiveNext())
                    {
                        _nrealRightHandParent.transform.localScale = Vector3.zero;
                        _nrealRightHandIK.transform.position = BasicNetworkingManager.Instance.ReferenceObject.TransformPoint((Vector3)stream.ReceiveNext());
                        _nrealRightHandIK.transform.rotation = BasicNetworkingManager.Instance.ReferenceObject.rotation * (Quaternion)stream.ReceiveNext();
                        for (int i = 0; i < 21; i++)
                        {
                            _rightHandPoints[i].SetActive(true);
                            _rightHandPoints[i].transform.position = BasicNetworkingManager.Instance.ReferenceObject.TransformPoint((Vector3)stream.ReceiveNext());
                        }
                    }
                    else
                    {
                        _nrealRightHandParent.transform.localScale = Vector3.one;
                        _nrealRightHandIK.transform.position = _rightRest.position;
                        _nrealRightHandIK.transform.rotation = _rightRest.rotation;
                        for (int i = 0; i < 21; i++)
                        {
                            _rightHandPoints[i].SetActive(false);
                        }
                    }

                    if ((bool)stream.ReceiveNext())
                    {
                        _nrealLeftHandParent.transform.localScale = Vector3.zero;
                        _nrealLeftHandIK.transform.position = BasicNetworkingManager.Instance.ReferenceObject.TransformPoint((Vector3)stream.ReceiveNext());
                        _nrealLeftHandIK.transform.rotation = BasicNetworkingManager.Instance.ReferenceObject.rotation * (Quaternion)stream.ReceiveNext();
                        for (int i = 0; i < 21; i++)
                        {
                            _leftHandPoints[i].SetActive(true);
                            _leftHandPoints[i].transform.position = BasicNetworkingManager.Instance.ReferenceObject.TransformPoint((Vector3)stream.ReceiveNext());
                        }
                    }
                    else
                    {
                        _nrealLeftHandIK.transform.position = _leftRest.position;
                        _nrealLeftHandIK.transform.rotation = _leftRest.rotation;
                        _nrealLeftHandParent.transform.localScale = Vector3.one;
                        for (int i = 0; i < 21; i++)
                        {
                            _leftHandPoints[i].SetActive(false);
                        }
                    }
                }
                else
                {
                    // Deactivate active hand positions and return to default state

                    if(_nrealLeftHandIK && _nrealRightHandIK && _leftRest && _rightRest)
                    {
                        _nrealLeftHandIK.transform.position = _leftRest.position;
                        _nrealRightHandIK.transform.position = _rightRest.position;

                        _nrealLeftHandIK.transform.rotation = _leftRest.rotation;
                        _nrealRightHandIK.transform.rotation = _rightRest.rotation;


                        _nrealRightHandParent.transform.localScale = Vector3.one;
                        _nrealLeftHandParent.transform.localScale = Vector3.one;

                        for (int i = 0; i < 21; i++)
                        {
                            _rightHandPoints[i].SetActive(false);
                            _leftHandPoints[i].SetActive(false);
                        }
                    }


                    if (_rigManager!=null)
                        _rigManager.RealHands(false);

                }
        }
    }
}
