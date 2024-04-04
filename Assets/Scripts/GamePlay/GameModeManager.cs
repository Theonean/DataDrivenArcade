using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum GameModeState
{
    NOTSTARTED,
    COUNTDOWN,
    RUNNING,
}

public class GameModeManager : MonoBehaviour
{
    //This class should handle the game mode logic
    //Initiates the game and tells the challengemanager to create the correct number / grid of challenge
    public TextMeshProUGUI countdownRoundTimer;
    private GameManager gm;

    public float roundTime = 60f;

    private float timeLeftRound = 0f;
    public TextMeshProUGUI countdownStartTime;
    public float countdownStart = 3;
    private float timeLeftStart = 0f;
    public ChallengeManager challengeManager;
    public Vector2 challengeGridSize;
    public PlayerManager p1;
    public PlayerManager p2;
    private List<ChallengeFactoryList> challengeFactories;
    private GameModeState gameModeState = GameModeState.COUNTDOWN;

    private void Start()
    {
        gm = GameManager.instance;

        timeLeftRound = roundTime;
        timeLeftStart = countdownStart;

        challengeManager.gridSize = challengeGridSize;
        challengeFactories = challengeManager.ConstructChallengeLayout(challengeGridSize);


        //Iterate over all challenge factories and create a challenge shape if it is not selected
        for (int i = 0; i < challengeFactories.Count; i++)
        {
            for (int j = 0; j < challengeFactories[i].list.Count; j++)
            {
                ChallengeFactory cf = challengeFactories[i].list[j];
                cf.ResetCF();
            }
        }

        p1.playerInfoManager.SetName(gm.GetPlayerName(1));
        p2.playerInfoManager.SetName(gm.GetPlayerName(2));
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

                gameModeState = GameModeState.COUNTDOWN;
                countdownStartTime.enabled = true;
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
                timeLeftRound = roundTime;

                p1.ReadyPlayer(challengeFactories);
                p2.ReadyPlayer(challengeFactories);
            }
        }
    }
}
