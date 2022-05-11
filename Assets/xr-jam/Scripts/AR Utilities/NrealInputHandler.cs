using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NRKernal;
using NRKernal.NRExamples;

/// <summary>
/// Changes the input method - Controller or Hands
/// Also has method to gracefully exit the app.
/// </summary>
public class NrealInputHandler : MonoBehaviour
{
    // Event called whenever the input changes
    [Tooltip("Unity event for whenever the input mode is changed - handled in code.")]
    public UnityEvent onInputChange;

    // Method attached to a button in the hidden menu, to toggle between input states.
    public void ToggleInput()
    {

        // Switch input modes
        if(NRInput.CurrentInputSourceType == InputSourceEnum.Controller)
        {
            NRInput.SetInputSource(InputSourceEnum.Hands);
        }
        else
        {
            NRInput.SetInputSource(InputSourceEnum.Controller);
        }

        // Trigger any functions linked to the event
        onInputChange?.Invoke();
    }

    // Gracefully exit the application
    // Attached to the exit button in the hidden menu
    public void QuitApplication()
    {
        AppManager.QuitApplication();
    }

}
