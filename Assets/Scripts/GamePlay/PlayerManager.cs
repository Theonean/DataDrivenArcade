using System.Collections.Generic;
using System.ComponentModel;
using SaveSystem;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
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

    [SerializeField]
    public ChallengeFactory selectedFactory;

    public CustomShapeBuilder playerShape;
    private GameManager gm;
    private bool playerReady = false;
    [SerializeField] private GameObject Hammer;
    [SerializeField] private Sprite[] growDirectionSprites = new Sprite[3];
    [SerializeField] private SpriteRenderer growDirectionSpriteRenderer;

    //SCORE MANAGEMENT
    //SIMPLIFY THIS, MULTIPLY SIDES OF FACE WITH COMBO
    //WHAT IF SCORE IS LITERALLY HOW MANY LINES ARE PRESENT IN THE SHAPE
    //MAYBE DEDUCT POINTS FOR EACH LINE THAT'S WRONG IN THE SENT SHAPE?
    public int score = 0;
    private int combo = 0;
    private int growDirection = 1; //1 is up, 0 neutral, -1 down

    public SpriteRenderer SpriteInputeyboard;
    public SpriteRenderer SpriteInputController;
    public SpriteRenderer SpriteHenryAI;
    public UnityEvent<bool> OnChangeReadyState;
    public UnityEvent<string> OnFinishedShape;
    private int shapesCompleted = 0;
    private int shapesCorrect = 0;
    public float LineAccuracy { get => (float)shapesCorrect / shapesCompleted; }
    private int linesPlaced = 0;
    private float timeSpent = 0.1f; //Not zero to avoid division by zero
    public float InputSpeed { get => linesPlaced / timeSpent; }

    private void Start()
    {
        gm = GameManager.instance;

        playerInfoManager.SetName(gm.GetPlayerName(playerNum));
        int highScore = SaveManager.singleton.playersData[playerNum - 1].GetHighScore();
        if (highScore < 0)
        {
            highScore = 0;
        }
        playerInfoManager.SetLastScore(highScore);
        print("PlayerManager" + playerNum + " Start with highscore: " + highScore);


        if (playerNum == 2 && GameManager.instance.singlePlayer)
        {
            SpriteInputeyboard.enabled = false;
            SpriteInputController.enabled = false;
            SpriteHenryAI.enabled = true;
            GetComponent<PlayerInput>().enabled = false;
        }
        else
        {
            InputDevice device = GameManager.instance.playerDevices[playerNum - 1];
            GetComponent<PlayerInput>().SwitchCurrentControlScheme(device);
            if (device is Keyboard)
            {
                Debug.Log("Player " + playerNum + " is in Keyboard Mode");
                SpriteInputeyboard.enabled = true;
                SpriteInputController.enabled = false;
            }
            else
            {
                Debug.Log("Player " + playerNum + " is in Controller Mode");
                SpriteInputeyboard.enabled = false;
                SpriteInputController.enabled = true;
            }
        }

        selectedFactory.ResetCF();
        selectedFactory.SetSelectableState(true);

        playerShape.transform.position = selectedFactory.shapeBuilder.transform.position - Vector3.forward;
    }

    private void Update()
    {
        if (playerReady)
            timeSpent += Time.deltaTime;
    }

    public void ReadyPlayer()
    {
        gm = GameManager.instance;

        //print("PlayerManager Start");
        playerShape.InitializeShape(false, selectedFactory.shapeNumSides);

        selectedFactory.shapeBuilder.StartLineHighlight(playerNum, 0);
        selectedFactory.shapeBuilder.selectState = SelectState.SELECTED;

        //Create shadow of selected shape as guide for player
        //CreateSelectedShapeShadow();

        playerInfoManager.SetScore(score);
        playerInfoManager.SetCombo(combo);

        playerReady = true;
        OnChangeReadyState.Invoke(playerReady);
    }

    public void UnreadyPlayer()
    {
        selectedFactory.shapeBuilder.EndLineHighlight();

        playerReady = false;
        OnChangeReadyState.Invoke(playerReady);
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

    public void OnResetShape() => DeleteLastLine();

    public void OnChangeShapeGrowDirection()
    {
        switch (growDirection)
        {
            case 1:
                growDirection = -1;
                break;
            case -1:
                growDirection = 0;
                break;
            case 0:
                growDirection = 1;
                break;
        }

        growDirectionSpriteRenderer.sprite = growDirectionSprites[growDirection + 1];
    }

    private void TryAddLine(InputData iData)
    {
        //When event comes from the right Player and the selected factory is not locked (ie. Animating)
        if (iData.playerNum == playerNum && !selectedFactory.shapeBuilder.IsLocked())
        {
            linesPlaced++;
            selectedFactory.shapeBuilder.HighlightNextLine();

            //Stop playing audio on sap so theres not as much overlapping sounds
            selectedFactory.shapeBuilder.sap.StopCurrentAudio();

            //Play sound from sap on player shape
            playerShape.sap.PlayLinePlaced(iData.lineCode);

            bool IsCorrectLine = selectedFactory.shapeBuilder.GetShapecode()[playerShape.GetShapecode().Length] == iData.lineCode;

            //Check if adding line finished shape
            if (playerShape.AddLine(iData.lineCode, IsCorrectLine ? LineState.REGULAR : LineState.SHADOW))
            {
                shapesCompleted++;
                FinishedShape();
            }

            //Ultra hacky way of getting the last line placed, but I didn't wanna make a specific interface just for this on the shapeBuilder
            GameObject lastLinePlaced = playerShape.transform.GetChild(playerShape.transform.childCount - 1).gameObject;
            Hammer.transform.position = new Vector3(lastLinePlaced.transform.position.x, lastLinePlaced.transform.position.y, Hammer.transform.position.z);
            Hammer.GetComponent<Animator>().StopPlayback();
            Hammer.GetComponent<Animator>().Play("HammerSmash");
        }
    }

    private void FinishedShape()
    {
        selectedFactory.shapeBuilder.HighlightNextLine();
        selectedFactory.shapeBuilder.selectState = SelectState.LOCKEDSELECTED; //lock factory so that player can't add lines while selecting this factory

        //TODO: Remove and simply this looping call to CF, Shapes used to be able to fly to the CF, thats no more however
        StartCoroutine(selectedFactory.MoveShapeToChallenge(this, playerShape.GetShapecode(), growDirection));

        OnFinishedShape.Invoke(playerShape.GetShapecode());

        playerShape.InitializeShape(false, selectedFactory.shapeNumSides);

        if (SteamManager.Initialized)
            SteamStatsAndAchievements.Instance.CreatedShape();
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
            }
            //Normal reset of player factory and with proper reset on highlighting
            else
            {
                playerShape.InitializeShape(false, selectedFactory.shapeNumSides);
                selectedFactory.shapeBuilder.EndLineHighlight(true);
                selectedFactory.shapeBuilder.StartLineHighlight(playerNum, playerShape.GetShapecode().Length);
            }
        }
    }

    private void DeleteLastLine()
    {
        if (playerReady)
        {
            playerShape.DeleteLastLine();
            selectedFactory.shapeBuilder.HighlightLastLine();
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

            shapesCorrect++;
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
            playerShape.InitializeShape(false, selectedFactory.shapeNumSides);
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

        playerShape.InitializeShape(false, selectedFactory.shapeNumSides);

        //Create shadow of selected shape as guide for player
        //CreateSelectedShapeShadow();

        score = 0;
        combo = 0;
        playerInfoManager.Reset();
    }
}
