using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple script to destroy the editor room environment when on an Android device.
/// </summary>
public class EditorRoomHandler : MonoBehaviour
{
    void Start()
    {
#if !UNITY_EDITOR
    Destroy(this.gameObject);
#endif
    }
}
