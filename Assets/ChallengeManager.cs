using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChallengeManager : MonoBehaviour
{
    public GameObject challengePrefab;

    //Variables for boundary of "game area" where challenges can be created
    private Vector2 topLeft = new Vector2(-9, 5);
    private Vector2 bottomRight = new Vector2(9, -2);
    private float boundaryPadding = 1f;

    //Variables for Factory Gamemode
    private Vector2 gridSize = new Vector2(3, 3);

    //Variables for CLASSIC Gamemode
    private Vector2 p1ChallengePos = new Vector2(-5, 3);
    private Vector2 p2ChallengePos = new Vector2(5, 3);
    private Vector2 challengeScale = new Vector2(2, 2);

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Constructs Game Mode Challenges, for classic first challenge is player 1 and second is player 2 challenge
    /// </summary>
    /// <returns> returns the created grid of challenge factories </returns>
    public List<ChallengeFactoryList> ConstructChallengeLayout(bool createClassic = true)
    {
        List<ChallengeFactoryList> challengeFactories = new List<ChallengeFactoryList>();
        if (createClassic)
        {
            ChallengeFactoryList factoryList = new ChallengeFactoryList();

            //Add Player 1 Challenge
            GameObject p1Challenge = Instantiate(challengePrefab, p1ChallengePos, Quaternion.identity);
            p1Challenge.transform.localScale = new Vector3(challengeScale.x, challengeScale.y, 1);
            p1Challenge.GetComponent<ChallengeFactory>().SetMaxAllowedFaces(2);
            factoryList.list.Add(p1Challenge.GetComponent<ChallengeFactory>());

            //Add Player 2 Challenge
            GameObject p2Challenge = Instantiate(challengePrefab, p2ChallengePos, Quaternion.identity);
            p2Challenge.transform.localScale = new Vector3(challengeScale.x, challengeScale.y, 1);
            p2Challenge.GetComponent<ChallengeFactory>().SetMaxAllowedFaces(2);
            factoryList.list.Add(p2Challenge.GetComponent<ChallengeFactory>());

            challengeFactories.Add(factoryList);

        }

        return challengeFactories;
    }
}
