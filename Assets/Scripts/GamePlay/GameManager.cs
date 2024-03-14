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

    public float roundTime = 60f;
    public TextMeshProUGUI countdownTimer;

    private float timeLeft = 0f;
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

    private void Start()
    {
        //This won't work on final gamemanager but currently Im not using it as a proper singleton so ya know whateva
        if (gameState == CurrentScene.GAMECLASSIC || gameState == CurrentScene.GAMEFACTORY)
        {
            timeLeft = roundTime;
        }
        else
        {
            foreach (PlayerManager player in players)
            {
                player.playerInfoManager.enabled = false;
            }
        }
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
                StartCoroutine(PopulateVariables(newState));
                break;
            case CurrentScene.GAMEFACTORY:
                gameState = CurrentScene.WAITFORSCENELOAD;
                SceneManager.LoadScene("03GameFactory");
                StartCoroutine(PopulateVariables(newState));
                break;
            case CurrentScene.GAMEMEGASHAPE:
                break;
            default:
                break;
        }

        gameState = newState;
    }

    private IEnumerator PopulateVariables(CurrentScene targetState)
    {
        //Wait for the game to load
        yield return new WaitForSeconds(2f);

        players = new PlayerManager[2];
        players[0] = GameObject.Find("Player1").GetComponent<PlayerManager>();
        players[1] = GameObject.Find("Player2").GetComponent<PlayerManager>();
        gameModeManager = players[0].gameModeManager;
        countdownTimer = gameModeManager.countdownTimer;
        gameState = targetState;
    }

    private void Update()
    {
        if (gameState == CurrentScene.GAMECLASSIC || gameState == CurrentScene.GAMEFACTORY)
        {
            timeLeft -= Time.deltaTime;
            countdownTimer.text = timeLeft.ToString("F2");
            if (timeLeft < 0)
            {
                timeLeft = roundTime;

                foreach (PlayerManager player in players)
                {
                    player.ResetPlayer();
                }
            }
        }
        else if (gameState == CurrentScene.GAMEMEGASHAPE)
        {
            timeLeft += Time.deltaTime;
            countdownTimer.text = timeLeft.ToString("F2");

            //Check if one of the players score is higher than 0, if yes, then we can end the game
            foreach (PlayerManager player in players)
            {
                if (player.GetScore() > 0)
                {
                    gameState = CurrentScene.GAMESELECTION;
                    timeLeft = 0;
                    countdownTimer.text = timeLeft.ToString("F2");

                    //reset all players
                    foreach (PlayerManager tempP in players)
                    {
                        tempP.ResetPlayer();
                    }
                }
            }
        }

        //Check if the player has pressed a button and call relevant events
        //TODO expand system so if player does double input within a given set of updates, it will be counted as a double input
        for (int i = 1; i <= 2; i++)
        {
            int[] numbersPressed = GetInputNumber(i);
            //Add a line when one input was given this frame
            if (numbersPressed.Length == 1)
            {
                LineInputEvent.Invoke(new InputData(numbersPressed[0], i));
            }
            //Reset the shape when two buttons or more are pressed simultaneously
            else if (numbersPressed.Length > 1)
            {
                DoubleLineInputEvent.Invoke(new InputData(i));
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

}
