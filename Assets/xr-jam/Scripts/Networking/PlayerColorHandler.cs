using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// Script that applies the broadcast color to the material on a given object
/// </summary>
public class PlayerColorHandler : MonoBehaviour
{
    private Material _myMaterial;

    public void SetPlayerColor(Color myRGB)
    {
        if (_myMaterial is null)
            _myMaterial = GetComponent<Renderer>().material;
        _myMaterial.color = myRGB;
    }
}