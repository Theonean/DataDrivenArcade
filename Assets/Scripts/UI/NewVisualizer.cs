using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class NewVisualizer : MonoBehaviour
{
    public GameObject lines;
    public GameObject[] playerKeyboardBindings;
    public GameObject Player;

    private Animator[] buttonAnimators;
    private bool active = false;

    GameManager gm;

    private void Start()
    {
        gm = GameManager.instance;

        //Joystick Events
        //gm.JoystickInputEvent.AddListener(OnJoystickInput);
        //gm.JoystickReleasedEvent.AddListener(OnJoystickReleased);

        //Button Events
        //gm.LineInputEvent.AddListener((iData) => OnButtonInput(iData, "Pressed"));
        //gm.LineReleasedEvent.AddListener((iData) => OnButtonInput(iData, "Released"));
        Debug.LogError("Repair Input System");

        //Find the animators in the scene
        buttonAnimators = GetComponentsInChildren<Animator>().Where(a => a.gameObject.name.Contains("Button")).ToArray();

        PlayerInput playerInput = Player.GetComponent<PlayerInput>();
        playerInput.actions["CreateLine1"].performed += ctx => OnButtonInput(0, ctx);
        playerInput.actions["CreateLine2"].performed += ctx => OnButtonInput(1, ctx);
        playerInput.actions["CreateLine3"].performed += ctx => OnButtonInput(2, ctx);
        playerInput.actions["CreateLine4"].performed += ctx => OnButtonInput(3, ctx);
        playerInput.actions["CreateLine5"].performed += ctx => OnButtonInput(4, ctx);
        playerInput.actions["CreateLine6"].performed += ctx => OnButtonInput(5, ctx);
    }

    public void ToggleActive(bool showLines)
    {
        gm = GameManager.instance;
        active = !active;
        lines.SetActive(showLines);
    }

    public void OnButtonInput(int lineCode, InputAction.CallbackContext ctx)
    {
        Debug.Log("Button Input: " + lineCode + " " + ctx.action.phase);

        string buttonAnimName = lineCode % 2 == 0 ? "ButtonBlue" : "ButtonRed";
        buttonAnimName += ctx.action.phase == InputActionPhase.Started ? "Pressed" : "Released";

        buttonAnimators[lineCode].Play(buttonAnimName);
    }
}
