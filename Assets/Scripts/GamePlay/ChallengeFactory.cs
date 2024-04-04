using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using Unity.VisualScripting;
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
    public int shapeNumSidesScaling = 1;

    public GameObject movingShape;

    public void ResetCF()
    {
        Destroy(movingShape);

        //#UNITYBUG Stopcoroutine has to be called on the object it was started on
        if (!shapeBuilder.flashingRoutine.IsUnityNull()) shapeBuilder.StopCoroutine(shapeBuilder.flashingRoutine);

        localCombo = 0;
        shapeNumSides = maxFacesFloorMIN;

        bool showLockNumber = gridPosition.y == 1 ? true : false;
        shapesNeededForUnlock = shapesNeededForUnlockStart;
        SetSelectableState(false, showLockNumber);

        UpdateUI();

        shapeBuilder.InitializeShape(true, shapeNumSides);
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
    public IEnumerator MoveShapeToChallenge(PlayerManager player, string playerShapeCode)
    {
        print("Moving shape to challenge");
        //Clone player shape
        GameObject playerShapeObject = player.playerShape.gameObject;
        CustomShapeBuilder flyingShape = Instantiate(shapePrefab, playerShapeObject.transform.position, playerShapeObject.transform.rotation).GetComponent<CustomShapeBuilder>();
        flyingShape.InitializeShape(true, shapeNumSides, playerShapeCode);

        flyingShape.transform.localScale = new Vector3(1.7f, 1.7f, 1);

        movingShape = flyingShape.gameObject;

        // Get the player shape and challenge shape positions
        Vector3 playerShapePosition = flyingShape.transform.position;
        Vector3 challengeShapePosition = transform.position;

        // Calculate the distance and direction between the cloned shape and challenge shape
        Vector3 direction = (challengeShapePosition - playerShapePosition).normalized;
        float distance = Vector3.Distance(playerShapePosition, challengeShapePosition);

        float flySpeed = 5f / shapeBuilder.GetShapecode().Length;

        // Move the player shape towards the challenge shape
        while (distance > 0.1f && flyingShape != null)
        {
            flyingShape.transform.position += direction * Time.deltaTime * flySpeed;
            distance = Vector3.Distance(flyingShape.transform.position, challengeShapePosition);
            //print(distance);
            yield return null;
        }

        if (flyingShape != null)
        {
            //Destroy cloned shape when it arrives (and not destroyed yet by game reset)
            Destroy(flyingShape.gameObject);

            bool isCorrectShape = playerShapeCode == shapeBuilder.GetShapecode();
            print("Shape arrived and code is correct: " + isCorrectShape + " after comparing codes: " + playerShapeCode + " to " + shapeBuilder.GetShapecode());

            //Create new challenge if code was correct
            if (isCorrectShape)
            {
                localCombo += 1;
                shapeNumSides = maxFacesFloorMIN + localCombo;

                shapeBuilder.InitializeShape(true, shapeNumSides);
            }
            else
            {
                //If shapecode is longer than the number of sides, reset shapecode to maxFacesFloorMIN
                if (shapeBuilder.GetShapecode().Length > maxFacesFloorMIN)
                {
                    shapeBuilder.InitializeShape(true, maxFacesFloorMIN);
                }

                localCombo = 0;
                shapeNumSides = maxFacesFloorMIN;
            }

            UpdateUI();

            shapeBuilder.sap.playShapeFinished(isCorrectShape, player.GetCombo());

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

            player.ShapeArrived(isCorrectShape, this);
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
    }

    public void IncreaseShapeNumSides()
    {
        maxFacesFloorMIN += 1;
        shapeNumSides = maxFacesFloorMIN;
        if (movingShape != null) Destroy(movingShape);
        shapeBuilder.InitializeShape(true, maxFacesFloorMIN);
    }
    public void DecreaseShapeNumSides()
    {
        if (maxFacesFloorMIN > 1)
        {
            maxFacesFloorMIN -= 1;
            shapeNumSides = maxFacesFloorMIN;
            if (movingShape != null) Destroy(movingShape);
            shapeBuilder.InitializeShape(true, maxFacesFloorMIN);
        }
    }

    private void UpdateUI()
    {
        localComboText.text = localCombo.ToString();
        shapesNeededForUnlockText.text = shapesNeededForUnlock.ToString();
    }
}
