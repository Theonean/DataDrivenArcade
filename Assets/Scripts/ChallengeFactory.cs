using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChallengeFactory : ShapeFactory
{
    public string factoryName;
    public bool showLocalCombo;
    public TextMeshProUGUI localComboText;
    
    [HideInInspector]
    public int localCombo = 1;

    [Range(2, 100)]
    public int maxFacesFloorMIN;

    private void Start() {
        print("Challenge Factory Start");
        maxAllowedFaces = maxFacesFloorMIN;
        localComboText.enabled = showLocalCombo;
        CreateChallenge();
    }

    //Returns code of shape created by challenge
    public string CreateChallenge(){
        shapeBuilder.ResetShape();
        //If maxFacesFloorMIN is greater than maxAllowedFaces, set maxAllowedFaces to maxFacesFloorMIN
        //Used so that Factory Gamemode can use "Layers" which have increasing difficulty but also higher reward
        maxAllowedFaces = maxAllowedFaces <= maxFacesFloorMIN ? maxFacesFloorMIN : maxAllowedFaces;

        return shapeBuilder.InitializeShape(true, maxAllowedFaces);
    }

    public void SuccessfullShape(){
        localCombo += 1;
        UpdateUI();
    }

    public void FailedShape(){
        localCombo = 1;
        UpdateUI();
    }

    private void UpdateUI() {
        localComboText.text = localCombo.ToString();;
    }
}
