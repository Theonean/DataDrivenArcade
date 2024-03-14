using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

[System.Serializable]
public class ChallengeFactoryList
{
    public List<ChallengeFactory> list = new List<ChallengeFactory>();

    public ChallengeFactoryList()
    {
        list = new List<ChallengeFactory>();
    }

    public ChallengeFactoryList(ChallengeFactory challengeFactory)
    {
        list = new List<ChallengeFactory>();
        list.Add(challengeFactory);
    }
}

public class PlayerManager : MonoBehaviour
{
    //What does this class do?
    //track score of player and manage the current selection of the player and challenge shapes

    //What SHOULDN't this class do?
    //Imma leave this to future robin
    //AND THATS EXACTLY HOW WE GOT HERE TO A BROKENASS CLASS THAT DOESN'T KNOW WHAT IT IS

    public int playerNum;
    public GameModeManager gameModeManager;

    public PlayerInfoManager playerInfoManager;
    public List<ChallengeFactoryList> challengeFactories = new List<ChallengeFactoryList>();

    [Description("Set Value in Editor determines first Factory that's selected when the game starts")]
    public Vector2 selectedFactoryStartIndex = new Vector2(0, 0);
    private ChallengeFactory selectedFactory;

    public CustomShapeBuilder playerShape;
    private int shapeSidesNum;
    private GameManager gm;

    //SCORE MANAGEMENT
    private int score = 0;
    private int combo = 0;
    private int comboNeededForMultiplier = 2;
    private int comboMultiplier = 0;

    public void SetPlayerReady(List<ChallengeFactoryList> challengeFactories)
    {
        this.challengeFactories = challengeFactories;

        gm = GameManager.instance;

        selectedFactory = challengeFactories[(int)selectedFactoryStartIndex.y].list[(int)selectedFactoryStartIndex.x];

        gm.LineInputEvent.AddListener(TryAddLine);
        gm.DoubleLineInputEvent.AddListener(ReinitializePlayer);

        
        print("ScoreManager Start");
        selectedFactory.CreateChallenge();

        shapeSidesNum = selectedFactory.shapeNumSides;
        playerShape.InitializeShape(false, shapeSidesNum);

        selectedFactory.shapeBuilder.StartLineHighlight(playerNum, playerShape.GetShapecode().Length);
        selectedFactory.shapeBuilder.selectState = SelectState.SELECTED;

        playerInfoManager.SetName(gm.GetPlayerName(playerNum));
    }

    private void TryAddLine(InputData iData)
    {
        //When event comes from the right Player and the selected factory is not locked (ie. Animating)
        if (iData.playerNum == playerNum && !selectedFactory.shapeBuilder.IsLocked())
        {
            //Play sound from sap on player shape
            playerShape.sap.PlayLinePlaced(iData.lineCode);

            //Check if adding line finished shape
            if (playerShape.AddLine(iData.lineCode))
            {
                FinishedShape();
            }
        }
    }

    private void FinishedShape()
    {
        selectedFactory.shapeBuilder.HighlightNextLine(new InputData(playerNum));
        selectedFactory.shapeBuilder.selectState = SelectState.LOCKEDSELECTED; //lock factory so that player can't add lines while selecting this factory
        string playerShapeCode = playerShape.GetShapecode();

        //print("Comparing " + playerShapeCode + " to " + challengeShapeCode);
        bool isCorrectShape = false;

        //Score and Combo Calculation
        if (playerShapeCode == selectedFactory.shapeBuilder.GetShapecode())
        {
            combo++;
            comboMultiplier = Mathf.RoundToInt((selectedFactory.maxFacesFloorMIN + combo) / comboNeededForMultiplier) + 1; //+1 to avoid 0 multiplier

            int facesAdder = selectedFactory.maxFacesFloorMIN - 2;
            int newAllowedFaces = Mathf.Clamp(comboMultiplier + facesAdder, selectedFactory.maxFacesFloorMIN, 10);

            //Only the personal factory of a player increases in Combo, find other system for the other factories which are "shared"
            selectedFactory.shapeNumSides = newAllowedFaces;
            selectedFactory.SuccessfullShape();

            shapeSidesNum = newAllowedFaces;

            isCorrectShape = true;

            //Add score to player
            score += (newAllowedFaces - 1) * comboMultiplier;
            playerInfoManager.SetScore(score);
        }
        //When wrong shape is completed, stop combo and reset multiplier
        else
        {
            combo = 0;
            comboMultiplier = 0;
            shapeSidesNum = selectedFactory.maxFacesFloorMIN;
            selectedFactory.FailedShape();
        }

        playerShape.InitializeShape(false, shapeSidesNum);

        playerInfoManager.SetCombo(combo);//Subtract 1 to remove the 1 that was added in the if statement
        StartCoroutine(selectedFactory.MoveShapeToChallenge(playerShape, isCorrectShape));
    }

    /// <summary>
    /// Reinitializes a player when a double input is received to allow for player to retry the same shape without losing combo
    /// </summary>
    /// <param name="iData"></param>
    private void ReinitializePlayer(InputData iData)
    {
        if (iData.playerNum == playerNum)
        {
            playerShape.InitializeShape(false, shapeSidesNum);
            selectedFactory.shapeBuilder.EndLineHighlight();
            selectedFactory.shapeBuilder.StartLineHighlight(playerNum, playerShape.GetShapecode().Length);
        }
    }

    public void UpdateSelectedFactory(Vector2 newIndex)
    {
        print("Updating Selected Factory by Player");

        //Deselect old Shape
        selectedFactory.shapeBuilder.EndLineHighlight();

        //Set old factory to locked (without selection) if it was selected
        if (selectedFactory.shapeBuilder.IsLocked()) selectedFactory.shapeBuilder.selectState = SelectState.LOCKED;

        //Select and highlight new factory / shape
        selectedFactory = challengeFactories[(int)newIndex.y].list[(int)newIndex.x];

        shapeSidesNum = selectedFactory.shapeNumSides;

        //Reset player shape with the currently set lines
        playerShape.InitializeShape(true, shapeSidesNum, playerShape.GetShapecode());


        //Check if the new playershape is as long as the challenge shape
        if (playerShape.GetShapecode().Length == selectedFactory.shapeBuilder.GetShapecode().Length)
        {
            FinishedShape();
        }
        else
        {
            selectedFactory.shapeBuilder.StartLineHighlight(playerNum, playerShape.GetShapecode().Length);
        }

    }

    //Getter for combo, needed in audiomanager
    public int GetCombo() { return combo; }
    public int GetScore() { return score; }

    /// <summary>
    /// Resets this player to how how they were at the Start of the Game
    /// </summary>
    /// //TODO: Create Proper Score Manager and move this to there (Score Manager should manage score of both player and handle game resets and the like)
    public void ResetPlayer()
    {
        playerShape.InitializeShape(false, shapeSidesNum);

        print("Resetting GULL GAME ACTUALLY");
        //Iterate over all challenge factories and reset them
        foreach (ChallengeFactoryList cfl in challengeFactories)
        {
            foreach (ChallengeFactory cf in cfl.list)
            {
                cf.ResetCF();
            }
        }

        //Reset to original position, which also resets player factory
        UpdateSelectedFactory(selectedFactoryStartIndex);

        score = 0;
        combo = 0;
        comboMultiplier = 0;
        playerInfoManager.Reset();
    }
}
