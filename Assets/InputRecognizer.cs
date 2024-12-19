using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputRecognizer : MonoBehaviour
{
    public TextMeshProUGUI text;
    public InputAction inputAction;

    void OnEnable()
    {
        /*
        ShapeShifterControls.PlayerActions playerActions = new ShapeShifterControls.PlayerActions();
        playerActions.Enable();
        inputAction = playerActions.CreateLine1;
        inputAction.Enable();
        inputAction.performed += OnInputPerformed;
        */
    }

    void OnDisable()
    {
        inputAction.performed -= OnInputPerformed;
        inputAction.Disable();
    }

    private void OnInputPerformed(InputAction.CallbackContext context)
    {
        string input = context.control.displayName;
        text.text = $"Input Detected: {input}";
    }

    void Update()
    {
        if(Gamepad.all.Count > 0)
        {
            Gamepad gamepad = Gamepad.current;
            if(gamepad.buttonSouth.wasPressedThisFrame)
            {
                text.text = "Gamepad Button South";
            }
            if(gamepad.buttonNorth.wasPressedThisFrame)
            {
                text.text = "Gamepad Button North";
            }
            if(gamepad.buttonEast.wasPressedThisFrame)
            {
                text.text = "Gamepad Button East";
            }
            if(gamepad.buttonWest.wasPressedThisFrame)
            {
                text.text = "Gamepad Button West";
            }
        }
    }
}
