using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSelection : MonoBehaviour
{
    public JoystickSelectable[] selectedOnStart = new JoystickSelectable[2];
    private string[] playersSelectedActions = new string[2];
    private GameManager gm;

    private void Start()
    {
        gm = GameManager.instance;

        foreach (JoystickSelectable js in selectedOnStart)
        {
            js.Selected();
        }
    }

    public void SelectionChanged(int playernum, string actionType)
    {
        playersSelectedActions[playernum - 1] = actionType;
    }
}
