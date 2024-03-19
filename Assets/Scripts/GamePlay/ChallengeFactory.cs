using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;

public class ChallengeFactory : ShapeFactory
{
    public string factoryName;
    public bool showLocalCombo;
    [Description("Drag all objects in here which are part of the visual \"locking\" of a challenge")]
    public GameObject[] visualChallengeLocks;
    public TextMeshProUGUI localComboText;
    public TextMeshProUGUI shapesNeededForUnlockText;

    [HideInInspector]
    public int localCombo = 1;
    public int shapesNeededForUnlockStart = 3;
    public int shapesNeededForUnlock;
    public Vector2 gridPosition;

    [Range(2, 100)]
    public int maxFacesFloorMIN;

    public int shapeNumSides;
    public int shapeNumSidesScaling = 3;

    private GameObject movingShape;

    //Returns code of shape created by challenge
    public void CreateChallenge()
    {
        //If maxFacesFloorMIN is greater than maxAllowedFaces, set maxAllowedFaces to maxFacesFloorMIN
        //Used so that Factory Gamemode can use "Layers" which have increasing difficulty but also higher reward
        shapeNumSides = shapeNumSides <= maxFacesFloorMIN ? maxFacesFloorMIN : shapeNumSides;

        shapeBuilder.InitializeShape(true, shapeNumSides);
    }

    public void SuccessfullShape()
    {
        localCombo += 1;
        shapeNumSides = maxFacesFloorMIN + (int)Math.Floor(localCombo / (float)shapeNumSidesScaling);
        UpdateUI();
    }

    public void FailedShape()
    {
        localCombo = 0;
        shapeNumSides = maxFacesFloorMIN;

        UpdateUI();
    }

    public void ResetCF()
    {
        Destroy(movingShape);

        localCombo = 0;
        shapeNumSides = maxFacesFloorMIN;

        bool showLockNumber = gridPosition.y == 1 ? true : false;
        shapesNeededForUnlock = shapesNeededForUnlockStart;
        SetSelectableState(false, showLockNumber);

        UpdateUI();
        CreateChallenge();
    }

    /// <summary>
    /// Reduces the number of shapes needed to unlock the challenge by 1.
    /// </summary>
    /// <returns> returns whether this shape was unlocked </returns>
    public bool ReduceNeededShapesUntilUnlock()
    {
        shapesNeededForUnlock -= 1;
        if (shapesNeededForUnlock <= 0)
        {
            SetSelectableState(true);
            return true;
        }
        UpdateUI();
        return false;
    }

    /// <summary>
    /// Sets the selectable state of the challenge factory. toggling the visibility of the lock and other relevant UI elements.
    /// </summary>
    /// <param name="selectable">Sets whether the Shape should be made selectable</param>
    /// <param name="hideLockNumber">default(true) shows the lock number, false hides the number needed until lock dissolves</param>
    public void SetSelectableState(bool selectable, bool hideLockNumber = true)
    {
        if (selectable || gridPosition.y == 0)
        {
            shapeBuilder.selectState = shapeBuilder.selectState == SelectState.SELECTED ? SelectState.SELECTED : SelectState.UNSELECTED;
            localComboText.enabled = showLocalCombo;
            shapesNeededForUnlockText.enabled = false;

            //Hide all visual lock objects
            foreach (GameObject lockObject in visualChallengeLocks)
            {
                lockObject.SetActive(false);
            }
        }
        else
        {
            //Set shape to unselectable
            if (gridPosition.y != 0)
            {
                shapeBuilder.selectState = SelectState.UNSELECTABLE;
            }
            else if (gridPosition.y == 0)
            {
                shapeBuilder.selectState = SelectState.UNSELECTED;
            }
            else
            {
                Debug.LogError("Error in ChallengeFactory.cs: SetSelectableState() with gridPosition.y = " + gridPosition.y);
            }

            //hide local combo and show shapes needed for unlock
            localComboText.enabled = false;
            shapesNeededForUnlockText.enabled = hideLockNumber;

            //Unhide all visual lock objects
            foreach (GameObject lockObject in visualChallengeLocks)
            {
                lockObject.SetActive(true);
            }
        }
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
        while (distance > 0.1f && flyingShape != null)
        {
            flyingShape.transform.position += direction * Time.deltaTime * 5f;
            distance = Vector3.Distance(flyingShape.transform.position, challengeShapePosition);
            //print(distance);
            yield return null;
        }

        if (flyingShape != null)
        {
            //Destroy cloned shape when it arrives (and not destroyed yet by game reset)
            Destroy(flyingShape.gameObject);
        }

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

        //Update shape selection status
        if (shapeBuilder.selectState == SelectState.LOCKED)
        {
            shapeBuilder.selectState = SelectState.UNSELECTED;
        }
        else if (shapeBuilder.selectState == SelectState.LOCKEDSELECTED)
        {
            shapeBuilder.selectState = SelectState.SELECTED;
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
    }

    private void UpdateUI()
    {
        localComboText.text = localCombo.ToString();
        shapesNeededForUnlockText.text = shapesNeededForUnlock.ToString();
    }
}
