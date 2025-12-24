using UnityEngine;
using UnityEngine.InputSystem;

public class InputDeviceRegistry : MonoBehaviour
{
    public static InputDevice Player1Device;

    [SerializeField] private InputActionReference joinInputActionReference;

    private void OnEnable()
    {
        joinInputActionReference.action.Enable();
        joinInputActionReference.action.performed += OnJoinPerformed;
    }

    private void OnDisable()
    {
        joinInputActionReference.action.performed -= OnJoinPerformed;
        joinInputActionReference.action.Disable();
    }

    private void OnJoinPerformed(InputAction.CallbackContext context)
    {
        if (Player1Device != null) return; // Already assigned

        Player1Device = context.control.device;
        Debug.Log("[InputDeviceRegistry] Saved Player 1 Device: " + Player1Device.name);
    }
}
