using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SaveSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

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
    public TextMeshProUGUI countdownRoundTimer;
    private GameManager gm;

    private float roundTime;

    private float timeLeftRound = 0f;
    private float timeLeftBeforePause = 0f;
    public TextMeshProUGUI countdownStartTime;
    private float countdownStart = 3.5f;
    private float timeLeftStart = 0f;
    [Header("Go Again Inbetween Rounds")]
    public GameObject goAgainObject;
    public TextMeshProUGUI goAgainTitleText;
    public JoystickSelectable[] startSelectablesGoAgain = new JoystickSelectable[2];
    private String[] playersSelectedActions = new string[2] { "Nothing", "Nothing" };
    [Header("Game Mode Data")]
    public ChallengeManager challengeManager;
    public PlayerManager p1;
    public PlayerManager p2;
    private List<ChallengeFactoryList> challengeFactories;
    private GameModeState gameModeState = GameModeState.COUNTDOWN;
    private GameModeData gameModeData;

    private void Start()
    {
        gm = GameManager.instance;
        this.gameModeData = gm.gameModeData;
        roundTime = gameModeData.roundTime;

        countdownRoundTimer.text = roundTime.ToString();

        Debug.LogWarning("Make this into a activate function with the gamemodedata and set the settings");

        timeLeftRound = roundTime;
        timeLeftStart = countdownStart;

        challengeFactories = challengeManager.ConstructChallengeLayout();

        //Iterate over all challenge factories and create a challenge shape if it is not selected
        for (int i = 0; i < challengeFactories.Count; i++)
        {
            for (int j = 0; j < challengeFactories[i].list.Count; j++)
            {
                ChallengeFactory cf = challengeFactories[i].list[j];
                cf.ResetCF();
            }
        }

        goAgainObject.SetActive(false);

        //Get all inputvisualizer and activate them
        foreach (InputVisualizer iV in FindObjectsOfType<MonoBehaviour>(true).OfType<InputVisualizer>())
        {
            iV.ToggleActive(true);
        }
    }


    private void Update()
    {

        if (gameModeState == GameModeState.RUNNING)
        {
            timeLeftRound -= Time.deltaTime;
            countdownRoundTimer.text = Mathf.RoundToInt(timeLeftRound).ToString();

            //Round Finished
            if (timeLeftRound < 0)
            {
                timeLeftRound = roundTime;

                //ADD PLUS ONE ROUND PLAYED TO BOTH PLAYERS and ADD SCORES
                SaveManager.singleton.playersData[0].roundsPlayed += 1;
                SaveManager.singleton.playersData[1].roundsPlayed += 1;
                SaveManager.singleton.playersData[0].scores.Add(new Score(p1.score, gameModeData.gameMode));
                SaveManager.singleton.playersData[1].scores.Add(new Score(p2.score, gameModeData.gameMode));
                Debug.LogWarning("Wee Woo Wee Woo Scuffed Code Police");
                SaveManager.singleton.SaveData();

                //Reset both players
                p1.UnreadyPlayer();
                p2.UnreadyPlayer();

                //Iterate over all challenge factories and reset them
                foreach (ChallengeFactoryList cfl in challengeFactories)
                {
                    foreach (ChallengeFactory cf in cfl.list)
                    {
                        cf.ResetCF();
                    }
                }

                gameModeState = GameModeState.CHOOSINGANOTHERROUND;
                goAgainTitleText.text = "Round Over! \n Play Again?";
                goAgainObject.SetActive(true);
                foreach (JoystickSelectable js in startSelectablesGoAgain)
                {
                    js.Selected();
                }

                countdownRoundTimer.text = roundTime.ToString();
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
                timeLeftRound = roundTime;

                p1.ReadyPlayer(challengeFactories);
                p2.ReadyPlayer(challengeFactories);
            }
        }

        //QUIT / Continue Menu
        //Check if insert coin button has been pressed -> open the quit / continue menu, menu should not be open / closable when the round has ended
        if (Input.GetButtonDown("InsertCoinArcade") || Input.GetButtonDown("InsertCoinKeyboard") && timeLeftRound > 0)
        {
            //If pressed while the menu is open, simply close it and contine round after 3 second wait
            if (gameModeState == GameModeState.CHOOSINGANOTHERROUND)
            {
                goAgainObject.SetActive(false);
                timeLeftStart = countdownStart;
                gameModeState = GameModeState.COUNTDOWN;
                countdownStartTime.enabled = true;
                p1.ReadyPlayer();
                p2.ReadyPlayer();
                timeLeftRound = timeLeftBeforePause;
                countdownRoundTimer.text = Mathf.RoundToInt(timeLeftRound).ToString();
            }
            //Open the menu 
            else
            {
                timeLeftBeforePause = timeLeftRound;
                p1.UnreadyPlayer();
                p2.UnreadyPlayer();
                gameModeState = GameModeState.CHOOSINGANOTHERROUND;
                goAgainTitleText.text = "Continue?";
                goAgainObject.SetActive(true);
                foreach (JoystickSelectable js in startSelectablesGoAgain)
                {
                    js.Selected();
                }
            }
        }
    }

    public void SelectionChanged(int playerNum, string actionType)
    {
        playersSelectedActions[playerNum - 1] = actionType;
        print("players have these actions selected: " + playersSelectedActions[0] + " " + playersSelectedActions[1]);
        print("they are equals: " + playersSelectedActions[0].Equals(playersSelectedActions[1]));

        if (playersSelectedActions[0].Equals(playersSelectedActions[1]) && !actionType.Equals("Empty"))
        {
            switch (playersSelectedActions[0])
            {
                case "GoAgain":
                    goAgainObject.SetActive(false);
                    //Reset the game
                    print("Resetting Game");
                    timeLeftStart = countdownStart;
                    gameModeState = GameModeState.COUNTDOWN;
                    countdownStartTime.enabled = true;
                    break;
                case "Quit":
                    //Quit the game
                    print("Quitting Game");
                    GameManager.SwitchScene(CurrentScene.GAMESELECTION);
                    break;
            }
        }
    }
}
