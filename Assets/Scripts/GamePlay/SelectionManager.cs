using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    //This class works as an addon to the ScoreManager to handle player input and selection of Factories
    //Without this class, Scoremanager will default its selected Factory to the first index
    public CustomShapeBuilder playerShape;
    public GameObject selectionSprite;

    public float timeUntilMoveAgainMax = 1.24f;
    private float timeUntilMoveAgain;
    private float timeUntilMoveAgainCounter = 0f;
    private bool canMove = true;

    private Vector2 inputDir;
    private Vector2 factoryIndex;
    private int playerNum;
    public PlayerManager playerManager;
    bool isClassic = false;

    // Start is called before the first frame update
    void Start()
    {
        //Check if playerManager has more than 1 challenge factory
        if (playerManager.challengeFactories.Count < 1)
        {
            //Hide selector and disable movement input, because theres only one factory to select anyway
            selectionSprite.SetActive(false);
            isClassic = true;
        }
        else
        {
            timeUntilMoveAgain = timeUntilMoveAgainMax;

            playerNum = playerManager.playerNum;

            factoryIndex = playerManager.selectedFactoryStartIndex;

            //DIRRRTTTYYYY but its fine for this kind of "DLC" Script I guess?
            selectionSprite.transform.position = playerManager.challengeFactories[(int)factoryIndex.y].list[(int)factoryIndex.x].transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isClassic)
        {
            //Save Input Direction
            inputDir = Vector2.zero;
            inputDir.x = Input.GetAxis("P" + playerNum + "Horizontal");
            inputDir.y = Input.GetAxis("P" + playerNum + "Vertical");

            //Reset when no input received so player can move again
            if (inputDir == Vector2.zero)
            {
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

                //IDEA: what if selection loops around (so reaching rightborder will move you to the left side)

                //Safety check on inputdir to make sure it is within the bounds of the factories
                if (factoryIndex.x + inputDir.x < 0) inputDir.x = 0;
                else if (factoryIndex.x + inputDir.x >= playerManager.challengeFactories[(int)factoryIndex.y].list.Count) inputDir.x = 0;

                if (factoryIndex.y + inputDir.y < 0) inputDir.y = 0;
                else if (factoryIndex.y + inputDir.y >= playerManager.challengeFactories.Count) inputDir.y = 0;

                //Final check if inputDir is still 0,0 as we do not want the updateselectedfactory function to be called when theres no movement (inefficient)
                if (inputDir != Vector2.zero)
                {
                    factoryIndex += inputDir;

                    playerManager.UpdateSelectedFactory(factoryIndex);

                    //DIRRRTTTYYYY but its fine for this kind of "DLC" Script I guess?
                    selectionSprite.transform.position = playerManager.challengeFactories[(int)factoryIndex.y].list[(int)factoryIndex.x].transform.position;
                }
                else
                {
                    print("Didn't move selection because at edge of factory");
                }

            }
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
