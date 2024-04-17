using System.Collections;
using System.Collections.Generic;
using SaveSystem;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public enum CurrentScene
{
    LOGIN,
    GAMESELECTION,
    GAME,
    WAITFORSCENELOAD
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public static CurrentScene gameState;
    public bool arcadeMode = false;
    public static string p1Name;
    public static string p2Name;
    public GameModeData gameModeData;

    [Header("Button Input")]
    public UnityEvent<InputData> LineInputEvent;
    public UnityEvent<InputData> LineReleasedEvent;
    public UnityEvent<InputData> DoubleLineInputEvent;
    [Header("Joystick Input")]
    public UnityEvent<InputData> JoystickInputEvent;
    public UnityEvent<InputData> JoystickReleasedEvent;
    public UnityEvent<bool> InsertCoinPressed;
    public UnityEvent SinglePlayerPressed;
    public UnityEvent MultiplayerPressed;
    private float[] joystickCooldowns = new float[2];
    private bool[] joystickMovedLastUpdate = new bool[2];
    private float joystickCooldownTime = 0.3f;
    private SaveManager saveManager;

    /// <summary>
    /// Setup Singleton instance
    /// </summary>
    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(this);

        if (gameModeData == null)
        {
            gameModeData = new GameModeData(GameModeType.CLASSIC);
            saveManager = SaveManager.singleton;
        }
    }

    public static void SwitchScene(string sceneName)
    {
        if (sceneName == "00MainMenu")
        {
            SwitchScene(CurrentScene.LOGIN);
        }
        else if (sceneName == "01GameSelection")
        {
            SwitchScene(CurrentScene.GAMESELECTION);
        }
        else if (sceneName == "02GameClassic")
        {
            SwitchScene(CurrentScene.GAME);
        }
        else
        {
            Debug.LogWarning("No scene specified for switch");
        }
    }

    public static void SwitchScene(CurrentScene newState)
    {

        switch (newState)
        {
            case CurrentScene.LOGIN:
                Debug.Log("Switching to login");
                SaveManager.singleton.DeInitiate();
                gameState = CurrentScene.LOGIN;
                SceneManager.LoadScene("00MainMenu");
                break;
            case CurrentScene.GAMESELECTION:
                Debug.Log("Switching to game selection");
                //Switching from login to gameselection, reinitialize save data
                if (gameState == CurrentScene.LOGIN)
                {
                    SaveManager.singleton.Initiate(p1Name, 1);
                    SaveManager.singleton.Initiate(p2Name, 2);
                }
                else if (gameState == CurrentScene.GAME)
                {
                    SaveManager.singleton.SaveData();
                }

                gameState = CurrentScene.WAITFORSCENELOAD;
                SceneManager.LoadScene("01GameSelection");
                break;
            case CurrentScene.GAME:
                SaveManager.singleton.SaveData();

                Debug.Log("Switching to game");
                gameState = CurrentScene.WAITFORSCENELOAD;
                SceneManager.LoadScene("02GameClassic");
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

        //Insert Coin Buttons
        if (Input.GetButtonDown("InsertCoinKeyboard"))
        {
            InsertCoinPressed?.Invoke(false);
        }
        else if (Input.GetButtonDown("InsertCoinArcade"))
        {
            InsertCoinPressed?.Invoke(true);
        }

        //Player 1 and 2 Buttons
        if (Input.GetAxis("SingleOrMultiplayer") != 0f)
        {
            if (Input.GetAxis("SingleOrMultiplayer") < 0)
            {
                SinglePlayerPressed?.Invoke();
            }
            else
            {
                MultiplayerPressed?.Invoke();
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
