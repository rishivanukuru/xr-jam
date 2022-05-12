using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script to accept all MIDI messages, and process them into forms relevant for controlling player animtion.
/// </summary>
[RequireComponent(typeof(MIDIPlayerHandler))]
public class MIDIProcessor : MonoBehaviour
{
    
    // Int flag to know which instrument is being used: 0 - Hands, 1 - Keys, 2 - Drums, 3 - Guitar
    [Tooltip("Flag to know which instrument is being used: 0 - Hands, 1 - Keys, 2 - Drums, 3 - Guitar. Set in Code.")]
    public int InstrumentMode;

    // Reference to the Player Handler that transmits messages over Photon
    private MIDIPlayerHandler _midiPlayerHandler;

    // MIDI message variables
    private string _message;
    private string[] _msgData;
    private string[] _msgNote;

    // List of all active notes
    List<Note> _notes;

    // Keyboard MIDI processing variables
    int _leftNotes = 0;
    int _rightNotes = 0;
    int _leftNoteSum = 0;
    int _rightNoteSum = 0;
    float _rightNoteAvg = 0;
    float _leftNoteAvg = 0;

    // Change middle note to influence animation on differently sized pianos
    int _middleNote = 44;

    [Header("Keyboard Variables")]
    // Public state of Keyboard Animations
    [Tooltip("Keyboard Right hand position")]
    public float KeyRightHand = 0f;
    [Tooltip("Keyboard Left hand position")]
    public float KeyLeftHand = 0f;

    [Tooltip("Keyboard Right hand keypress state")]
    public bool _rightKeyHit = false;
    [Tooltip("Keyboard Left hand keypress state")]
    public bool _leftKeyHit = false;

    [Header("Guitar Variables")]
    // Guitar MIDI Processing Variables
    bool _isPressed = false;
    int _leftNote = 40;

    // Public state of Guitar Animations
    [Tooltip("Keyboard Left hand position")]
    public float GuitarLeftHand = 0f;

    [Tooltip("Keyboard Left hand strum state")]
    public bool _shouldStrum = false;

    [Header("Drum Variables")]
    // Public state of Drum Animations
    [Tooltip("Drum Right Hand Hit State")]
    public bool _rightDrumHit = false;
    [Tooltip("Drum Left Hand Hit State")]
    public bool _leftDrumHit = false;

    void Start()
    {
        _midiPlayerHandler = GetComponent<MIDIPlayerHandler>();
        _notes = new List<Note>();
    }

    // Function to initialize OSC receiver only on the local MIDI client
    public void SetupOSC()
    {
        // Add the OSC receiver component to this gameobject
        OSC OscReceiver = gameObject.AddComponent(typeof(OSC)) as OSC;
        
        // Defined in the OSC bridge application
        OscReceiver.inPort = 5555;
        // Localhost
        OscReceiver.outIP = "127.0.0.1";
        
        OscReceiver.enabled = true;
        // Sets the ProcessMIDI function to be called whenever a new message arrives
        OscReceiver.SetAllMessageHandler(ProcessMIDI);
    }

    void ProcessMIDI(OscMessage message)
    {

        _message = (message.ToString()).Substring(1);
        _msgData = _message.Split('/');
        _msgNote = _msgData[2].Split(' ');

        /*
        // DEBUG MIDI NOTES
        Debug.Log(message.ToString());
        Debug.Log(_msgData[0] + " " + _msgData[1] + " " + _msgData[2]);
        Debug.Log(_msgNote[0] + " " + _msgNote[1] + " " + _msgNote[2]);
        */

        // Add/remove the note from the Notes List
        if (_msgData[0].Equals("NoteOn"))
        {
            if(_msgNote[2] != "0")
            {
                // Note On
                _notes.Add(new Note(int.Parse(_msgNote[0]), int.Parse(_msgData[1])));
            }
            else
            {
                // Note Off
                _notes.Remove(new Note(int.Parse(_msgNote[0]), int.Parse(_msgData[1])));
            }
        }

        // No Instrument, do nothing
        if(_midiPlayerHandler.InstrumentMode == 0)
        {

        }
        else
        // Keyboard
        if(_midiPlayerHandler.InstrumentMode == 1)
        {
            // Reset Keyboard variable states
            _leftNotes = 0;
            _rightNotes = 0;

            _leftNoteSum = 0;
            _rightNoteSum = 0;

            foreach (Note n in _notes)
            {
                // Calculate a sum of all active notes in the left and right sides of the keyboard.
                if(n.noteChannel == 1)
                {
                    if (n.noteNumber < _middleNote)
                    {
                        _leftNotes++;
                        _leftNoteSum += n.noteNumber;

                    }
                    else
                    {
                        _rightNotes++;
                        _rightNoteSum += n.noteNumber;
                    }
                }

            }

            // Find an average position of all active notes in both hands
            if (_rightNotes > 0)
            {
                _rightNoteAvg = _rightNoteSum / _rightNotes;
                KeyRightHand = (_rightNoteAvg - 44f) / 44f;
            }

            if (_leftNotes > 0)
            {
                _leftNoteAvg = _leftNoteSum / _leftNotes;
                KeyLeftHand = 1f - (_leftNoteAvg / 44f);
            }

            // Trigger keyboard hit animations on the appropriate hands
            if (_msgData[0].Equals("NoteOn") && _msgData[1].Equals("1"))
            {
                if (_msgNote[2] != "0")
                {
                    // Note On
                    if(int.Parse(_msgNote[0]) < 44 )
                    {
                        _leftKeyHit = true;
                    }
                    else
                    {
                        _rightKeyHit = true;
                    }
                }
                else
                {
                    // Note Off
                    if (int.Parse(_msgNote[0]) < 44)
                    {
                        _leftKeyHit = false;
                    }
                    else
                    {
                        _rightKeyHit = false;
                    }
                }
            }

        }
        else
        // Drum
        if (_midiPlayerHandler.InstrumentMode == 2)
        {
            // Trigger Drum hit animations on the appropriate hand
            // Even-numbered MIDI notes trigger the left hand, Odd-numbered MIDI notes trigger the right hand
            if (_msgData[0].Equals("NoteOn") && _msgData[1].Equals("1"))
            {
                if (_msgNote[2] != "0")
                {
                    // Note On
                    if (int.Parse(_msgNote[0]) % 2 == 0)
                    {
                        _leftDrumHit = true;
                    }
                    else
                    {
                        _rightDrumHit = true;
                    }
                }
                else
                {
                    // Note Off
                    if (int.Parse(_msgNote[0]) % 2 == 0)
                    {
                        _leftDrumHit = false;
                    }
                    else
                    {
                        _rightDrumHit = false;
                    }
                }
            }

        }
        else
        // Guitar
        if (_midiPlayerHandler.InstrumentMode == 3)
        {
            // Currently only processes movement based on MIDI notes from the 6th string (Low E)
            foreach (Note n in _notes)
            {
                // If the note is arriving from the lowest string
                // Black Jamstik - Channel 7 (also including channel 1 for MIDI keyboard debug)
                if (n.noteChannel == 7 || n.noteChannel == 7) 
                {
                    _isPressed = true;
                    _leftNote = n.noteNumber;
                }
            }

            // Change neck position based on the note if it's being held
            if(_isPressed)
            {
                GuitarLeftHand = Mathf.Clamp((((float)_leftNote) - 40f)/10f,0f,1f);
            }
            else
            {
                // Let the hand remain in that position, awaiting the next string pick
            }

            // Handle Guitar Strum
            if (_msgData[0].Equals("NoteOn"))
            {
                if (_msgNote[2] != "0")
                {
                    _shouldStrum = true;

                }
                else
                {
                    _shouldStrum = false;
                }
            }

        }

    }



    /// <summary>
    /// Note class to make it easier to process MIDI information across multiple streams.
    /// </summary>
    public class Note : IComparable<Note>, IEquatable<Note>
    {
        public int noteNumber;
        public int noteChannel;

        public Note(int newNote)
        {
            noteNumber = newNote;
            noteChannel = 0;
        }

        public Note(int newNote, int newChannel)
        {
            noteNumber = newNote;
            noteChannel = newChannel;
        }

        // This method is required by the IComparable interface
        public int CompareTo(Note other)
        {
            if (other == null)
            {
                return 1;
            }

            return noteNumber - other.noteNumber;
        }

        // This method is required by the IEquatable interface
        public bool Equals(Note other)
        {
            if (other == null) return false;
            return (this.noteNumber.Equals(other.noteNumber) && this.noteChannel.Equals(other.noteChannel));
        }
    }
}
