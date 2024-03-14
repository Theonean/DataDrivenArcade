using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class PlayerFactory : ShapeFactory
{

    //How should this class work?
    //Playerfactory is managed by the playermanager
    //It in itself doesn't do anything, it gets interacted with and controller by player and score manager

    //NOW OBSOLETE 

    public PlayerManager playerManager;

    private int playerNum;
    public bool beingAnimated;
    private void Start() {
        Debug.LogError("PlayerFactory is obsolete, WHY ARE YOU RUNNING");
    }

    //Reset only needed for player, used to be on the shapefactory but moved for safetyreasons (why have it higher when that can only cause trouble?)
    public void ResetFactory(InputData iData)
    {
        if (iData.playerNum == playerNum)
        {
            ResetFactory();
        }
    }

    //Reset only needed for player, used to be on the shapefactory but moved for safetyreasons (why have it higher when that can only cause trouble?)
    public void ResetFactory()
    {
        //print("Reseting playerfactory with faces: " + maxAllowedFaces);
        //shapeBuilder.InitializeShape(false, maxAllowedFaces); GUGUUUUUS, 
        //maxAllowedFaces is a misnomer variable that was used once but changed to shapeNumSides, this script is archived ANYWAY so I just commented it out
        
        shapeBuilder.DestroyLines();
        beingAnimated = false;

    }
}
