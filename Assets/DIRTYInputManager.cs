using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class DIRTYInputManager : MonoBehaviour
{
    public static DIRTYInputManager instance;
    public InputActionReference joinInputActionReference;

    public InputDevice player1DeviceType;
    public InputDevice player2DeviceType;
    public bool Player1IsKeyboard;
    public bool Player2IsKeyboard;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        if (instance != this) return;
        joinInputActionReference.action.Enable();
        CustomUIEvents.OnSetPlayerCount += LockinPlayerInput;
    }

    private void OnDisable()
    {
        if (instance != this) return;
        joinInputActionReference.action.Disable();
        CustomUIEvents.OnSetPlayerCount -= LockinPlayerInput;
    }

    private void GetControlSchemeLastJoinPressed()
    {
        // Get the last control scheme that triggered the join action, either "Gamepad" or "Keyboard"
        var controlScheme = joinInputActionReference.action.activeControl.device.name;

        Debug.Log("Last control scheme that pressed join: " + controlScheme);

        // Check the device that triggered the action
        var device = joinInputActionReference.action.activeControl.device;

        if (device is Keyboard)
        {
            Debug.Log("Player 1 pressed confirm with keyboard");
            player1DeviceType = Keyboard.current;
            Player1IsKeyboard = true;
            Player2IsKeyboard = false;
        }
        else if (device is Gamepad)
        {
            Debug.Log("Player 1 pressed confirm with gamepad");
            player1DeviceType = Gamepad.current;
            Player1IsKeyboard = false;
            Player2IsKeyboard = true;
        }
        else
        {
            Debug.Log("Player 1 pressed confirm with unknown device: " + device.GetType().Name);
        }
    }

    public void LockinPlayerInput(int playerCount)
    {
        GetControlSchemeLastJoinPressed();

        if (playerCount == 1)
        {
            player2DeviceType = null;
            Debug.Log("Player 1 locked in with: " + player1DeviceType.GetType().Name);
            Debug.Log("Player 2 does not have a control scheme");
        }
        else if (playerCount == 2)
        {
            player2DeviceType = player1DeviceType == Gamepad.current ? Keyboard.current : Gamepad.current;

            if (player2DeviceType == null)
            {
                Debug.Log("Player 1 locked in with: " + player1DeviceType.GetType().Name);
                Debug.Log("Player 2 does not have a control scheme");
            }
            else
            {
                Debug.Log("Player 1 locked in with: " + player1DeviceType.GetType().Name);
                Debug.Log("Player 2 locked in with: " + player2DeviceType.GetType().Name);
            }
        }
    }
}
