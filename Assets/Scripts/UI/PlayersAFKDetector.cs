using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PlayersAFKDetector : MonoBehaviour
{
    public TextMeshProUGUI afkText;
    public string targetScene;
    public float AFKWaitTime;
    public float warningTime;
    private float afkTimer = 0f;
    private GameManager gm;
    private bool afk = false;

    private void Start()
    {
        gm = GameManager.instance;
        gm.LineInputEvent.AddListener(InputReceived);
        gm.LineReleasedEvent.AddListener(InputReceived);
        gm.JoystickInputEvent.AddListener(InputReceived);
        gm.JoystickReleasedEvent.AddListener(InputReceived);
        gm.InsertCoinPressed.AddListener(InputReceived);
        gm.SinglePlayerPressed.AddListener(InputReceived);
        gm.MultiplayerPressed.AddListener(InputReceived);
        afkTimer = AFKWaitTime;
    }

    private void Update()
    {
        if (afkTimer > 0f && !afk)
        {
            afkTimer -= Time.deltaTime;
            if (afkTimer <= 0f)
            {
                afk = true;
                GameManager.SwitchScene(targetScene);
            }
            else if (afkTimer <= warningTime)
            {
                //Display the afk timer text and scale it up
                afkText.enabled = true;
                afkText.text = "Going AFK in " + afkTimer.ToString("0") + " \n Press any button to stay active";
            }
        }
    }
    public void ToggleVisibility(bool visible)
    {
        afkText.enabled = visible;
    }

    private void InputReceived(InputData iData)
    {
        InputReceived();
    }
    private void InputReceived(bool b)
    {
        InputReceived();
    }
    private void InputReceived()
    {
        afkTimer = AFKWaitTime;
        afk = false;
        afkText.enabled = false;
    }
}
