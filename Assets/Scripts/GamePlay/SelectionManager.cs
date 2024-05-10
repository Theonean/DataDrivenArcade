using System;
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

    private Vector2 inputDir;
    private Vector2 factoryIndex;
    private int playerNum;
    public PlayerManager playerManager;
    bool isClassic = true;
    bool active = false;
    bool scaleSet = false;

    public void Activate(Vector2 startIndex)
    {
        //Check if playerManager has  1 challenge factory
        if (GameManager.instance.gameModeData.gameMode == GameModeType.CLASSIC)
        {
            //Hide selector and disable movement input, because theres only one factory to select anyway
            selectionSprite.SetActive(false);
            isClassic = true;
        }
        else
        {
            playerNum = playerManager.playerNum;

            //Change selectionsprite colour to blue if playernum 2
            if (playerNum == 2)
            {
                selectionSprite.GetComponent<SpriteRenderer>().color = Color.blue;
                selectionSprite.GetComponent<SpriteRenderer>().color = new Color(Color.blue.r, Color.blue.g, Color.blue.b, 0.9f);
            }
            else
            {
                selectionSprite.GetComponent<SpriteRenderer>().color = Color.red;
                selectionSprite.GetComponent<SpriteRenderer>().color = new Color(Color.red.r, Color.red.g, Color.red.b, 0.9f);
            }

            factoryIndex = startIndex;

            selectionSprite.SetActive(true);
            MoveSelection(factoryIndex);

            //Workaround to make it not scale up each round when the game resets
            if (!scaleSet)
            {
                scaleSet = true;
                //Scale the selection to the scaling of the CF on the grid so it always looks the same size (because grid size can change depending on setting and game mode)
                ChallengeFactory cf = playerManager.challengeFactories[(int)startIndex.y].list[(int)startIndex.x];
                selectionSprite.transform.localScale = new Vector3(
                    selectionSprite.transform.localScale.x * cf.transform.localScale.x,
                    selectionSprite.transform.localScale.y * cf.transform.localScale.y,
                    selectionSprite.transform.localScale.z);
            }

            isClassic = false;
        }

        active = true;
        GameManager.instance.JoystickInputEvent.AddListener(JoystickInput);
    }

    public void Deactivate()
    {
        GameManager.instance.JoystickInputEvent.RemoveListener(JoystickInput);
        active = false;
        //Hide selector
        selectionSprite.SetActive(false);
        //print("Deactivating SelectionManager");
    }

    public void ResetSelection(Vector2 startIndex)
    {
        factoryIndex = startIndex;
        MoveSelection(factoryIndex);
    }

    // Update is called once per frame
    void JoystickInput(InputData iData)
    {
        if (!isClassic && active && iData.playerNum == playerNum)
        {
            //Save Input Direction
            inputDir = iData.joystickDirection;

            //print("Input: " + inputDir);

            //IDEA: what if selection loops around (so reaching rightborder will move you to the left side)

            //Safety check on inputdir to make sure it is within the bounds of the factories

            //Final check if inputDir is still 0,0 as we do not want the updateselectedfactory function to be called when theres no movement (inefficient)

            Vector2 tempFactoryIndex = factoryIndex + inputDir;
            if (CheckMoveValidity(tempFactoryIndex))
            {
                MoveSelection(tempFactoryIndex);
            }
            else
            {
                //If the next factory is unselectable, try and jump one and check if that is selectable, if yes, move there
                tempFactoryIndex += inputDir;
                if (CheckMoveValidity(tempFactoryIndex))
                {
                    MoveSelection(tempFactoryIndex);
                }
                else
                {
                    //If that still doesn't work, look at the direction (if going horizontal, try up), if going up, try left and right
                    tempFactoryIndex -= inputDir;

                    //If were on a horizontal move and we're blocked
                    if (inputDir.x != 0)
                    {
                        //Try going up
                        tempFactoryIndex.y -= 1;
                        if (CheckMoveValidity(tempFactoryIndex))
                        {
                            MoveSelection(tempFactoryIndex);
                        }
                        else
                        {
                            //Try going down
                            tempFactoryIndex.y += 2;
                            if (CheckMoveValidity(tempFactoryIndex))
                            {
                                MoveSelection(tempFactoryIndex);
                            }
                            else
                            {
                                //print("We tried it all (Horizontally), but we still can't move");
                            }
                        }
                    }
                    //If we're on a vertical move try going right and then try left
                    else if (inputDir.y != 0)
                    {
                        tempFactoryIndex.x += 1;
                        if (CheckMoveValidity(tempFactoryIndex))
                        {
                            MoveSelection(tempFactoryIndex);
                        }
                        else
                        {
                            tempFactoryIndex.x -= 2;
                            if (CheckMoveValidity(tempFactoryIndex))
                            {
                                MoveSelection(tempFactoryIndex);
                            }
                            else
                            {
                                //print("We tried it all (vertically), but we still can't move");
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
        float zPos = -0.01f;
        //DIRRRTTTYYYY but its fine for this kind of "DLC" Script I guess?
        selectionSprite.transform.position = playerManager.challengeFactories[(int)factoryIndex.y].list[(int)factoryIndex.x].transform.position;
        selectionSprite.transform.position = new Vector3(selectionSprite.transform.position.x, selectionSprite.transform.position.y, zPos);
    }

    private bool CheckMoveValidity(Vector2 gridPosition)
    {
        return PositionWithingGridBounds(gridPosition) && FactorySelectable(gridPosition);
    }

    private bool PositionWithingGridBounds(Vector2 gridPosition)
    {
        //print("Input x " + input.x + " y " + input.y);
        if (gridPosition.y < 0 || gridPosition.y >= playerManager.challengeFactories.Count) return false;
        if (gridPosition.x < 0 || gridPosition.x >= playerManager.challengeFactories[(int)gridPosition.y].list.Count) return false;


        return true;
    }

    private bool FactorySelectable(ChallengeFactory challengeFactory)
    {
        if (challengeFactory.shapeBuilder.selectState == SelectState.UNSELECTABLE) return false;
        if (challengeFactory.shapeBuilder.IsSelected()) return false;

        return true;
    }

    private bool FactorySelectable(Vector2 gridPosition)
    {
        ChallengeFactory cf = playerManager.challengeFactories[(int)gridPosition.y].list[(int)gridPosition.x];
        return FactorySelectable(cf);
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
