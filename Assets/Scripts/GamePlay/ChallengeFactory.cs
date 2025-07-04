using System.Collections;
using System.ComponentModel;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ChallengeFactory : ShapeFactory //Remove the dependency on shapefactory
{
    public string factoryName;
    public bool showLocalCombo;
    [SerializeField] GameObject shapePlatform;
    [Description("Drag all objects in here which are part of the visual \"locking\" of a challenge")]
    public GameObject[] visualChallengeLocks;

    [HideInInspector]
    public int localCombo = 1;
    public int shapesNeededForUnlockStart = 3;
    public int shapesNeededForUnlock;
    public Vector2 gridPosition;

    [Range(1, 100)]
    public int maxFacesFloorMIN;

    public int shapeNumSides;

    public GameObject movingShape;
    public bool shapeTeleports = false;
    public bool shapeSameSpeed = false;
    public ParticleSystem[] shapeArrivedCorrectSystem = new ParticleSystem[2];
    private Vector3 cameraStartPos = new Vector3(0, 0, -20);

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

        shapeBuilder.InitializeShape(true, shapeNumSides);
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
        //print("Moving shape to challenge");
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

        //Set from challengemanager on creation from gamemodesettings
        float flySpeed = shapeSameSpeed ? 5f : 5f / shapeBuilder.GetShapecode().Length;
        ParticleSystem myShapeArrivedCorrectSystem = player.playerNum == 1 ? shapeArrivedCorrectSystem[0] : shapeArrivedCorrectSystem[1];

        if (shapeTeleports)
        {
            flyingShape.transform.position = challengeShapePosition;
            yield return null;
        }
        else
        {
            // Move the player shape towards the challenge shape
            while (distance > 0.1f && flyingShape != null)
            {
                flyingShape.transform.position += direction * Time.deltaTime * flySpeed;
                myShapeArrivedCorrectSystem.transform.position = flyingShape.transform.position;
                distance = Vector3.Distance(flyingShape.transform.position, challengeShapePosition);
                //print(distance);
                yield return null;
            }
        }

        //finalize shape arrival at CF and signal player
        if (flyingShape != null)
        {
            //Destroy cloned shape when it arrives (and not destroyed yet by game reset)
            Destroy(flyingShape.gameObject);

            bool isCorrectShape = playerShapeCode == shapeBuilder.GetShapecode();
            //print("Shape arrived and code is correct: " + isCorrectShape + " after comparing codes: " + playerShapeCode + " to " + shapeBuilder.GetShapecode());

            //DROP SHAPE ONTO PLATTFORM BELOW SEQUENCE
            shapePlatform.GetComponent<Animator>().Play("LetShapeFallOntoBelt");

            //Prevent lines blinking while falling down
            shapeBuilder.EndLineHighlight(true);
            shapeBuilder.InitializeShape(true, shapeNumSides, playerShapeCode, LineState.REGULAR);

            //Short sequence where the built shape gets dropped onto belt below
            //To achieve this, the shape is scaled down over a short period of time and then properly reset
            //lerp scale from 1 to 0.25 depending on how close shapeBuilder.radius is to 1.7 from starting point 1
            float time = 0.4f;
            float counter = 0f;
            Vector3 originalScale = shapeBuilder.transform.localScale;
            float targetScaleMultiplier = Mathf.Lerp(1, 0.25f, shapeBuilder.radius / shapeBuilder.maxRadius);
            while (counter < time)
            {
                counter += Time.deltaTime;
                shapeBuilder.transform.localScale = Vector3.Lerp(originalScale, originalScale * targetScaleMultiplier, counter / time);
                yield return null;
            }
            shapeBuilder.transform.localScale = originalScale; //Reset original scale after dropping shape

            //Create new challenge if code was correct
            if (isCorrectShape)
            {
                localCombo = Mathf.Max(0, localCombo + 1);
                shapeNumSides = maxFacesFloorMIN + localCombo;

                shapeBuilder.InitializeShape(true, shapeNumSides);
                shapeBuilder.StartLineHighlight(player.playerNum, 0); //Start Highlighting again


                myShapeArrivedCorrectSystem.Play();
            }
            else
            {
                localCombo = Mathf.Max(0, localCombo - 1);
                shapeNumSides = maxFacesFloorMIN + localCombo;

                shapeBuilder.InitializeShape(true, shapeNumSides);
                shapeBuilder.StartLineHighlight(player.playerNum, 0); //Start Highlighting again
            }

            //Create some camera screenshake in a coroutine
            StartCoroutine(ShakeCamera());

            shapeBuilder.sap.playShapeFinished(isCorrectShape, player.GetCombo());

            //If the shape is wrong, jiggle it left and right
            if (!isCorrectShape)
            {
                time = 0.5f;
                counter = 0f;
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
        //If flyingShape already destroyed, play a destruction sound and particle effect for flair
        else
        {
            myShapeArrivedCorrectSystem.Play();
            shapeBuilder.sap.playShapeFinished(false, 0);
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

    private IEnumerator ShakeCamera()
    {
        float time = 0.5f;
        float counter = 0f;
        float frequency = 0.02f;
        float amplitude = 20f;

        while (counter < time)
        {
            counter += Time.deltaTime;
            Camera.main.transform.position = cameraStartPos + new Vector3(Mathf.Sin(counter * amplitude) * frequency, Mathf.Sin(counter * amplitude) * frequency, 0);
            yield return null;
        }
        Camera.main.transform.position = cameraStartPos;
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
}
