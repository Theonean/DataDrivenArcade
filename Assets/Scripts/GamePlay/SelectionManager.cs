using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;

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
    bool isClassic = true;

    // Start is called before the first frame update
    public void Activate(Vector2 startIndex)
    {
        //Check if playerManager has  1 challenge factory
        if (playerManager.challengeFactories.Count == 1)
        {
            //Hide selector and disable movement input, because theres only one factory to select anyway
            selectionSprite.SetActive(false);
            isClassic = true;
        }
        else
        {
            timeUntilMoveAgain = timeUntilMoveAgainMax;

            playerNum = playerManager.playerNum;

            //Change selectionsprite colour to blue if playernum 2
            if (playerNum == 2)
            {
                selectionSprite.GetComponent<SpriteShapeRenderer>().color = Color.blue;
            }

            factoryIndex = startIndex;

            //DIRRRTTTYYYY but its fine for this kind of "DLC" Script I guess?
            selectionSprite.transform.position = playerManager.challengeFactories[(int)factoryIndex.y].list[(int)factoryIndex.x].transform.position;
            isClassic = false;
        }
    }

    public void ResetSelection(Vector2 startIndex)
    {
        factoryIndex = startIndex;
        selectionSprite.transform.position = playerManager.challengeFactories[(int)factoryIndex.y].list[(int)factoryIndex.x].transform.position;
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

                //Final check if inputDir is still 0,0 as we do not want the updateselectedfactory function to be called when theres no movement (inefficient)

                Vector2 tempFactoryIndex = factoryIndex + inputDir;
                if (PositionWithingGridBounds(tempFactoryIndex))
                {
                    ChallengeFactory newSelectedFactory = playerManager.challengeFactories[(int)tempFactoryIndex.y].list[(int)tempFactoryIndex.x];

                    //Check if the new factory CAN be selected
                    if (FactorySelectable(newSelectedFactory))
                    {
                        MoveSelection(tempFactoryIndex);
                    }
                    else
                    {
                        //If the next factory is unselectable, try and jump one and check if that is selectable, if yes, move there
                        tempFactoryIndex += inputDir;
                        if (PositionWithingGridBounds(tempFactoryIndex))
                        {
                            newSelectedFactory = playerManager.challengeFactories[(int)tempFactoryIndex.y].list[(int)tempFactoryIndex.x];
                            if (FactorySelectable(newSelectedFactory))
                            {
                                MoveSelection(tempFactoryIndex);
                            }
                        }
                    }
                }

            }
        }
    }

    private void MoveSelection(Vector2 gridPos)
    {
        factoryIndex = gridPos;
        playerManager.UpdateSelectedFactory(factoryIndex);

        //DIRRRTTTYYYY but its fine for this kind of "DLC" Script I guess?
        selectionSprite.transform.position = playerManager.challengeFactories[(int)factoryIndex.y].list[(int)factoryIndex.x].transform.position;
    }

    private bool PositionWithingGridBounds(Vector2 input)
    {
        //print("Input x " + input.x + " y " + input.y);
        if (input.y < 0 || input.y >= playerManager.challengeFactories.Count) return false;
        if (input.x < 0 || input.x >= playerManager.challengeFactories[(int)input.y].list.Count) return false;


        return true;
    }

    private bool FactorySelectable(ChallengeFactory challengeFactory)
    {
        if (challengeFactory.shapeBuilder.selectState == SelectState.UNSELECTABLE) return false;
        if (challengeFactory.shapeBuilder.IsSelected()) return false;

        return true;
    }

    //set x and y of Input to either -1,0 or 1
    private Vector2 ParseInput(Vector2 input)
    {
        Vector2 inputDir = input;
        //set x and y of Input to either -1,0 or 1
        if (inputDir.x > 0) inputDir.x = 1;
        else if (inputDir.x < 0) inputDir.x = -1;
        else inputDir.x = 0;

        if (inputDir.y > 0) inputDir.y = 1;
        else if (inputDir.y < 0) inputDir.y = -1;
        else inputDir.y = 0;

        return inputDir;
    }
}
