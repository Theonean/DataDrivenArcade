using System.Linq;
using Localization;
using SaveSystem;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum GameModeState
{
    NOTSTARTED,
    COUNTDOWN,
    RUNNING,
    CHOOSINGANOTHERROUND
}

public class GameModeManager : MonoBehaviour
{
    //This class should handle the game mode logic
    //Initiates the game and tells the challengemanager to create the correct number / grid of challenge
    public static GameModeManager Instance { get; private set; }
    public TextMeshProUGUI countdownRoundTimer;
    private GameManager gm;

    private float roundTime;

    private float timeLeftRound = 0f;
    private float timeLeftBeforePause = 0f;
    public TextMeshProUGUI countdownStartTime;
    private float countdownStart = 3.5f;
    private float timeLeftStart = 0f;
    [Header("Go Again Inbetween Rounds")]
    public FadeElementInOut PauseMenu;
    public FadeElementInOut RoundOverMenu;
    public TextMeshProUGUI PlayerWonText;
    [Header("Game Mode Data")]
    public PlayerManager p1;
    public PlayerManager p2;
    private GameModeState gameModeState = GameModeState.COUNTDOWN;
    private GameModeData gameModeData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void OnEnable()
    {
        CustomUIEvents.OnResumeGame += TogglePauseMenu;
    }

    private void OnDisable()
    {
        CustomUIEvents.OnResumeGame -= TogglePauseMenu;
    }

    private void Start()
    {
        gm = GameManager.instance;
        this.gameModeData = gm.gameModeData;
        roundTime = gameModeData.roundTime;

        countdownRoundTimer.text = roundTime.ToString();

        Debug.LogWarning("Make this into a activate function with the gamemodedata and set the settings");

        timeLeftRound = roundTime;
        timeLeftStart = countdownStart;

        PauseMenu.FadeElementOut();
        RoundOverMenu.FadeElementOut();
    }


    private void Update()
    {

        if (gameModeState == GameModeState.RUNNING)
        {
            timeLeftRound -= Time.deltaTime;
            countdownRoundTimer.text = Mathf.RoundToInt(timeLeftRound).ToString();

            if (timeLeftRound <= 10f)
            {                //Scale the timer text down and up to indicate time is running out
                float t = timeLeftRound - Mathf.Round(timeLeftRound);
                countdownRoundTimer.color = Color.Lerp(Color.white, Color.red, Mathf.PingPong(Time.time, 1f));
            }



            //Round Finished
            if (timeLeftRound < 0)
            {
                //unready both players, BUGFIX: when not first thing in the update loop, players can sneak in an extra input and destroy the highlighting system
                p1.UnreadyPlayer();
                p1.OnResetShape();

                p2.UnreadyPlayer();
                p2.OnResetShape();

                countdownRoundTimer.color = Color.white;
                timeLeftRound = roundTime;
                int playerWon = 0;

                Debug.Log("Round Ended in two player mode - Player 1 Score: " + p1.score + " Player 2 Score: " + p2.score);

                //ADD Scores to both players and add roundswon to winner if not singleplayer
                SaveManager.singleton.playersData[0].roundsPlayed += 1;
                SaveManager.singleton.playersData[0].scores.Add(new Score(p1.score, gameModeData.gameMode, gm.GetPlayerName(2)));

                SaveManager.singleton.playersData[1].roundsPlayed += 1;
                SaveManager.singleton.playersData[1].scores.Add(new Score(p2.score, gameModeData.gameMode, gm.GetPlayerName(1)));

                Debug.LogWarning("Right now both scores are uploaded in the name of player 1, fix this");
                SteamLeaderboardHandler.UploadScore(p1.score);

                if (SteamManager.Initialized)
                {
                    if (GameManager.instance.singlePlayer)
                        SteamStatsAndAchievements.Instance.FinishedSinglePlayerGame(p1.score);
                    else
                        SteamStatsAndAchievements.Instance.FinishedMultiPlayerGame(p1.score);
                }

                if (p1.score > p2.score)
                {
                    SaveManager.singleton.playersData[0].roundsWon += 1;
                    playerWon = 1;
                }
                else if (p1.score < p2.score)
                {
                    SaveManager.singleton.playersData[1].roundsWon += 1;
                    playerWon = 2;
                }


                SaveManager.singleton.SaveData();

                gameModeState = GameModeState.CHOOSINGANOTHERROUND;
                PlayerWonText.text = GameManager.instance.GetPlayerName(playerWon) + " " + PlayerWonText.gameObject.GetComponent<LocalizedText>().GetValue();

                RoundOverMenu.FadeElementIn();

                FindObjectOfType<EndOfMatchStatisticsDisplayer>().DisplayEndOfMatchStatistics();

                countdownRoundTimer.text = roundTime.ToString();
                timeLeftRound = roundTime;
                timeLeftStart = countdownStart;
            }
        }
        else if (gameModeState == GameModeState.COUNTDOWN)
        {
            timeLeftStart -= Time.deltaTime;
            countdownStartTime.text = Mathf.RoundToInt(timeLeftStart).ToString();
            if (timeLeftStart < 0)
            {
                countdownStartTime.enabled = false;
                gameModeState = GameModeState.RUNNING;

                print("Round Started");

                p1.ReadyPlayer();
                p2.ReadyPlayer();
            }
        }
    }

    public void TogglePauseMenu()
    {
        if (gameModeState == GameModeState.COUNTDOWN)
        {
            return;
        }

        //If pressed while the menu is open, simply close it and contine round after 3 second wait
        if (gameModeState == GameModeState.CHOOSINGANOTHERROUND)
        {
            timeLeftStart = countdownStart;
            gameModeState = GameModeState.COUNTDOWN;
            countdownStartTime.enabled = true;
            timeLeftRound = timeLeftBeforePause;
            countdownRoundTimer.text = Mathf.RoundToInt(timeLeftRound).ToString();

            PauseMenu.FadeElementOut();
            SetUIInteractionMode(false);
        }
        //Open the menu 
        else
        {
            timeLeftBeforePause = timeLeftRound;
            p1.UnreadyPlayer();
            p2.UnreadyPlayer();
            gameModeState = GameModeState.CHOOSINGANOTHERROUND;

            PauseMenu.FadeElementIn();
            SetUIInteractionMode(true);
        }
    }

    private void SetUIInteractionMode(bool enabled)
    {
        //BUGFIX: Player Input Component was found to disable Canvas Interaction somehow, so as a workaround it gets disabled when UI Interaction should be possible
        p1.GetComponent<PlayerInput>().enabled = !enabled;
        p2.GetComponent<PlayerInput>().enabled = !enabled;
    }
}
