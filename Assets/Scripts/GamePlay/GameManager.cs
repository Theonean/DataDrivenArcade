using System.Collections.Generic;
using SaveSystem;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public enum CurrentScene
{
    WELCOME,
    LOGIN,
    GAMESELECTION,
    GAME,
    WAITFORSCENELOAD
}

// BRIAN: Add enum for playermode (singleplayer, multiplayer)

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public CurrentScene gameState; //REWORK, call over instance instead of static
    public bool arcadeMode = false;
    public bool singlePlayer = false; //BRIAN: if you make this an int, you can use it for savemanager array sizes

    // Add coop mode variable here
    public bool coopMode = false;

    public static string p1Name; //REWORK, call over instance instead of static
    public static string p2Name; //REWORK, call over instance instead of static
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
    public TextMeshProUGUI coopObject;
    public TextMeshProUGUI onlineStatusObject;

    /// <summary>
    /// Setup Singleton instance
    /// </summary>
    void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        DontDestroyOnLoad(this);

        if (gameModeData == null)
        {
            gameModeData = new GameModeData(GameModeType.CLASSIC);
        }

        if (gameState == CurrentScene.LOGIN)
        {
            coopObject = GameObject.Find("CoopMode").GetComponent<TextMeshProUGUI>();
            coopObject.enabled = false;
            onlineStatusObject = GameObject.Find("SaveFilesMode").GetComponent<TextMeshProUGUI>();
            onlineStatusObject.enabled = false;

            if (!arcadeMode && MongoConnector.isOnline)
            {
                onlineStatusObject.enabled = true;
                onlineStatusObject.text = "Online Mode ~ Scores uploaded";
            }
            else
            {
                onlineStatusObject.enabled = true;
                onlineStatusObject.text = "Offline Mode ~ Scores saved locally";
            }
        }
    }

    public void SwitchScene(string sceneName)
    {
        switch (sceneName)
        {
            case "00Welcome":
                SwitchScene(CurrentScene.WELCOME);
                break;
            case "01MainMenu":
                SwitchScene(CurrentScene.LOGIN);
                break;
            case "02Selection":
                SwitchScene(CurrentScene.GAMESELECTION);
                break;
            case "03Game":
                SwitchScene(CurrentScene.GAME);
                break;
            default:
                Debug.LogWarning("No scene specified for switch");
                break;
        }
    }


    public void SwitchScene(CurrentScene newState)
    {

        switch (newState)
        {
            case CurrentScene.WELCOME:
                Debug.Log("Switching to welcome");
                coopObject.enabled = false;
                onlineStatusObject.enabled = false;

                //Deinitiate savemanager to stop from loading nonexistant player data (Welcome and MainMenu are not logged in yet)
                SaveManager.singleton.DeInitiate();

                SceneManager.LoadScene("00Welcome");
                break;
            case CurrentScene.LOGIN:
                Debug.Log("Switching to login");
                //Default set singleplayer mode to true, P1 has to manually switch to P2 mode
                singlePlayer = true;

                coopObject.enabled = true;
                onlineStatusObject.enabled = true;

                //Deinitiate savemanager to stop from loading nonexistant player data (Welcome and MainMenu are not logged in yet)
                SaveManager.singleton.DeInitiate();

                SceneManager.LoadScene("01MainMenu");
                break;
            case CurrentScene.GAMESELECTION:
                Debug.Log("Switching to game selection");
                coopObject.enabled = true;
                onlineStatusObject.enabled = true;

                //Checking to see where we come from
                switch (gameState)
                {
                    case CurrentScene.LOGIN:
                        SaveManager.singleton.Initiate(p1Name, 1);
                        SaveManager.singleton.Initiate(p2Name, 2);
                        break;
                    case CurrentScene.GAME:
                        SaveManager.singleton.SaveData();
                        break;
                }

                SceneManager.LoadScene("02Selection");
                break;
            case CurrentScene.GAME:
                SaveManager.singleton.SaveData();
                coopObject.enabled = false;
                onlineStatusObject.enabled = false;

                Debug.Log("Switching to game");

                SceneManager.LoadScene("03Game");
                break;
        }

        //Changing to requested state for currentscene
        gameState = newState;
    }
    private void Update()
    {
        /*
         PlayerInput() {}
            KeyBoardInput() {}
            JoystickInput() {}
        CoinButtons() {}

        */
        //Check if the player has pressed a button and call relevant events
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
            if ((keysBeingHeld >= 1 && lineIndexPressed.Count > 0) || lineIndexPressed.Count == 2)
            {
                //Iterate over pressed line indexes and release events
                foreach (int lineIndex in lineIndexPressed)
                {
                    LineReleasedEvent?.Invoke(new InputData(lineIndex, i));
                }

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
            print("Insert Coin Pressed");
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

        //If both player names are the same, coop mode is activated
        if (p1Name == p2Name)
        {
            coopObject.enabled = true;
            coopObject.text = "Coop ~ scores combined";
        }
        else
        {
            coopObject.enabled = true;
            coopObject.text = "1V1 ~ scores separate";
        }
    }

    //Round inputVector to nearest of -1,0 or 1
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
