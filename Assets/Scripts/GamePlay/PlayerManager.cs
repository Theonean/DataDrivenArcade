using System.Collections.Generic;
using System.ComponentModel;
using SaveSystem;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


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
        list = new List<ChallengeFactory>
        {
            challengeFactory
        };
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
    public bool isKeyboardMode = false;

    public PlayerInfoManager playerInfoManager;
    public SelectionManager selectionManager;
    public List<ChallengeFactoryList> challengeFactories = new List<ChallengeFactoryList>();

    [Description("Set Value in Editor determines first Factory that's selected when the game starts")]
    private Vector2 selectedFactoryStartIndex = new Vector2(0, 0);
    private ChallengeFactory selectedFactory;

    public CustomShapeBuilder playerShape;
    private GameManager gm;
    private bool playerReady = false;
    private GameObject shadowShape = null;
    public SpriteRenderer[] lockObjects;

    //SCORE MANAGEMENT
    //SIMPLIFY THIS, MULTIPLY SIDES OF FACE WITH COMBO
    //WHAT IF SCORE IS LITERALLY HOW MANY LINES ARE PRESENT IN THE SHAPE
    //MAYBE DEDUCT POINTS FOR EACH LINE THAT'S WRONG IN THE SENT SHAPE?
    public int score = 0;
    private int combo = 0;

    public SpriteRenderer SpriteInputeyboard;
    public SpriteRenderer SpriteInputController;

    private void Start()
    {
        gm = GameManager.instance;

        if (!gm.singlePlayer || playerNum == 1)
        {
            playerInfoManager.SetName(gm.GetPlayerName(playerNum));
            int highScore = SaveManager.singleton.playersData[playerNum - 1].GetHighScore();
            if (highScore < 0)
            {
                highScore = 0;
            }
            playerInfoManager.SetLastScore(highScore);
            print("PlayerManager" + playerNum + " Start with highscore: " + highScore);
        }

        //Set iskeyboardmode by checking PlayerInput Component
        isKeyboardMode = GetComponent<PlayerInput>().currentControlScheme == "Keyboard";

        if (isKeyboardMode)
        {
            SpriteInputeyboard.enabled = true;
            SpriteInputController.enabled = false;
        }
        else
        {
            SpriteInputeyboard.enabled = false;
            SpriteInputController.enabled = true;
        }
    }

    public void ReadyPlayer()
    {
        ReadyPlayer(challengeFactories);
    }

    public void ReadyPlayer(List<ChallengeFactoryList> challengeFactories)
    {
        this.challengeFactories = challengeFactories;
        gm = GameManager.instance;

        if (playerNum == 1)
        {
            selectedFactoryStartIndex = new Vector2(0, 0);
        }
        else
        {
            selectedFactoryStartIndex = new Vector2(challengeFactories[0].list.Count - 1, 0);
        }

        selectedFactory = challengeFactories[(int)selectedFactoryStartIndex.y].list[(int)selectedFactoryStartIndex.x];

        //print("PlayerManager Start");
        playerShape.InitializeShape(false, selectedFactory.shapeNumSides);

        selectedFactory.shapeBuilder.StartLineHighlight(playerNum, playerShape.GetShapecode().Length);
        selectedFactory.shapeBuilder.selectState = SelectState.SELECTED;

        //Create shadow of selected shape as guide for player
        CreateSelectedShapeShadow();

        playerInfoManager.SetScore(score);
        playerInfoManager.SetCombo(combo);

        //activate the selection manager and pass it the starting index of the selected factory
        selectionManager.Activate(selectedFactoryStartIndex);
        playerReady = true;
    }

    public void UnreadyPlayer()
    {
        selectedFactory.shapeBuilder.EndLineHighlight();

        selectionManager.Deactivate();
        playerReady = false;
    }

    public void OnCreateLine1()
    {
        if (playerReady)
            TryAddLine(new InputData(0, playerNum));
    }

    public void OnCreateLine2()
    {
        if (playerReady) TryAddLine(new InputData(1, playerNum));
    }

    public void OnCreateLine3()
    {
        if (playerReady) TryAddLine(new InputData(2, playerNum));
    }

    public void OnCreateLine4()
    {
        if (playerReady) TryAddLine(new InputData(3, playerNum));
    }

    public void OnCreateLine5()
    {
        if (playerReady) TryAddLine(new InputData(4, playerNum));
    }

    public void OnCreateLine6()
    {
        if (playerReady) TryAddLine(new InputData(5, playerNum));
    }

    public void OnResetShape() => ReinitializePlayer(new InputData(playerNum));
    public void OnMoveLeft() => selectionManager.TryMoveSelection(Vector2.left);
    public void OnMoveRight() => selectionManager.TryMoveSelection(Vector2.right);
    public void OnMoveUp() => selectionManager.TryMoveSelection(Vector2.up);
    public void OnMoveDown() => selectionManager.TryMoveSelection(Vector2.down);

    private void TryAddLine(InputData iData)
    {
        //When event comes from the right Player and the selected factory is not locked (ie. Animating)
        if (iData.playerNum == playerNum && !selectedFactory.shapeBuilder.IsLocked())
        {
            selectedFactory.shapeBuilder.HighlightNextLine();

            //Stop playing audio on sap so theres not as much overlapping sounds
            selectedFactory.shapeBuilder.sap.StopCurrentAudio();

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
        selectedFactory.shapeBuilder.HighlightNextLine();
        selectedFactory.shapeBuilder.selectState = SelectState.LOCKEDSELECTED; //lock factory so that player can't add lines while selecting this factory

        StartCoroutine(selectedFactory.MoveShapeToChallenge(this, playerShape.GetShapecode()));

        playerShape.InitializeShape(false, selectedFactory.shapeNumSides);

        //Show player input is blocked while this shape is selected
        foreach (SpriteRenderer lockObject in lockObjects)
        {
            lockObject.enabled = true;
        }
    }

    /// <summary>
    /// Reinitializes a player when a double input is received to allow for player to retry the same shape without losing combo
    /// </summary>
    /// <param name="iData"></param>
    private void ReinitializePlayer(InputData iData)
    {
        if (iData.playerNum == playerNum)
        {

            //Destroy the moving shape to stop other player from getting points
            if (selectedFactory.shapeBuilder.IsLocked())
            {
                Destroy(selectedFactory.movingShape);
                selectedFactory.shapeBuilder.sap.playShapeFinished(false, combo);

                //remove visual lock objects
                foreach (SpriteRenderer lockObject in lockObjects)
                {
                    lockObject.enabled = false;
                }
            }
            //Normal reset of player factory and with proper reset on highlighting
            else
            {
                playerShape.InitializeShape(false, selectedFactory.shapeNumSides);
                selectedFactory.shapeBuilder.EndLineHighlight();
                selectedFactory.shapeBuilder.StartLineHighlight(playerNum, playerShape.GetShapecode().Length);
            }
        }
    }

    public void UpdateSelectedFactory(Vector2 newIndex)
    {
        //print("Updating Selected Factory by Player");

        //Deselect old Shape and stop audio
        selectedFactory.shapeBuilder.EndLineHighlight();
        selectedFactory.shapeBuilder.sap.StopCurrentAudio();

        //Set old factory to locked (without selection) 
        if (selectedFactory.shapeBuilder.IsLocked()) selectedFactory.shapeBuilder.selectState = SelectState.LOCKED;

        //Select and highlight new factory / shape
        selectedFactory = challengeFactories[(int)newIndex.y].list[(int)newIndex.x];

        //Reset player shape to the selected factory
        playerShape.InitializeShape(false, selectedFactory.shapeNumSides);

        //Create shadow of selected shape as guide for player
        CreateSelectedShapeShadow();

        //Start line highlight on new selected factory (starts blinking animation)
        selectedFactory.shapeBuilder.StartLineHighlight(playerNum, 0);

        //Show player input is blocked while this shape is selected
        foreach (SpriteRenderer lockObject in lockObjects)
        {
            lockObject.enabled = selectedFactory.shapeBuilder.IsLocked();
        }
    }

    public void ShapeArrived(bool correctShape, ChallengeFactory cf)
    {

        //Score and Combo Calculation
        if (correctShape)
        {
            //Increase combo which influences score as multiplier
            combo++;

            //Add score to player
            score += (cf.shapeNumSides - 1) * combo; //-1 adjusts to account for shapenumsides going up before this function is called
            playerInfoManager.SetScore(score);

            //Inform challengemanager to reduce Lock Number on challenges below this one
            //if (!challengeManager.IsUnityNull()) challengeManager.ReduceShapeLockNum(cf);
            Debug.LogWarning("Fading out \"Lock\" functionality as grid has become obsolete");
        }
        //When wrong shape is completed, stop combo which resets multiplier
        else
        {
            combo = 0;
        }

        playerInfoManager.SetCombo(combo);

        //If the player still has this factory selected when it arrives
        if (cf.Equals(selectedFactory))
        {
            //Show player input is blocked while this shape is selected
            foreach (SpriteRenderer lockObject in lockObjects)
            {
                lockObject.enabled = false;
            }

            playerShape.InitializeShape(false, selectedFactory.shapeNumSides);

            //Create shadow of selected shape as guide for player
            CreateSelectedShapeShadow();
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
        print("Resetting PLAYER " + playerNum);

        //Reset playe shape to 2 sides -> Old Bug workaround?
        playerShape.InitializeShape(false, 2);

        //Reset to original position, which also resets player factory
        UpdateSelectedFactory(selectedFactoryStartIndex);
        selectionManager.ResetSelection(selectedFactoryStartIndex);

        playerShape.InitializeShape(false, selectedFactory.shapeNumSides);

        //Create shadow of selected shape as guide for player
        CreateSelectedShapeShadow();

        score = 0;
        combo = 0;
        playerInfoManager.Reset();
    }

    /// <summary>
    /// Create a shadow of the shape that is currently selected inside the player factory as a guide for the player
    /// </summary>
    private void CreateSelectedShapeShadow()
    {
        //If shadow doesn't exist yet, create new one from selected factorys shapeBuilder
        if (shadowShape.IsUnityNull())
        {
            //Clone selected factorys shapeBuilder and parent it to the player
            shadowShape = Instantiate(selectedFactory.shapeBuilder.gameObject, transform);
            shadowShape.name = "ShadowShape";
            shadowShape.transform.localScale = playerShape.transform.localScale;
            shadowShape.transform.localPosition = new Vector3(0, 0, 0);
            shadowShape.transform.position = playerShape.transform.position;
            shadowShape.transform.position = new Vector3(shadowShape.transform.position.x, shadowShape.transform.position.y, 0);
        }
        //If shadow already exists, update it to be the same as the selected factorys shapeBuilder
        else
        {
            //Get shapebuilder script from shadowShape
            CustomShapeBuilder shadowShapeBuilder = shadowShape.GetComponent<CustomShapeBuilder>();

            //Recreate shadow to be same as selected factory
            shadowShapeBuilder.InitializeShape(true, selectedFactory.shapeNumSides, selectedFactory.shapeBuilder.GetShapecode());
        }

        //create shadow effect by setting all the line textures to a black color
        foreach (SpriteRenderer lineRenderer in shadowShape.GetComponentsInChildren<SpriteRenderer>())
        {
            lineRenderer.color = new Color(Color.black.r, Color.black.g, Color.black.b, 0.5f);
        }
    }
}
