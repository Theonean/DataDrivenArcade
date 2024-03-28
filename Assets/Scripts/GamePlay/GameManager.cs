using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public enum CurrentScene
{
    LOGIN,
    GAMESELECTION,
    GAMECLASSIC,
    GAMEFACTORY,
    GAMEMEGASHAPE,
    WAITFORSCENELOAD
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public CurrentScene gameState;
    public bool arcadeMode = false;
    public string p1Name;
    public string p2Name;
    private GameModeManager gameModeManager;

    public PlayerManager[] players;

    public UnityEvent<InputData> LineInputEvent;
    public UnityEvent<InputData> DoubleLineInputEvent;

    void Awake()
    {

        if (instance == null) instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(this);
    }

    public void SwitchScene(CurrentScene newState)
    {

        switch (newState)
        {
            case CurrentScene.GAMESELECTION:
                break;
            case CurrentScene.GAMECLASSIC:
                gameState = CurrentScene.WAITFORSCENELOAD;
                SceneManager.LoadScene("02GameClassic");
                break;
            case CurrentScene.GAMEFACTORY:
                gameState = CurrentScene.WAITFORSCENELOAD;
                SceneManager.LoadScene("03GameFactory");
                break;
            case CurrentScene.GAMEMEGASHAPE:
                break;
            default:
                break;
        }

        gameState = newState;
    }
    private void Update()
    {
        //Check if the player has pressed a button and call relevant events
        //TODO expand system so if player does double input within a given set of updates, it will be counted as a double input
        for (int i = 1; i <= 2; i++)
        {
            int[] numbersPressed = GetInputNumber(i);
            //Add a line when one input was given this frame
            if (numbersPressed.Length == 1)
            {
                LineInputEvent?.Invoke(new InputData(numbersPressed[0], i));
            }
            //Reset the shape when two buttons or more are pressed simultaneously
            else if (numbersPressed.Length > 1)
            {
                DoubleLineInputEvent?.Invoke(new InputData(i));
            }
        }
    }

    /// <summary>
    /// Returns the inputs given by a specific player 
    /// </summary>
    /// <param name="playerNum"></param>
    /// <returns></returns>
    private int[] GetInputNumber(int playerNum)
    {
        List<int> AmountKeysPressedThisFrame = new List<int>();
        int i = 0;

        if (Input.GetButtonDown("P" + playerNum + "L1"))
        {
            AmountKeysPressedThisFrame.Add(0);
            i += 1;
        }

        if (Input.GetButtonDown("P" + playerNum + "L2"))
        {
            AmountKeysPressedThisFrame.Add(1);
            i += 1;
        }

        if (Input.GetButtonDown("P" + playerNum + "L3"))
        {
            AmountKeysPressedThisFrame.Add(2);
            i += 1;
        }

        if (Input.GetButtonDown("P" + playerNum + "L4"))
        {
            AmountKeysPressedThisFrame.Add(3);
            i += 1;
        }

        if (Input.GetButtonDown("P" + playerNum + "L5"))
        {
            AmountKeysPressedThisFrame.Add(4);
            i += 1;
        }

        if (Input.GetButtonDown("P" + playerNum + "L6"))
        {
            AmountKeysPressedThisFrame.Add(5);
            i += 1;
        }

        return AmountKeysPressedThisFrame.ToArray();
    }

    public string GetPlayerName(int playerNum)
    {
        if (playerNum == 1) return p1Name;
        else if (playerNum == 2) return p2Name;
        else
        {
            Debug.Log("Error in GetPlayerName");
            return "Error";
        }
    }

    public void SetPlayerName(int playernum, string playerName)
    {
        if (playernum == 1)
        {
            p1Name = playerName;
        }
        else if (playernum == 2)
        {
            p2Name = playerName;
        }
    }

}
