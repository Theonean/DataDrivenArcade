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
    public bool beingAnimated;

    //Update so far only used for Player Input
    private void Update()
    {
        //NumbersPressed is expected to only contain one number most of the time, 
        //however by pressing two inputs at the same time a shape can be reset
        int[] numbersPressed = gm.GetInputNumber(playerNum);
        if (!beingAnimated)
        {
            //Add a line when one input was given this frame
            if (numbersPressed.Length == 1)
            {
                //print("Number pressed: " + numPressed);
                //Adds a line and checks if shape is finished
                if (shapeBuilder.AddLine(numbersPressed[0]))
                {
                    beingAnimated = true;
                    scoreManager.PlayerFinishedShape(shapeBuilder.GetShapecode());
                    shapeBuilder.InitializeShape(false, maxAllowedFaces);
                }
            }
            //Reset the shape when two buttons or more are pressed simultaneously
            else if (numbersPressed.Length > 1)
            {
                ResetFactory();
            }
        }
    }

    //Reset only needed for player, used to be on the shapefactory but moved for safetyreasons (why have it higher when that can only cause trouble?)
    public void ResetFactory()
    {
        //print("Reseting playerfactory with faces: " + maxAllowedFaces);
        shapeBuilder.InitializeShape(false, maxAllowedFaces);
        shapeBuilder.ResetShape();
        beingAnimated = false;
    }
}
