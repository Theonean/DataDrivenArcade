using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;

public class ChallengeManager : MonoBehaviour
{
    public GameObject challengePrefab;
    public int faceMultiplierPerLevel;
    public int faceStartValue;

    //Variables to determine the needed amount of shapes to unlock a challenge and its scaling
    private int shapesNeededForUnlockStart = 3;
    private int ShapesNeededForUnlockScalePerLevel = 2;

    //Variables for boundary of "game area" where challenges can be created
    private Vector2 topLeft = new Vector2(-8, 4);
    private Vector2 bottomRight = new Vector2(8, -3);
    private float boundaryPadding = 1f;
    private float spaceInbetweenChallenges = 0.2f;
    private float challengeFactorySideLength = 1.25f;

    [Description("The size of the grid that the challenges will be placed in, X MUST be min. of 2")]
    public Vector2 gridSize;

    List<ChallengeFactoryList> challengeFactories;

    public List<ChallengeFactoryList> ConstructChallengeLayout(Vector2 gridSize)
    {
        List<ChallengeFactoryList> challengeFactories = new List<ChallengeFactoryList>();

        print("Creating Factory Gamemode Layout");

        //Calculate needed scales and positioning boundaries for challenges
        float gameAreaWidth = Math.Abs(topLeft.x - bottomRight.x) - boundaryPadding * 2; //Total width of game area that can be filled with challenges
        float widthUsedForChallenges = gridSize.x * challengeFactorySideLength; //Width of the area that challenges would use with standard size
        float paddingBetweedChallengesUsedForWidth = (gridSize.x - 1) * spaceInbetweenChallenges; //Width of the area that challenges would use with standard size
        float widthAvailableForChallenge = gameAreaWidth - paddingBetweedChallengesUsedForWidth; //Total width of game area that can be filled with challenges

        float scaleX = widthAvailableForChallenge / widthUsedForChallenges;

        float gameAreaHeight = Math.Abs(topLeft.y - bottomRight.y) - boundaryPadding * 2; //Total height of game area that can be filled with challenges
        float heightUsedForChallenges = gridSize.y * challengeFactorySideLength; //Height of the area that challenges would use with standard size
        float paddingBetweedChallengesUsedForHeight = (gridSize.y - 1) * spaceInbetweenChallenges; //Height of the area that challenges would use with standard size
        float heightAvailableForChallenge = gameAreaHeight - paddingBetweedChallengesUsedForHeight; //Total Height of game area that can be filled with challenges

        float scaleY = heightAvailableForChallenge / heightUsedForChallenges;
        print("Scale X: " + scaleX + " Scale Y: " + scaleY);

        // Adjusted spawn position calculation
        Vector2 spawnChallengePosStart = new Vector2(topLeft.x + boundaryPadding + scaleX / 2, topLeft.y - boundaryPadding - scaleY / 2);

        //Use the smaller scale so that the challenges remain square
        float scale = scaleX < scaleY ? scaleX : scaleY;

        // Loop adjustments for challenge creation
        for (int y = 0; y < gridSize.y; y++)
        {
            ChallengeFactoryList factoryList = new ChallengeFactoryList();
            for (int x = 0; x < gridSize.x; x++)
            {
                Vector2 currentPos = new Vector2(
                    spawnChallengePosStart.x + x * (challengeFactorySideLength * scaleX + spaceInbetweenChallenges),
                    spawnChallengePosStart.y - y * (challengeFactorySideLength * scaleY + spaceInbetweenChallenges));

                int challengeFactoryFacesFloorMIN = y * faceMultiplierPerLevel + faceStartValue;
                ChallengeFactory challengeFactory = CreateChallengeFactory(
                    currentPos,
                    challengeFactoryFacesFloorMIN,
                    new Vector2(scale, scale));

                challengeFactory.gridPosition = new Vector2(x, y);

                //First line should always be selectable
                if (y == 0)
                {
                    challengeFactory.SetSelectableState(true);
                }
                else
                {
                    //second line should show lock number, others should hide the number but show their locked state
                    challengeFactory.shapesNeededForUnlockStart = shapesNeededForUnlockStart + ((y - 1) * ShapesNeededForUnlockScalePerLevel);
                    if (y == 1)
                    {
                        //Only the first level should show the lock number, the rest hides it for UI-Clarity
                        challengeFactory.SetSelectableState(false, true);
                    }
                    else
                    {
                        //Hide the lock number for all other levels
                        challengeFactory.SetSelectableState(false, false);
                    }
                }

                factoryList.list.Add(challengeFactory);
            }
            challengeFactories.Add(factoryList);
        }

        this.challengeFactories = challengeFactories;
        return challengeFactories;
    }

    /// <summary>
    /// Reduces the shape lock count of of all challenge factories under the given grid position
    /// </summary>
    /// <param name="gridPos"></param>
    public void ReduceShapeLockNum(ChallengeFactory challengeFactory)
    {
        Vector2 gridPos = challengeFactory.gridPosition;
        //print("Trying to reduce shape lock num at " + gridPos + " with gridsize " + gridSize);
        //First check if the next y level is still within bounds
        if (gridPos.y + 1 < gridSize.y)
        {
            //get the grid position of the challenge factory
            for (int y = (int)gridPos.y + 1; y < gridSize.y; y++)
            {
                //get the challenge factory at the given grid position
                ChallengeFactory cf = challengeFactories[y].list[(int)gridPos.x];

                //Skip unlocked challenge factories
                if (cf.shapeBuilder.selectState != SelectState.UNSELECTABLE)
                {
                    //print("Skipping unlocked challenge factory at grid position: " + cf.gridPosition);
                    continue;
                }

                //When this challenge factory is unlocked, show the number for the next level
                if (cf.ReduceNeededShapesUntilUnlock() && y + 1 < gridSize.y)
                {
                    challengeFactories[y + 1].list[(int)gridPos.x].SetSelectableState(false, true);
                    //print("Showing lock number for next level");
                }

                //print("Reduced Shape Lock Count to: " + cf.shapesNeededForUnlock + " at grid position: " + cf.gridPosition);

            }
        }
        else
        {
            //print("Next position for unlocking would be out of bounds, not doing ANYTHING!");
        }
    }

    private ChallengeFactory CreateChallengeFactory(Vector2 pos, int maxFacesFloorMIN, Vector2 targetScale)
    {
        GameObject challenge = Instantiate(challengePrefab, pos, Quaternion.identity);
        challenge.transform.localScale = new Vector3(targetScale.x, targetScale.y, 1);
        ChallengeFactory challengeFactory = challenge.GetComponent<ChallengeFactory>();
        challengeFactory.maxFacesFloorMIN = maxFacesFloorMIN;
        return challengeFactory;
    }
}
