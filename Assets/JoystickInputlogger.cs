using UnityEngine;

public class JoystickInputLogger : MonoBehaviour
{
    void Update()
    {
        // Log joystick axis movement for the first joystick
        float horizontal1 = Input.GetAxis("Horizontal");
        float vertical1 = Input.GetAxis("Vertical");
        if (horizontal1 != 0 || vertical1 != 0)
        {
            Debug.Log($"Joystick 1 Axis - Horizontal: {horizontal1} Vertical: {vertical1}");
        }

        // Assuming you've set up "Horizontal2" and "Vertical2" for the second joystick
        float horizontal2 = Input.GetAxis("Horizontal2");
        float vertical2 = Input.GetAxis("Vertical2");
        if (horizontal2 != 0 || vertical2 != 0)
        {
            Debug.Log($"Joystick 2 Axis - Horizontal: {horizontal2} Vertical: {vertical2}");
        }

        // Log joystick button presses - Example for the first joystick
        for (int i = 0; i < 20; i++)
        {
            if (Input.GetKeyDown($"joystick 1 button {i}")) // For the first joystick
            {
                Debug.Log($"Joystick 1 Button {i} Pressed");
            }
            if (Input.GetKeyDown($"joystick 2 button {i}")) // For the second joystick
            {
                Debug.Log($"Joystick 2 Button {i} Pressed");
            }
        }

        if(Input.GetButtonDown("P1L1")){
            Debug.Log("Place Line 1 for Player 1");
        }

        if(Input.GetButtonDown("P2L1")){
            Debug.Log("Place Line 1 for Player 2");
        }
    }
}

