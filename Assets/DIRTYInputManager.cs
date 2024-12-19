using UnityEngine;
using UnityEngine.InputSystem;

public class DIRTYInputManager : MonoBehaviour
{
    public static DIRTYInputManager instance;
    public InputActionReference joinInputActionReference;

    public InputDevice player1DeviceType;
    public InputDevice player2DeviceType;

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

        if (SceneHandler.Instance.currentScene == SceneType.GAME30)
        {
            //Find Player1 and Player2 Game Objects
            GameObject player1 = GameObject.Find("Player1");
            GameObject player2 = GameObject.Find("Player2");

            //Find Player1 and Player2 Input Actions
            PlayerInput player1Input = player1.GetComponent<PlayerInput>();
            PlayerInput player2Input = player2.GetComponent<PlayerInput>();

            // Assign Input Device to players
            player1Input.SwitchCurrentControlScheme(player1DeviceType);
            player2Input.SwitchCurrentControlScheme(player2DeviceType);

            // Enable the corresponding action maps for Player 1 and Player 2
            player1Input.actions.FindActionMap("Player").Enable();
            player2Input.actions.FindActionMap("Player").Enable();
        }
    }

    private void OnEnable()
    {
        // Subscribe to the action's performed event
        joinInputActionReference.action.performed += OnActionPerformed;
        joinInputActionReference.action.Enable();
    }

    private void OnDisable()
    {
        // Unsubscribe from the action's performed event
        joinInputActionReference.action.performed -= OnActionPerformed;
    }

    public void OnActionPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("Player 1 pressed confirm");

        // Check the device that triggered the action
        var device = context.control.device;

        if (device is Keyboard)
        {
            Debug.Log("Player 1 pressed confirm with keyboard");
        }
        else if (device is Gamepad)
        {
            Debug.Log("Player 1 pressed confirm with gamepad");
        }
        else
        {
            Debug.Log("Player 1 pressed confirm with unknown device: " + device.GetType().Name);
        }
    }

    private void GetControlSchemeLastJoinPressed()
    {
        // Get the last control scheme that triggered the join action, either "Gamepad" or "Keyboard"
        var controlScheme = joinInputActionReference.action.activeControl.device.name;

        Debug.Log("Last control scheme that pressed join: " + controlScheme);

        // Check the control scheme
        if (controlScheme.Contains("Gamepad"))
        {
            player1DeviceType = Gamepad.current;
        }
        else if (controlScheme == "Keyboard")
        {
            player1DeviceType = Keyboard.current;
        }
        else
        {
            Debug.Log("Player 1 joined with unknown control scheme: " + controlScheme);
        }
    }

    public void LockinPlayer1Input()
    {
        GetControlSchemeLastJoinPressed();
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
