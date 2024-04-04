using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Editable : MonoBehaviour
{
    /*
    This class may only be used in combination with a Joystickselectable
    Joystickselectable tells this script whether it's selected and allows for player to enter input or change value of an input field
    */
    private int controlledByPlayer;
    private JoystickSelectable joystickSelectable;
    public UnityEvent buttonPressedEvent;

    private GameManager gm;

    private void Start()
    {
        gm = GameManager.instance;

        joystickSelectable = GetComponent<JoystickSelectable>();
        controlledByPlayer = joystickSelectable.controlledByPlayer;
        joystickSelectable.SelectedEvent.AddListener(Activate);
        joystickSelectable.DeselectedEvent.AddListener(Deactivate);
    }

    public void Activate()
    {
        gm.LineInputEvent.AddListener(ButtonPressed);
    }

    public void Deactivate()
    {
        gm.LineInputEvent.RemoveListener(ButtonPressed);
    }

    private void ButtonPressed(InputData iData)
    {
        if (iData.playerNum == controlledByPlayer)
        {
            joystickSelectable.ToggleInputLock();
            buttonPressedEvent.Invoke();
        }
    }
}
