using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Script attached to each key in the in-room keyboard.
/// </summary>
public class ARKeyboardButton : MonoBehaviour
{
    // Reference to the name input field.
    [SerializeField]
    [Tooltip("The name input field UI element.")]
    private TMP_InputField _nameInput;

    // Only one button functions as "delete" - flag for that
    [SerializeField]
    [Tooltip("True if the current button is supposed to function as a delete key.")]
    private bool _isDelete;

    // Letter of the current key
    private TMP_Text _letter;

    // Button of the current key
    private Button _key;

    void Start()
    {
        // Grab the button component.
        _key = this.GetComponent<Button>();

        // If the button is not Delete i.e. it is a letter input
        if (!_isDelete)
        {
            // Assign the visual text on the button to be the same as the gameobject's name (previously set to be the right letter in the scene)
            this.transform.GetChild(0).GetComponent<TMP_Text>().text = this.gameObject.name;
            
            // Local reference to the letter of the key
            _letter = this.transform.GetChild(0).GetComponent<TMP_Text>();            
            
            // Adds the text input function to the button's OnClick event
            _key.onClick.AddListener(EnterLetter);
        }
        else
        {
            // Adds the delete text function to the button's OnClick event
            _key.onClick.AddListener(DeleteLetter);
        }

    }

    public void EnterLetter()
    {
        // Adds the corresponding character to the input field
        _nameInput.text += _letter.text;
    }

    public void DeleteLetter()
    {
        // As long as the input field has some text, this function deletes the last letter in the name.
        if(_nameInput.text.Length >0)
        {
            _nameInput.text = _nameInput.text.Substring(0, _nameInput.text.Length - 1);
        }
    }

}
