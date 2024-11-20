using System.Collections;
using System.Collections.Generic;
using SaveSystem;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

// BRIAN: Add enum for playermode (singleplayer, multiplayer)


public enum SceneType
{
    WELCOME,
    LOGIN,
    GAMESELECTION,
    GAME,
    //NEW SCENES; KEEP OLD ONES FOR NOW
    MAINMENU01,
    LEADERBOARD02,
    PLAYERAMOUNTSELECTION10,
    PLAYER1NAME11,
    PLAYER2NAME12,
    SELECTGAMEMODE20,
    GAME30
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public SceneType gameState; //REWORK, call over instance instead of static
    public bool arcadeMode = false;
    public bool singlePlayer = false; //BRIAN: if you make this an int, you can use it for savemanager array sizes
    public bool coopMode = true; //Start with true because both inputfields are initialized to AAA

    public string p1Name;
    public string p2Name;
    [SerializeField]
    public SpriteRenderer transitionOverlay;
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
    public AnimationCurve fadeOutCurve;
    public AnimationCurve fadeInCurve;
    private AsyncOperation asyncLoad;
    private bool leavingSceneLeft = false;

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

        if (!leavingSceneLeft && gameState == SceneType.WELCOME)
        {
            print("Fading scene in manually on first load of GM");
            StartCoroutine(FadeSceneIn());
        }

        print("GameManager Awake");
    }

    public static void SwitchScene(string sceneName)
    {
        //Uüdate swoitch case to new scenes
        switch (sceneName)
        {
            case "MainMenu01":
                SwitchScene(SceneType.MAINMENU01);
                break;
            case "Leaderboard02":
                SwitchScene(SceneType.LEADERBOARD02);
                break;
            case "PlayerAmountSelection10":
                SwitchScene(SceneType.PLAYERAMOUNTSELECTION10);
                break;
            case "Player1Name11":
                SwitchScene(SceneType.PLAYER1NAME11);
                break;
            case "Player2Name12":
                SwitchScene(SceneType.PLAYER2NAME12);
                break;
            case "SelectGameMode20":
                SwitchScene(SceneType.SELECTGAMEMODE20);
                break;
            case "Game30":
                SwitchScene(SceneType.GAME);
                break;
            default:
                Debug.LogError("Scene not found: " + sceneName);
                break;
        }
    }

    public static void SwitchScene(SceneType newState)
    {
        int targetBuildIndex = -1;
        switch (newState)
        {
            /*
            case SceneType.WELCOME:
                Debug.Log("Switching to welcome");

                // Deinitiate savemanager to stop from loading nonexistant player data (Welcome and MainMenu are not logged in yet)
                SaveManager.singleton.DeInitiate();

                instance.asyncLoad = SceneManager.LoadSceneAsync("00Welcome");
                targetBuildIndex = 0;
                break;
            case SceneType.LOGIN:
                Debug.Log("Switching to login");
                // Default set singleplayer mode to true, P1 has to manually switch to P2 mode
                singlePlayer = true;

                // Deinitiate savemanager to stop from loading nonexistant player data (Welcome and MainMenu are not logged in yet)
                SaveManager.singleton.DeInitiate();

                instance.asyncLoad = SceneManager.LoadSceneAsync("01MainMenu");
                targetBuildIndex = 1;
                break;
            case SceneType.GAMESELECTION:
                Debug.Log("Switching to game selection");

                // Checking to see where we come from
                switch (gameState)
                {
                    case SceneType.LOGIN:
                        SaveManager.singleton.Initiate(p1Name, 1);
                        SaveManager.singleton.Initiate(p2Name, 2);
                        break;
                    case SceneType.GAME:
                        SaveManager.singleton.SaveData();
                        break;
                }

                instance.asyncLoad = SceneManager.LoadSceneAsync("02Selection");
                targetBuildIndex = 2;
                break;
            case SceneType.GAME:
                SaveManager.singleton.SaveData();

                Debug.Log("Switching to game");

                instance.asyncLoad = SceneManager.LoadSceneAsync("03Game");
                targetBuildIndex = 3;
                break;
                */
            case SceneType.MAINMENU01:
                Debug.Log("Switching to MainMenu01");
                instance.asyncLoad = SceneManager.LoadSceneAsync("01_MainMenu");
                targetBuildIndex = 1;
                break;
            case SceneType.LEADERBOARD02:

                Debug.Log("Switching to Leaderboard02");
                instance.asyncLoad = SceneManager.LoadSceneAsync("02_Leaderboard");
                targetBuildIndex = 2;
                break;
            case SceneType.PLAYERAMOUNTSELECTION10:
                Debug.Log("Switching to PlayerAmountSelection10");
                instance.asyncLoad = SceneManager.LoadSceneAsync("10_PlayerAmountSelection");
                targetBuildIndex = 3;
                break;
            case SceneType.PLAYER1NAME11:
                Debug.Log("Switching to Player1Name11");
                instance.asyncLoad = SceneManager.LoadSceneAsync("11_Player1Name");
                targetBuildIndex = 4;
                break;
            case SceneType.PLAYER2NAME12:
                Debug.Log("Switching to Player2Name12");
                instance.asyncLoad = SceneManager.LoadSceneAsync("12_Player2Name");
                targetBuildIndex = 5;
                break;
            case SceneType.SELECTGAMEMODE20:
                Debug.Log("Switching to SelectGameMode20");
                instance.asyncLoad = SceneManager.LoadSceneAsync("20_SelectGameMode");
                targetBuildIndex = 6;
                break;
            case SceneType.GAME30:
                Debug.Log("Switching to Game30");
                instance.asyncLoad = SceneManager.LoadSceneAsync("30_Game");
                targetBuildIndex = 7;
                break;
        }

        //Check if build index of next scene is lower than current scene, if yes leaving scene to the left
        if (targetBuildIndex < SceneManager.GetActiveScene().buildIndex)
        {
            instance.leavingSceneLeft = true;
        }
        else
        {
            instance.leavingSceneLeft = false;
        }

        instance.asyncLoad.allowSceneActivation = false; // Stop automatic loading of next scene
        instance.StartCoroutine(instance.LoadSceneAndFadeIn());

        // Changing to requested state for SceneType
        instance.gameState = newState;
    }

    private IEnumerator LoadSceneAndFadeIn()
    {
        yield return StartCoroutine(FadeSceneOut());

        instance.asyncLoad.allowSceneActivation = true;

        yield return new WaitUntil(() => instance.asyncLoad.isDone);

        // Wait for the first frame to ensure the scene is fully initialized
        yield return null;

        StartCoroutine(FadeSceneIn());
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
                if (arcadeMode)
                {
                    LineInputEvent?.Invoke(new InputData(lineIndexPressed[0], i));
                }
                else if (Input.GetButton("P" + i + "ToggleLineSelection"))
                {
                    LineInputEvent?.Invoke(new InputData(lineIndexPressed[0] + 3, i));
                }
                else
                {
                    LineInputEvent?.Invoke(new InputData(lineIndexPressed[0], i));
                }
                //Dirty workaround to put in the control to toggle line selection (so you eihter place lines 1 to 3 or 4 to 6 depending on if lshift or rctrl is held)

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

            if (Input.GetButtonUp("P" + i + "ToggleLineSelection"))
            {
                Debug.Log("Toggle Line Selection going up");
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

    public void SetSingleplayer(bool singlePlayer)
    {
        this.singlePlayer = singlePlayer;
        SwitchScene(SceneType.PLAYER1NAME11);
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

    public void SetPlayer1Name()
    {
        //Find NameCreator in scene and get Name
        string playerName = GameObject.Find("CustomNameInput").GetComponent<NameCreator>().GetName();
        p1Name = playerName;

        Debug.Log("Set player 1 name " + playerName);
    }

    public void SetPlayer2Name()
    {
        //Find NameCreator in scene and get Name
        string playerName = GameObject.Find("CustomNameInput").GetComponent<NameCreator>().GetName();
        p2Name = playerName;

        Debug.Log("Set player 1 name " + playerName);
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

    /// <summary>
    /// Moves Camera from center of screen to right and fades from white to black
    /// </summary>
    /// <param name="time">Time taken for the fade and move</param>
    /// <returns></returns>
    private IEnumerator FadeSceneOut()
    {
        float elapsedTime = 0;
        float minTime = 1.5f;
        float cameraZ = Camera.main.transform.position.z;

        transitionOverlay.enabled = true;
        Vector3 startPos = Camera.main.transform.position;
        Vector3 endpos = leavingSceneLeft ? new Vector3(-3, 0, cameraZ) : new Vector3(3, 0, cameraZ);
        Color startColor = Color.clear;
        Color endColor = Color.black;

        while (instance.asyncLoad.progress < 0.9f || elapsedTime < minTime)
        {
            float t = fadeOutCurve.Evaluate(elapsedTime / minTime);
            t *= 1.2f; // Speed up the fade out so evaluationcurve overshoots and we actually reach 100 opacity

            Camera.main.transform.position = Vector3.Lerp(startPos, endpos, t);
            transitionOverlay.color = Color.Lerp(startColor, endColor, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        print("Scene Fade out Complete after " + elapsedTime + " seconds");

        //Allow scene activation during black screen
        instance.asyncLoad.allowSceneActivation = true;
    }

    /// <summary>
    /// Moves Camera from Left of Screen to Center and fades from black to white
    /// </summary>
    /// <param name="time">Time taken for the fade and move</param>
    /// <returns></returns>
    private IEnumerator FadeSceneIn()
    {
        print("Scene Fade In START");
        float fadeInTime = 1.5f; // Duration for the fade-in effect
        float elapsedTime = 0f;

        // Ensure the transition overlay is enabled and setup
        transitionOverlay.enabled = true;
        Color startColor = Color.black;
        Color endColor = Color.clear;

        // Ensure the main camera is correctly set after the scene load
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found!");
            yield break;
        }

        int startX = leavingSceneLeft ? 3 : -3;
        Vector3 startPos = new Vector3(startX, 0, mainCamera.transform.position.z);
        Vector3 endPos = new Vector3(0, 0, mainCamera.transform.position.z);

        while (elapsedTime < fadeInTime)
        {
            float t = elapsedTime / fadeInTime;
            float curveT = fadeInCurve.Evaluate(t); // Use curve for smooth interpolation

            // Interpolate camera position and overlay color
            mainCamera.transform.position = Vector3.Lerp(startPos, endPos, curveT);
            transitionOverlay.color = Color.Lerp(startColor, endColor, curveT);

            elapsedTime += Time.deltaTime;
            //print("t: " + t + " curveT: " + curveT + " elapsed: " + elapsedTime + " delta: " + Time.deltaTime);
            yield return null;
        }

        // Explicitly set final states to avoid lingering interpolation artifacts
        mainCamera.transform.position = endPos;
        transitionOverlay.color = endColor;
        transitionOverlay.enabled = false;

        Debug.Log("Scene Fade-in complete.");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
