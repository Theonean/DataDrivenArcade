using SaveSystem;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public SceneType gameState;
    public GameModeData gameModeData = new GameModeData(GameModeType.CLASSIC);
    public bool arcadeMode = false;
    public bool singlePlayer = false;

    public string p1Name;
    public string p2Name;
    public bool p1InputKeyboard = true;
    public bool p2InputKeyboard = true;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    private void OnEnable()
    {
        //Prevent duplicate singleton in scene from subscribing to events
        if (instance != this) return;

        CustomUIEvents.OnQuitGame += QuitGame;
        CustomUIEvents.OnSetPlayerCount += SetPlayerCount;
    }

    private void OnDisable()
    {
        //Prevent duplicate singleton in scene from unsubscribing to events
        if (instance != this) return;

        CustomUIEvents.OnQuitGame -= QuitGame;
        CustomUIEvents.OnSetPlayerCount -= SetPlayerCount;
    }

    private void Start()
    {
        // Set the first selected UI element for navigation
        //cheggi ned wieso das do isch
        Debug.LogWarning("Silly stuff");
        if (EventSystem.current.firstSelectedGameObject != null)
        {
            EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);
        }
    }

    public static void SetPlayerCount(int playerCount)
    {
        instance.singlePlayer = playerCount == 1;

        SaveManager.singleton.SetPlayerCount(2); //Legacy weirdness

        if (playerCount == 1)
        {
            instance.SetPlayerName(2, "henryai");
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

    public void SetPlayerName(int playerNum, string playerName)
    {
        if (playerNum == 1)
        {
            p1Name = playerName;
            SaveManager.singleton.Initiate(playerName, playerNum);
        }
        else if (playerNum == 2)
        {
            p2Name = playerName;
            SaveManager.singleton.Initiate(playerName, playerNum);
        }
        else
        {
            Debug.LogError("Error in SetPlayerName, invalid playerNum " + playerNum);
        }

        Debug.Log("Set player " + playerNum + " name " + playerName);
    }

    public static void QuitGame()
    {
        Application.Quit();
    }
}
