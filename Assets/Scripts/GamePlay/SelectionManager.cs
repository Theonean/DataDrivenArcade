using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    //This class works as an addon to the ScoreManager to handle player input and selection of Factories
    //Without this class, Scoremanager will default its selected Factory to the first index
    public PlayerFactory playerFactory;
    //Gets all the layer Information from the factoriesParent and builds the "locked layers" from it
    public GameObject factoriesParent;
    public GameObject selectionSprite;

    public float timeUntilMoveAgainMax = 1.24f;
    private float timeUntilMoveAgain;
    private float timeUntilMoveAgainCounter = 0f;
    private bool canMove = true;
    private bool lastUpdatePressed = false;

    private Vector2 inputDir;
    private Vector2 factoryIndex;
    private int playerNum;
    private ScoreManager scoreManager;

    // Start is called before the first frame update
    void Start()
    {
        timeUntilMoveAgain = timeUntilMoveAgainMax;

        playerNum = playerFactory.playerNum;
        scoreManager = playerFactory.scoreManager;

        factoryIndex = scoreManager.selectedFactoryIndex;

        //DIRRRTTTYYYY but its fine for this kind of "DLC" Script I guess?
        selectionSprite.transform.position = scoreManager.challengeFactories[(int)factoryIndex.y].list[(int)factoryIndex.x].transform.position;
    }

    // Update is called once per frame
    void Update()
    {

        //Save Input Direction
        inputDir = Vector2.zero;
        inputDir.x = Input.GetAxis("P" + playerNum + "Horizontal");
        inputDir.y = Input.GetAxis("P" + playerNum + "Vertical");

        if (inputDir != Vector2.zero)
        {
            lastUpdatePressed = true;
        }
        else
        {
            lastUpdatePressed = false;
            canMove = true;
            timeUntilMoveAgain = timeUntilMoveAgainMax;
        }

        timeUntilMoveAgainCounter -= Time.deltaTime;
        if (timeUntilMoveAgainCounter <= 0f)
        {
            canMove = true;
        }

        //Cleanup input so it can be used in the factory index
        inputDir = ParseInput(inputDir);

        if (inputDir != Vector2.zero && canMove)
        {
            canMove = false;
            timeUntilMoveAgain = timeUntilMoveAgain / 2;
            timeUntilMoveAgainCounter = timeUntilMoveAgain;


            print("Input: " + inputDir);

            //Safety check on inputdir to make sure it is within the bounds of the factories
            if (factoryIndex.x + inputDir.x < 0) inputDir.x = 0;
            else if (factoryIndex.x + inputDir.x >= scoreManager.challengeFactories[(int)factoryIndex.y].list.Count) inputDir.x = 0;

            if (factoryIndex.y + inputDir.y < 0) inputDir.y = 0;
            else if (factoryIndex.y + inputDir.y >= scoreManager.challengeFactories.Count) inputDir.y = 0;

            factoryIndex += inputDir;

            scoreManager.UpdateSelectedFactory(factoryIndex);

            //DIRRRTTTYYYY but its fine for this kind of "DLC" Script I guess?
            selectionSprite.transform.position = scoreManager.challengeFactories[(int)factoryIndex.y].list[(int)factoryIndex.x].transform.position;

        }
    }

    //set x and y of Input to either -1,0 or 1
    private Vector2 ParseInput(Vector2 input)
    {
        Vector2 inputDir = input;

        if (inputDir.x > 0) inputDir.x = 1;
        else if (inputDir.x < 0) inputDir.x = -1;
        else inputDir.x = 0;

        if (inputDir.y > 0) inputDir.y = 1;
        else if (inputDir.y < 0) inputDir.y = -1;
        else inputDir.y = 0;

        return inputDir;
    }
}
