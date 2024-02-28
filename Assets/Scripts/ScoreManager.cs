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
    public ChallengeFactory challengeFactory;

    private PlayerFactory playerFactory;

    private string challengeShapeCode;

    private int score = 0;
    private float remainingTime = 60f;

    //When combo goes over threshhold, increase complexity of next shape
    private int combo = 1;
    private int comboNeededForMultiplier = 5;
    private int comboMultiplier = 1;

    private void Awake() {
        print("ScoreManager Start");
        playerFactory = transform.Find("PlayerFactory").GetComponent<PlayerFactory>();
        
        playerFactory.scoreManager = this;
    }

    private void Start() {
        challengeShapeCode = challengeFactory.CreateChallenge();
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

        print("Comparing " + playerShapeCode + " to " + challengeShapeCode);
        bool rightShape = false;
        //TODO Visualize the current combo and Multiplier
        if (playerShapeCode == challengeShapeCode)
        {
            score += combo * comboMultiplier;
            scoreText.text = score.ToString();
            combo++;
            comboMultiplier = Mathf.RoundToInt(combo / comboNeededForMultiplier) + 1; //+1 to avoid 0 multiplier

            //Only the personal factory of a player increases in Combo, find other system for the other factories which are "shared"
            challengeFactory.SetMaxAllowedFaces(Mathf.Clamp(comboMultiplier, 2, 10));
            rightShape = true;
        }
        //When wrong shape is completed, stop combo and reset multiplier
        else
        {
            combo = 1;
            comboMultiplier = 1;
        }

        StartCoroutine(MoveShapeToChallenge(challengeFactory.shapeBuilder, rightShape));
    }

    // Coroutine which moves the shape of the player to the shape of the challenge and when it arrives, the player gets reset
    public IEnumerator MoveShapeToChallenge(CustomShapeBuilder sb, bool rightShape)
    {
        CustomShapeBuilder playerShapeBuilder = playerFactory.shapeBuilder;
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
        if(rightShape){
            challengeShapeCode = challengeFactory.CreateChallenge();
        }
    }

//Getter for combo, needed in audiomanager
    public int GetCombo(){ return combo; }
}
