using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    //What does this class do?
    //track score of player and manage the current selection of the player and challenge shapes

    //What SHOULDN't this class do?
    //Imma leave this to future robin
    

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI lastScoreText;

    [Description("The Factories this player has access to, INDEX 0 MUST BE PRIVATE FACTORY")]
    public ChallengeFactory[] challengeFactories;

    private PlayerFactory playerFactory;
    private int currFactoryIndex = 0;

    private ChallengeFactory challengeFactory;
    //ChallengeShapes change as when a challenge is "completed" the next one gets selected
    private CustomShapeBuilder challengeShapeBuilder;
    private string challengeShapeCode;

    private int score = 0;
    private float remainingTime = 60f;

    //When combo goes over threshhold, increase complexity of next shape
    private int combo = 1;
    private int comboNeededForMultiplier = 5;
    private int comboMultiplier = 1;
    //What to do when the player is too quick?
    private bool noFactoriesSelected = false;

    //Add event for when a factory has added a new challenge shape where factory has pass it's respective priority index
    public UnityEvent<int> onFactoryAddedChallenge;

    private void Awake() {
        print("ScoreManager Start");
        playerFactory = transform.Find("PlayerFactory").GetComponent<PlayerFactory>();
        challengeFactory = challengeFactories[currFactoryIndex];
        
        playerFactory.scoreManager = this;

        //Connect onFactoryAddedChallenge event to the function FactoryAddedChallenge in this class
        onFactoryAddedChallenge.AddListener(FactoryAddedChallenge);

        //set the playerData in each factory to point to this class and set their respective "index" ie priority for selecting their challenge
        for (int i = 0; i < challengeFactories.Length; i++)
        {
            challengeFactories[i].playerData.Add((this, i));
        }
    }
    private void Update() {

        //Countdown tbhe timer until it reaches 0, then reset clock and save score to previous score
        remainingTime -= Time.deltaTime;

        //only use whole numbers for the countdown
        countdownText.text = Mathf.Round(remainingTime).ToString();


        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            lastScoreText.text = score.ToString();
            scoreText.text = "0";
            score = 0;
            remainingTime = 60f;
            combo = 1;
        }
    }

    public void PlayerFinishedShape(string playerShapeCode){
        //set the current challenge to be done by player
        //THIS IS CAUSING ISSUES, NEED TO TRACK THE CURRENT CHALLENGE SHAPE IN A DIFFERENT MANNER
        //
        challengeFactory.GetCurrentChallenge(out challengeShapeBuilder);
        challengeShapeCode = challengeShapeBuilder.GetShapecode();


        CustomShapeBuilder currChallengeShape = challengeShapeBuilder;

        print("Comparing " + playerShapeCode + " to " + challengeShapeCode);
        //TODO Visualize the current combo and Multiplier
        if (playerShapeCode == challengeShapeCode)
        {
            score += combo * comboMultiplier;
            scoreText.text = score.ToString();
            combo++;
            comboMultiplier = Mathf.RoundToInt(combo / comboNeededForMultiplier) + 1; //+1 to avoid 0 multiplier

            //Only the personal factory of a player increases in Combo, find other system for the other factories which are "shared"
            challengeFactories[0].maxAllowedFaces = comboMultiplier;

            GetNextChallengeFromFactories();
        }
        //When wrong shape is completed, stop combo and reset multiplier
        else
        {
            combo = 1;
            comboMultiplier = 1;
        }

        StartCoroutine(MoveShapeToChallenge(currChallengeShape));
    }

    //Search for a factory which has a challenge until all factories are looked through
    private bool GetNextChallengeFromFactories(){
        while(!challengeFactory.GetNextChallenge(out challengeShapeBuilder)){
            //Loop is only used when the first factory has no challenge
            currFactoryIndex++;
            if(currFactoryIndex < challengeFactories.Length)
            {
                challengeFactory = challengeFactories[currFactoryIndex];
            }
            //If we reach end of possible factories, no possible challenges exist for now
            else
            {
                noFactoriesSelected = true;
                challengeShapeCode = "";
                print("We actually have no more challenges bruv");
                return false;
            }
        }
        
        challengeShapeCode = challengeShapeBuilder.GetShapecode();
        return true;
    }

    //Gets called each time any of the challenge factories connected to this object creates a new shape
    //Evaluates whether this new shape should be the next challenge for the player
    public void FactoryAddedChallenge(int factoryIndex){
        //print("Factory " + factoryIndex + " has added a new challenge shape for scoreManager " + gameObject.name);

        //if there are no current factories selected, set the current factory to the factory which just created a new challenge
        //Lower factory indeces have higher priority than higher factory indeces and can override the current challenge
        if(noFactoriesSelected || factoryIndex < currFactoryIndex)
        {
            noFactoriesSelected = false;
            currFactoryIndex = factoryIndex;
            challengeFactory = challengeFactories[currFactoryIndex];
            challengeFactory.GetNextChallenge(out challengeShapeBuilder);
        }
    }

    // Coroutine which moves the shape of the player to the shape of the challenge and when it arrives, the player gets reset
    public IEnumerator MoveShapeToChallenge(CustomShapeBuilder sb)
    {
        CustomShapeBuilder playerShapeBuilder = playerFactory.shapeBuilders[0];
        // Get the player shape and challenge shape positions
        Vector3 playerShapePosition = playerShapeBuilder.transform.position;
        Vector3 challengeShapePosition = sb.transform.position;

        // Calculate the distance and direction between the player shape and challenge shape
        Vector3 direction = (challengeShapePosition - playerShapePosition).normalized;
        float distance = Vector3.Distance(playerShapePosition, challengeShapePosition);

        // Move the player shape towards the challenge shape
        while (distance > 0.1f)
        {
            playerShapeBuilder.transform.position += direction * Time.deltaTime * 5f;
            distance = Vector3.Distance(playerShapeBuilder.transform.position, challengeShapePosition);
            //print(distance);
            yield return null;
        }

        // When the player shape arrives at the challenge shape, check if the shape is right
        //if shape is correct, color of the shape is green, if not, color is red, hold for 0.2 seconds

        //reset the player shape position to the original position
        playerShapeBuilder.transform.position = playerShapePosition;

        // Reset the player shape and building State
        playerFactory.ResetFactory();
    }

//Getter for combo, needed in audiomanager
    public int GetCombo(){ return combo; }
}
