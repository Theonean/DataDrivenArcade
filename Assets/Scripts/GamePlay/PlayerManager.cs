using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;


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
    public ChallengeManager challengeManager;

    public PlayerInfoManager playerInfoManager;
    public SelectionManager selectionManager;
    public List<ChallengeFactoryList> challengeFactories = new List<ChallengeFactoryList>();

    [Description("Set Value in Editor determines first Factory that's selected when the game starts")]
    private Vector2 selectedFactoryStartIndex = new Vector2(0, 0);
    private ChallengeFactory selectedFactory;

    public CustomShapeBuilder playerShape;
    private GameManager gm;

    //SCORE MANAGEMENT
    //SIMPLIFY THIS, MULTIPLY SIDES OF FACE WITH COMBO
    //WHAT IF SCORE IS LITERALLY HOW MANY LINES ARE PRESENT IN THE SHAPE
    //MAYBE DEDUCT POINTS FOR EACH LINE THAT'S WRONG IN THE SENT SHAPE?
    private int score = 0;
    private int combo = 0;

    public void ReadyPlayer(List<ChallengeFactoryList> challengeFactories)
    {
        this.challengeFactories = challengeFactories;

        if (playerNum == 1)
        {
            selectedFactoryStartIndex = new Vector2(0, 0);
        }
        else
        {
            selectedFactoryStartIndex = new Vector2(challengeFactories[0].list.Count - 1, 0);
        }

        gm = GameManager.instance;

        selectedFactory = challengeFactories[(int)selectedFactoryStartIndex.y].list[(int)selectedFactoryStartIndex.x];

        gm.LineInputEvent.AddListener(TryAddLine);
        gm.DoubleLineInputEvent.AddListener(ReinitializePlayer);


        print("ScoreManager Start");
        playerShape.InitializeShape(false, selectedFactory.shapeNumSides);

        selectedFactory.shapeBuilder.StartLineHighlight(playerNum, playerShape.GetShapecode().Length);
        selectedFactory.shapeBuilder.selectState = SelectState.SELECTED;

        playerInfoManager.SetScore(score);
        playerInfoManager.SetCombo(combo);

        //activate the selection manager and pass it the starting index of the selected factory
        GetComponentInChildren<SelectionManager>().Activate(selectedFactoryStartIndex);
    }

    public void UnreadyPlayer()
    {
        gm.LineInputEvent.RemoveListener(TryAddLine);
        gm.DoubleLineInputEvent.RemoveListener(ReinitializePlayer);

        ResetPlayer();
        selectedFactory.shapeBuilder.EndLineHighlight();

        GetComponentInChildren<SelectionManager>().Deactivate();
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

        StartCoroutine(selectedFactory.MoveShapeToChallenge(this, playerShape.GetShapecode()));

        playerShape.InitializeShape(false, selectedFactory.shapeNumSides);

    }

    /// <summary>
    /// Reinitializes a player when a double input is received to allow for player to retry the same shape without losing combo
    /// </summary>
    /// <param name="iData"></param>
    private void ReinitializePlayer(InputData iData)
    {
        if (iData.playerNum == playerNum)
        {
            playerShape.InitializeShape(false, selectedFactory.shapeNumSides);
            selectedFactory.shapeBuilder.EndLineHighlight();
            selectedFactory.shapeBuilder.StartLineHighlight(playerNum, playerShape.GetShapecode().Length);
        }
    }

    public void UpdateSelectedFactory(Vector2 newIndex)
    {
        print("Updating Selected Factory by Player");

        //Deselect old Shape
        selectedFactory.shapeBuilder.EndLineHighlight();

        //Set old factory to locked (without selection) 
        if (selectedFactory.shapeBuilder.IsLocked()) selectedFactory.shapeBuilder.selectState = SelectState.LOCKED;

        //Select and highlight new factory / shape
        selectedFactory = challengeFactories[(int)newIndex.y].list[(int)newIndex.x];

        //If player shape is longer than new factory, don't reset it
        if (playerShape.GetShapecode().Length <= selectedFactory.shapeNumSides)
        {
            //Reset player shape with the currently set lines
            playerShape.InitializeShape(true, selectedFactory.shapeNumSides, playerShape.GetShapecode());

            //Check if the new playershape is as long as the challenge shape
            bool sameLengthShape = playerShape.GetShapecode().Length == selectedFactory.shapeBuilder.GetShapecode().Length;

            if (selectedFactory.shapeBuilder.IsLocked() && sameLengthShape)
            {
                selectedFactory.shapeBuilder.StartLineHighlight(playerNum, 0);
            }
            else if (sameLengthShape)
            {
                FinishedShape();
            }
            else
            {
                selectedFactory.shapeBuilder.StartLineHighlight(playerNum, playerShape.GetShapecode().Length);
            }
        }

    }

    public void ShapeArrived(bool correctShape, ChallengeFactory cf)
    {
        playerShape.InitializeShape(true, selectedFactory.shapeNumSides, playerShape.GetShapecode());
        //Score and Combo Calculation
        if (correctShape)
        {
            combo++;

            //Add score to player
            score += selectedFactory.shapeNumSides * combo;
            playerInfoManager.SetScore(score);

            //Inform challengemanager to reduce Lock Number on challenges below this one
            challengeManager.ReduceShapeLockNum(cf);
        }
        //When wrong shape is completed, stop combo and reset multiplier
        else
        {
            combo = 0;
        }

        playerInfoManager.SetCombo(combo);
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
        print("Resetting PLAYER " + playerNum);

        //Reset playe shape to 2 sides
        playerShape.InitializeShape(false, 2);

        //Reset to original position, which also resets player factory
        UpdateSelectedFactory(selectedFactoryStartIndex);
        selectionManager.ResetSelection(selectedFactoryStartIndex);

        playerShape.InitializeShape(false, selectedFactory.shapeNumSides);

        score = 0;
        combo = 0;
        playerInfoManager.Reset();
    }
}
