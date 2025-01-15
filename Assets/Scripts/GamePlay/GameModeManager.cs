using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SaveSystem;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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
    public InputActionReference togglePauseAction;
    public GameObject PauseMenu;
    public GameObject RoundOverMenu;
    public TextMeshProUGUI PlayerWonText;
    [Header("Game Mode Data")]
    public PlayerManager p1;
    public PlayerManager p2;
    private List<ChallengeFactoryList> challengeFactories;
    private GameModeState gameModeState = GameModeState.COUNTDOWN;
    private GameModeData gameModeData;
    public GameObject[] p2Objects;

    private void Start()
    {
        gm = GameManager.instance;
        this.gameModeData = gm.gameModeData;
        roundTime = gameModeData.roundTime;

        countdownRoundTimer.text = roundTime.ToString();

        Debug.LogWarning("Make this into a activate function with the gamemodedata and set the settings");

        timeLeftRound = roundTime;
        timeLeftStart = countdownStart;

        PauseMenu.SetActive(false);
        RoundOverMenu.SetActive(false);

        //Get all inputvisualizer and activate them
        foreach (NewVisualizer iV in FindObjectsOfType<MonoBehaviour>(true).OfType<NewVisualizer>())
        {
            iV.ToggleActive(true);
        }

        //when in singleplayer hide p2 objects
        if (gm.singlePlayer)
        {
            foreach (GameObject obj in p2Objects)
            {
                obj.SetActive(false);
            }
        }

        togglePauseAction.action.Enable();
        togglePauseAction.action.performed += ctx => TogglePauseMenu();

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
                if (!gm.singlePlayer) p2.UnreadyPlayer();

                countdownRoundTimer.color = Color.white;
                timeLeftRound = roundTime;
                int playerWon = 0;

                //In singleplayer mode, save Player 1 Data and only visually set player 1
                if (gm.singlePlayer)
                {
                    Debug.Log("Round Ended in Single Player Mode - Player 1 Score: " + p1.score);

                    SaveManager.singleton.playersData[0].roundsPlayed += 1;
                    SaveManager.singleton.playersData[0].scores.Add(new Score(p1.score, gameModeData.gameMode, gm.GetPlayerName(1)));
                }
                //Otherwise we are in versus mode, save scores for both players
                else
                {
                    Debug.Log("Round Ended in two player mode - Player 1 Score: " + p1.score + " Player 2 Score: " + p2.score);

                    //ADD Scores to both players and add roundswon to winner if not singleplayer
                    SaveManager.singleton.playersData[0].roundsPlayed += 1;
                    SaveManager.singleton.playersData[0].scores.Add(new Score(p1.score, gameModeData.gameMode, gm.GetPlayerName(2)));
                    if (!gm.singlePlayer)
                    {
                        SaveManager.singleton.playersData[1].roundsPlayed += 1;
                        SaveManager.singleton.playersData[1].scores.Add(new Score(p2.score, gameModeData.gameMode, gm.GetPlayerName(1)));

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
                    }
                }

                SaveManager.singleton.SaveData();

                p1.selectedFactory.ResetCF();
                if (!gm.singlePlayer) p2.selectedFactory.ResetCF();

                gameModeState = GameModeState.CHOOSINGANOTHERROUND;
                PlayerWonText.text = GameManager.instance.GetPlayerName(playerWon) + " Won!";

                ToggleMenu(RoundOverMenu);
                RoundOverMenu.GetComponentsInChildren<Button>().Where(b => b.gameObject.name == "ButtonRestart").First().Select();

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
                if (!gm.singlePlayer) p2.ReadyPlayer();
            }
        }

        //QUIT / Continue Menu
        //Check if insert coin button has been pressed -> open the quit / continue menu, menu should not be open / closable when the round has ended
        //if (Input.GetButtonDown("InsertCoinArcade") || Input.GetButtonDown("InsertCoinKeyboard") && gameModeState != GameModeState.COUNTDOWN)
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

            ToggleMenu(PauseMenu);
        }
        //Open the menu 
        else
        {
            timeLeftBeforePause = timeLeftRound;
            p1.UnreadyPlayer();
            if (!gm.singlePlayer) p2.UnreadyPlayer();
            gameModeState = GameModeState.CHOOSINGANOTHERROUND;

            ToggleMenu(PauseMenu);
            PauseMenu.GetComponentsInChildren<Button>().Where(b => b.gameObject.name == "ButtonResume").First().Select();
        }
    }

    public void ToggleMenu(GameObject menu)
    {
        menu.SetActive(!menu.activeSelf);
        SetUIInteractionMode(menu.activeSelf);
    }

    private void SetUIInteractionMode(bool enabled)
    {
        //BUGFIX: Player Input Component was found to disable Canvas Interaction somehow, so as a workaround it gets disabled when UI Interaction should be possible
        Debug.LogError("FindObjectsOfType slow and dirty, refactor this");
        foreach (PlayerInput iV in FindObjectsOfType<MonoBehaviour>(true).OfType<PlayerInput>())
        {
            iV.enabled = !enabled;
        }
    }
}
