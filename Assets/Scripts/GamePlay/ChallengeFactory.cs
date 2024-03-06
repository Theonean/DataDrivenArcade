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

    public GameObject selectionHighlightPrefab;

    private bool selected = false;

    private void Start() {
        print("Challenge Factory Start");
        maxAllowedFaces = maxFacesFloorMIN;
        localComboText.enabled = showLocalCombo;
    }

    //Returns code of shape created by challenge
    public string CreateChallenge(){
        shapeBuilder.ResetShape();
        //If maxFacesFloorMIN is greater than maxAllowedFaces, set maxAllowedFaces to maxFacesFloorMIN
        //Used so that Factory Gamemode can use "Layers" which have increasing difficulty but also higher reward
        maxAllowedFaces = maxAllowedFaces <= maxFacesFloorMIN ? maxFacesFloorMIN : maxAllowedFaces;

        string shapeCode = shapeBuilder.InitializeShape(true, maxAllowedFaces);

        if(selected){
            print("Challenge Factory CreateChallenge and is selected");
            Selected();
        }

        return shapeCode;
    }

    public void SuccessfullShape(){
        localCombo += 1;
        UpdateUI();
    }

    public void FailedShape(){
        localCombo = 0;
        UpdateUI();
    }

    //When CF is Selected initiate the highlight of the first line of the customshapebuilder
    //Default value highlights the first line of the shape
    public void Selected(int lineIndex = 0, bool isHighlighted = true){
        print("Challenge Factory Selected line " + lineIndex + " isHighlighted " + isHighlighted);
        shapeBuilder.SetLineHighlight(lineIndex, isHighlighted);
        selected = isHighlighted;
    }

    public string ResetCF()
    {
        localCombo = 0;
        maxAllowedFaces = maxFacesFloorMIN;
        UpdateUI();
        string shapeCode = CreateChallenge();
        if(selected){
            Selected();
        }
        return shapeCode;
    }
    private void UpdateUI() {
        localComboText.text = localCombo.ToString();;
    }
}
