using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

[System.Serializable]
public class ChallengeFactoryList {
    public List<ChallengeFactory> list = new List<ChallengeFactory>();
}


public class ScoreManager : MonoBehaviour
{
    //What does this class do?
    //track score of player and manage the current selection of the player and challenge shapes

    //What SHOULDN't this class do?
    //Imma leave this to future robin

    public PlayerInfoManager playerInfoManager;
    public List<ChallengeFactoryList> challengeFactories = new List<ChallengeFactoryList>();

    [Description("Set Value in Editor determines first Factory that's selected when the game starts")]
    public Vector2 selectedFactoryIndex = new Vector2(0, 0);
    private ChallengeFactory selectedFactory;

    public PlayerFactory playerFactory;

    private string challengeShapeCode;

    private int score = 0;
    private float remainingTime = 60f;

    //When combo goes over threshhold, increase complexity of next shape
    private int combo = 1;
    private int comboNeededForMultiplier = 5;
    private int comboMultiplier = 1;

    private void Awake() {
        print("ScoreManager Awake");

        selectedFactory = challengeFactories[(int)selectedFactoryIndex.y].list[(int)selectedFactoryIndex.x];
            
        playerFactory.scoreManager = this;
    }

    private void Start() {
        print("ScoreManager Start");
        challengeShapeCode = selectedFactory.CreateChallenge();

        //AAAAGH this feels incredulously dirty but whatever
        playerFactory.SetMaxAllowedFaces(selectedFactory.GetMaxAllowedFaces());
        playerFactory.shapeBuilder.InitializeShape(false, selectedFactory.GetMaxAllowedFaces());
    }

    public void PlayerFinishedShape(string playerShapeCode){

        print("Comparing " + playerShapeCode + " to " + challengeShapeCode);
        bool rightShape = false;
        //TODO Visualize the current combo and Multiplier
        if (playerShapeCode == challengeShapeCode)
        {
            combo++;
            comboMultiplier = Mathf.RoundToInt((selectedFactory.maxFacesFloorMIN + combo) / comboNeededForMultiplier) + 1; //+1 to avoid 0 multiplier

            int facesAdder = selectedFactory.maxFacesFloorMIN - 2;
            int newAllowedFaces = Mathf.Clamp(comboMultiplier + facesAdder, selectedFactory.maxFacesFloorMIN, 10);

            //Only the personal factory of a player increases in Combo, find other system for the other factories which are "shared"
            selectedFactory.SetMaxAllowedFaces(newAllowedFaces);
            selectedFactory.SuccessfullShape();

            playerFactory.SetMaxAllowedFaces(newAllowedFaces);

            rightShape = true;

            //Add score to player
            score += (newAllowedFaces - 1) * comboMultiplier;
            playerInfoManager.SetScore(score);
        }
        //When wrong shape is completed, stop combo and reset multiplier
        else
        {
            combo = 1;
            comboMultiplier = 1;
            selectedFactory.FailedShape();
        }

        playerInfoManager.SetCombo(combo -1);//Subtract 1 to remove the 1 that was added in the if statement
        StartCoroutine(MoveShapeToChallenge(selectedFactory.shapeBuilder, rightShape));
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
            challengeShapeCode = selectedFactory.CreateChallenge();
        }
    }

    public void UpdateSelectedFactory(Vector2 newIndex){
        
        selectedFactory = challengeFactories[(int)newIndex.y].list[(int)newIndex.x];
        challengeShapeCode = selectedFactory.shapeBuilder.GetShapecode();
        
        playerFactory.SetMaxAllowedFaces(selectedFactory.GetMaxAllowedFaces());
        playerFactory.ResetFactory();
    }

    //Getter for combo, needed in audiomanager
    public int GetCombo(){ return combo; }
    public int GetScore(){ return score; }

    public void ResetPlayer(){
        //Iterate over all challenge factores and reset them
        foreach(ChallengeFactoryList cfl in challengeFactories){
            foreach (ChallengeFactory cf in cfl.list)
            {
                cf.ResetCF();
            }
        }

        //Reset to original position, which also resets player factory
        UpdateSelectedFactory(selectedFactoryIndex);

        combo = 1;
        comboMultiplier = 1;
        challengeShapeCode = selectedFactory.CreateChallenge();
        playerInfoManager.Reset();
    }
}
