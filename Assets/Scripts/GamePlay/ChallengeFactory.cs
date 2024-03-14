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

    private GameObject movingShape;

    private void Start()
    {
        print("Challenge Factory Start");
        maxAllowedFaces = maxFacesFloorMIN;
        localComboText.enabled = showLocalCombo;
    }

    //Returns code of shape created by challenge
    public void CreateChallenge()
    {
        //If maxFacesFloorMIN is greater than maxAllowedFaces, set maxAllowedFaces to maxFacesFloorMIN
        //Used so that Factory Gamemode can use "Layers" which have increasing difficulty but also higher reward
        maxAllowedFaces = maxAllowedFaces <= maxFacesFloorMIN ? maxFacesFloorMIN : maxAllowedFaces;

        shapeBuilder.InitializeShape(true, maxAllowedFaces);
    }

    public void SuccessfullShape()
    {
        localCombo += 1;
        UpdateUI();
    }

    public void FailedShape()
    {
        localCombo = 0;

        UpdateUI();
    }

    public void ResetCF()
    {
        Destroy(movingShape);


        localCombo = 0;
        maxAllowedFaces = maxFacesFloorMIN;
        UpdateUI();
        CreateChallenge();
    }

    //Clones a shape and moves that to the challenge shape
    public IEnumerator MoveShapeToChallenge(CustomShapeBuilder shapeToClone, bool isCorrectShape)
    {
        //Clone player shape
        CustomShapeBuilder flyingShape = Instantiate(shapeToClone, shapeToClone.transform.position, shapeToClone.transform.rotation);
        flyingShape.transform.localScale = new Vector3(1.7f, 1.7f, 1);

        movingShape = flyingShape.gameObject;

        // Get the player shape and challenge shape positions
        Vector3 playerShapePosition = flyingShape.transform.position;
        Vector3 challengeShapePosition = transform.position;

        // Calculate the distance and direction between the cloned shape and challenge shape
        Vector3 direction = (challengeShapePosition - playerShapePosition).normalized;
        float distance = Vector3.Distance(playerShapePosition, challengeShapePosition);

        // Move the player shape towards the challenge shape
        while (distance > 0.1f)
        {
            flyingShape.transform.position += direction * Time.deltaTime * 5f;
            distance = Vector3.Distance(flyingShape.transform.position, challengeShapePosition);
            //print(distance);
            yield return null;
        }

        //Destroy cloned shape when it arrives
        Destroy(flyingShape.gameObject);
        shapeBuilder.sap.playShapeFinished(isCorrectShape);

        //If the shape is wrong, jiggle it left and right
        if (!isCorrectShape)
        {
            float time = 0.5f;
            float counter = 0f;
            Vector3 originalPos = shapeBuilder.transform.position;
            while (counter < time)
            {
                counter += Time.deltaTime;
                shapeBuilder.transform.position = originalPos + new Vector3(Mathf.Sin(counter * 20f) * 0.2f, 0, 0);
                yield return null;
            }
            shapeBuilder.transform.position = originalPos;
        }

        //Create new challenge if code was correct
        if (isCorrectShape)
        {
            CreateChallenge();
        }
        else
        {
            if (shapeBuilder.GetShapecode().Length > maxFacesFloorMIN)
            {
                shapeBuilder.InitializeShape(true, maxFacesFloorMIN);
            }
        }

        if (shapeBuilder.selectState == SelectState.LOCKED)
        {
            shapeBuilder.selectState = SelectState.UNSELECTED;
        }
        else if (shapeBuilder.selectState == SelectState.LOCKEDSELECTED)
        {
            shapeBuilder.selectState = SelectState.SELECTED;
        }
    }

    private void UpdateUI()
    {
        localComboText.text = localCombo.ToString(); ;
    }
}
