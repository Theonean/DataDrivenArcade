using SaveSystem;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public SceneType gameState;
    public GameModeData gameModeData;
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

    public static void SetSingleplayer(bool singlePlayer)
    {
        instance.singlePlayer = singlePlayer;
        SaveManager.singleton.SetPlayerCount(2); //Legacy weirdness

        if (singlePlayer)
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

    public static void SetGameMode(string gameMode)
    {
        switch (gameMode)
        {
            case "Classic":
                instance.SwitchToGameMode(GameModeType.CLASSIC);
                break;
            case "Grid":
                instance.SwitchToGameMode(GameModeType.GRID);
                break;
            case "Custom":
                instance.SwitchToGameMode(GameModeType.CUSTOM);
                break;
            default:
                Debug.LogError("Invalid game mode: " + gameMode);
                break;
        }
    }

    private void SwitchToGameMode(GameModeType mode)
    {
        instance.gameModeData = new GameModeData(mode);
        SceneHandler.Instance.SwitchScene(SceneType.GAME30);
    }

    public static void QuitGame()
    {
        Application.Quit();
    }
}
