using System.Collections;
using System.Collections.Generic;
using SaveSystem;
using TMPro;
using Unity.VisualScripting;
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
    public bool coopMode = false;

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
    public TextMeshProUGUI coopObject;
    public TextMeshProUGUI onlineStatusObject;
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

        if (gameState == CurrentScene.LOGIN)
        {
            coopObject = GameObject.Find("CoopMode").GetComponent<TextMeshProUGUI>();
            coopObject.enabled = false;
            onlineStatusObject = GameObject.Find("SaveFilesMode").GetComponent<TextMeshProUGUI>();
            onlineStatusObject.enabled = false;

            if (!arcadeMode && DONOTCOMMIT_MongoConnector.isOnline)
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

        if (!leavingSceneLeft && gameState == CurrentScene.WELCOME)
        {
            print("Fading scene in manually on first load of GM");
            StartCoroutine(FadeSceneIn());
        }

        print("GameManager Awake");
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
        int targetBuildIndex = -1;
        switch (newState)
        {
            case CurrentScene.WELCOME:
                Debug.Log("Switching to welcome");
                coopObject.enabled = false;
                onlineStatusObject.enabled = false;

                // Deinitiate savemanager to stop from loading nonexistant player data (Welcome and MainMenu are not logged in yet)
                SaveManager.singleton.DeInitiate();

                asyncLoad = SceneManager.LoadSceneAsync("00Welcome");
                targetBuildIndex = 0;
                break;
            case CurrentScene.LOGIN:
                Debug.Log("Switching to login");
                // Default set singleplayer mode to true, P1 has to manually switch to P2 mode
                singlePlayer = true;

                coopObject.enabled = true;
                onlineStatusObject.enabled = true;

                // Deinitiate savemanager to stop from loading nonexistant player data (Welcome and MainMenu are not logged in yet)
                SaveManager.singleton.DeInitiate();

                asyncLoad = SceneManager.LoadSceneAsync("01MainMenu");
                targetBuildIndex = 1;
                break;
            case CurrentScene.GAMESELECTION:
                Debug.Log("Switching to game selection");
                coopObject.enabled = true;
                onlineStatusObject.enabled = true;

                // Checking to see where we come from
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

                asyncLoad = SceneManager.LoadSceneAsync("02Selection");
                targetBuildIndex = 2;
                break;
            case CurrentScene.GAME:
                SaveManager.singleton.SaveData();
                coopObject.enabled = false;
                onlineStatusObject.enabled = false;

                Debug.Log("Switching to game");

                asyncLoad = SceneManager.LoadSceneAsync("03Game");
                targetBuildIndex = 3;
                break;
        }

        //Check if build index of next scene is lower than current scene, if yes leaving scene to the left
        if (targetBuildIndex < SceneManager.GetActiveScene().buildIndex)
        {
            leavingSceneLeft = true;
        }
        else
        {
            leavingSceneLeft = false;
        }

        asyncLoad.allowSceneActivation = false; // Stop automatic loading of next scene
        StartCoroutine(LoadSceneAndFadeIn());

        // Changing to requested state for currentscene
        gameState = newState;
    }

    private IEnumerator LoadSceneAndFadeIn()
    {
        yield return StartCoroutine(FadeSceneOut());

        asyncLoad.allowSceneActivation = true;

        yield return new WaitUntil(() => asyncLoad.isDone);

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

        while (asyncLoad.progress < 0.9f || elapsedTime < minTime)
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
        asyncLoad.allowSceneActivation = true;
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

}
