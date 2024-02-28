using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class PlayerFactory : ShapeFactory
{
    
    public ScoreManager scoreManager;

    public int playerNum;
    
    //Update so far only used for Player Input
    private void Update() 
    {
        //NumbersPressed is expected to only contain one number most of the time, 
        //however by pressing two inputs at the same time a shape can be reset
        int[] numbersPressed = GetInputNumber();
        //Add a line when one input was given this frame
        if(numbersPressed.Length == 1){
            //print("Number pressed: " + numPressed);
            switch (state)
            {
                case BuildingState.WAITFORNUMBER:
                    //adjust number to number of sides, starts at 2 sides (E represents 0 without adjustment)
                    int numberOfSides = numbersPressed[0] + 2;

                    //WE ARE FORCING TWO SIDES FOR THE TIME BEING
                    //TODO CREATE CHALLENGE SCALING
                    //shapeBuilder.InitializeShape(false, 2);
                    state = BuildingState.BUILDING;
                    break;
                case BuildingState.BUILDING:
                
                    shapeBuilder.InitializeShape(false, 2);

                    //Adds a line and checks if shape is finished
                    if(shapeBuilder.AddLine(numbersPressed[0])){
                        state = BuildingState.FINISHED;
                        scoreManager.PlayerFinishedShape(shapeBuilder.GetShapecode());
                    };
                    break;
                    //In Finished state do not do anything. this is when the shape is being animated
            }
        }
        //Reset the shape when two buttons or more are pressed simultaneously
        else if (numbersPressed.Length > 1 && state != BuildingState.FINISHED)
        {
            ResetFactory();
        }   
    
    }
//Returns the number Input pressed by the player (out int) and how many inputs there were (normal function return)
    private int[] GetInputNumber(){
        List<int> AmountKeysPressedThisFrame = new List<int>();
        int i = 0;
        
        if(Input.GetButtonDown("P" + playerNum + "L1")){
            AmountKeysPressedThisFrame.Add(0);
            i += 1;
        }

        if(Input.GetButtonDown("P" + playerNum + "L2")){
            AmountKeysPressedThisFrame.Add(1);
            i += 1;
        }

        if(Input.GetButtonDown("P" + playerNum + "L3")){
            AmountKeysPressedThisFrame.Add(2);
            i += 1;
        }

        if(Input.GetButtonDown("P" + playerNum + "L4")){
            AmountKeysPressedThisFrame.Add(3);
            i += 1;
        }

        if(Input.GetButtonDown("P" + playerNum + "L5")){
            AmountKeysPressedThisFrame.Add(4);
            i += 1;
        }

        if(Input.GetButtonDown("P" + playerNum + "L6")){
            AmountKeysPressedThisFrame.Add(5);
            i += 1;
        }
/*
        //When a key has been pressed this frame, check for double input to remove shape
        if (i > 0)
        {
            //if so, add it to the input array
            foreach (KeyCode key in InputKeys)
            {
                int keyIndex = System.Array.IndexOf(InputKeys, key);
                if (Input.GetKey(key) && !AmountKeysPressedThisFrame.Contains(keyIndex))
                {
                    AmountKeysPressedThisFrame.Add(keyIndex);
                    i += 1;
                }
            }
        }*/

        return AmountKeysPressedThisFrame.ToArray();
    }

    //Reset only needed for player, used to be on the shapefactory but moved for safetyreasons (why have it higher when that can only cause trouble?)
    public void ResetFactory(){
        shapeBuilder.ResetShape();
        state = BuildingState.BUILDING;
    }
}
