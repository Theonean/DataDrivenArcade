using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    //This class should handle the game mode logic
    //Initiates the game and tells the challengemanager to create the correct number / grid of challenge
    public TextMeshProUGUI countdownTimer;
    public ChallengeManager challengeManager;
    public bool constructClassic;
    public PlayerManager p1;
    public PlayerManager p2;

    private void Start()
    {
        List<ChallengeFactoryList> challengeFactories = challengeManager.ConstructChallengeLayout(constructClassic);

        //Iterate over all challenge factories and create a challenge shape if it is not selected
        for (int i = 0; i < challengeFactories.Count; i++)
        {
            for (int j = 0; j < challengeFactories[i].list.Count; j++)
            {
                    challengeFactories[i].list[j].CreateChallenge();
            }
        }

        if (constructClassic)
        {
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
            p1.SetPlayerReady(challengeFactories);
            p2.SetPlayerReady(challengeFactories);
        }
    }
}
