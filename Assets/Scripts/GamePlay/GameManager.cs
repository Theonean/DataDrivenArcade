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
    public UnityEvent<InputData> LineReleasedEvent;
    public UnityEvent<InputData> DoubleLineInputEvent;

    private int[] buttonsPressedLastUpdate = new int[2];
    public UnityEvent<InputData> JoystickInputEvent;
    public UnityEvent<InputData> JoystickReleasedEvent;
    private float[] joystickCooldowns = new float[2];
    private bool[] joystickMovedLastUpdate = new bool[2];
    private float joystickCooldownTime = 0.3f;

    /// <summary>
    /// Setup Singleton instance
    /// </summary>
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
                gameState = CurrentScene.WAITFORSCENELOAD;
                SceneManager.LoadScene("01GameSelection");
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
            //BUTTON Input Handling
            List<int> lineIndexPressed = new List<int>();
            int keysBeingHeld = 0;

            //Get the button inputs per button Input (names from Input Manager)
            for (int lineI = 1; lineI <= 6; lineI++)
            {
                int lineIndex = lineI - 1; //-1 because the line Codes are 0-indexed (to use them directly in arrays)
                if (Input.GetButtonDown("P" + i + "L" + lineI))
                {
                    lineIndexPressed.Add(lineIndex);
                }
                else if (Input.GetButton("P" + i + "L" + lineI))
                {
                    keysBeingHeld += 1;
                }
                else if (Input.GetButtonUp("P" + i + "L" + lineI))
                {
                    LineReleasedEvent?.Invoke(new InputData(lineIndex, i));
                }
            }

            //Add a line when one input was given this frame
            if (lineIndexPressed.Count == 1)
            {
                LineInputEvent?.Invoke(new InputData(lineIndexPressed[0], i));
            }
            //Add a double line when two inputs were given this frame, either by one button being held and another pressed or two pressed this frame
            else if ((keysBeingHeld >= 1 && lineIndexPressed.Count > 0) || lineIndexPressed.Count == 2)
            {
                DoubleLineInputEvent?.Invoke(new InputData(i));
            }


            //JOYSTICK movement Checking
            if (joystickCooldowns[i - 1] <= 0f || !joystickMovedLastUpdate[i - 1])
            {
                joystickCooldowns[i - 1] = joystickCooldownTime;

                //Joystick Input Handling
                Vector2 inputDir = Vector2.zero;
                inputDir.x = Input.GetAxis("P" + i + "Horizontal");
                inputDir.y = Input.GetAxis("P" + i + "Vertical");

                //Cleanup input for joystick movement
                inputDir = ParseInput(inputDir);

                if (inputDir != Vector2.zero)
                {
                    joystickMovedLastUpdate[i - 1] = true;
                    JoystickInputEvent?.Invoke(new InputData(inputDir, i));
                }
                else if (joystickMovedLastUpdate[i - 1])
                {
                    joystickMovedLastUpdate[i - 1] = false;
                    JoystickReleasedEvent?.Invoke(new InputData(i));
                }
            }
            else
            {
                joystickCooldowns[i - 1] -= Time.deltaTime;
            }
        }
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

    //set x and y of Input to either -1,0 or 1
    private Vector2 ParseInput(Vector2 input)
    {
        Vector2 inputDir = input;
        //set x and y of Input to either -1,0 or 1
        if (inputDir.x > 0) inputDir.x = 1;
        else if (inputDir.x < 0) inputDir.x = -1;
        else inputDir.x = 0;

        if (inputDir.y > 0) inputDir.y = 1;
        else if (inputDir.y < 0) inputDir.y = -1;
        else inputDir.y = 0;

        return inputDir;
    }

}
