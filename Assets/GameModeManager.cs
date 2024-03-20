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
    public TextMeshProUGUI countdownTimer;

    public float roundTime = 60f;

    private float timeLeft = 0f;
    public ChallengeManager challengeManager;
    public bool constructClassic;
    public Vector2 challengeGridSize;
    public PlayerManager p1;
    public PlayerManager p2;
    private List<ChallengeFactoryList> challengeFactories;
    private GameModeState gameModeState = GameModeState.RUNNING;

    private void Start()
    {
        challengeManager.gridSize = challengeGridSize;
        if (constructClassic)
        {
            challengeFactories = challengeManager.ConstructChallengeLayout();
        }
        else
        {
            challengeFactories = challengeManager.ConstructChallengeLayout(challengeGridSize);
        }

        //Iterate over all challenge factories and create a challenge shape if it is not selected
        for (int i = 0; i < challengeFactories.Count; i++)
        {
            for (int j = 0; j < challengeFactories[i].list.Count; j++)
            {
                challengeFactories[i].list[j].ResetCF();
            }
        }

        if (constructClassic)
        {
            print("Getting player ready for classic gamemode");
            List<ChallengeFactoryList> p1ChallengeList = new List<ChallengeFactoryList>
            {
                new ChallengeFactoryList(challengeFactories[0].list[0])
            };
            p1.SetPlayerReady(p1ChallengeList);

            List<ChallengeFactoryList> p2ChallengeList = new List<ChallengeFactoryList>
            {
                new ChallengeFactoryList(challengeFactories[0].list[1])
            };
            p2.SetPlayerReady(p2ChallengeList);
        }
        else
        {
            print("Getting Player ready for Factory Gamemode");
            p1.SetPlayerReady(challengeFactories);
            p2.SetPlayerReady(challengeFactories);
        }
    }


    private void Update()
    {

        if (gameModeState == GameModeState.RUNNING)
        {
            timeLeft -= Time.deltaTime;
            countdownTimer.text = timeLeft.ToString("F2");
            if (timeLeft < 0)
            {
                timeLeft = roundTime;
                //Iterate over all challenge factories and reset them
                foreach (ChallengeFactoryList cfl in challengeFactories)
                {
                    foreach (ChallengeFactory cf in cfl.list)
                    {
                        cf.ResetCF();
                    }
                }

                //Reset both players
                p1.ResetPlayer();
                p2.ResetPlayer();
            }
        }
    }
}
