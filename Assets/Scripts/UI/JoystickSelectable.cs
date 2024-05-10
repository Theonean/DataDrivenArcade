using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.ComponentModel;
using TMPro;
using Unity.VisualScripting;
using System;

public enum ActionOnSelection
{
    EventOnly,
    ScaleAndColour,
    ShowHighlight
}
[SerializeField]
public enum SelectionState
{
    Unselected,
    Selected,
    SelectedInputLocked,
}

public class JoystickSelectable : MonoBehaviour
{

    [Header("Settings")]
    public ActionOnSelection animationOnSelection;
    [Description("The Name of the Action that is triggered when the selection animation is finished")]
    public string actionType;
    private SelectionState selectionState = SelectionState.Unselected;
    public int controlledByPlayer;

    [Header("Events")]
    public UnityEvent SelectedEvent;
    public UnityEvent DeselectedEvent;
    public UnityEvent<int, string> SelectionChangeEvent;
    public UnityEvent MoveSelectionUpEvent;
    public UnityEvent MoveSelectionDownEvent;
    public UnityEvent MoveSelectionLeftEvent;
    public UnityEvent MoveSelectionRightEvent;
    private AnimationCurve scaleUpDownCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    //Scale and Colour Selection Type
    private Image objectToColour;
    private TextMeshProUGUI objectToScale;
    private Vector3 scaleObjectOriginalScale;
    private Coroutine scaleCoroutine;

    [Header("Highlight Selection Type")]
    public Sprite highlightSprite;
    public Vector2 highlightSize;
    public Vector2 highlightOffset;
    private SpriteRenderer highlight;

    //Other variables
    private GameManager gm;

    private void Awake()
    {
        gm = GameManager.instance;

        objectToColour = GetComponent<Image>();
        objectToScale = GetComponentInChildren<TextMeshProUGUI>();

        if (animationOnSelection == ActionOnSelection.ShowHighlight)
        {
            //Create new gameobject with sprite renderer and add it under this
            GameObject highlightObject = new GameObject("Highlight");
            highlightObject.transform.SetParent(transform);
            highlightObject.transform.localScale = highlightSize;
            highlightObject.transform.localPosition = new Vector3(0, 0, -1) + new Vector3(highlightOffset.x, highlightOffset.y);

            highlight = highlightObject.AddComponent<SpriteRenderer>();
            highlight.sprite = highlightSprite;
            highlight.color = controlledByPlayer == 1 ? Color.red : Color.blue;
            highlight.color = new Color(highlight.color.r, highlight.color.g, highlight.color.b, 0.4f);
            highlight.enabled = false;
        }
        else if (animationOnSelection == ActionOnSelection.ScaleAndColour)
        {
            scaleObjectOriginalScale = objectToScale.transform.localScale;
        }
    }

    public void Selected()
    {
        SelectedEvent?.Invoke();
        SelectionChangeEvent?.Invoke(controlledByPlayer, "");
        gm.JoystickInputEvent.AddListener(MoveSelection);
        selectionState = SelectionState.Selected;
        switch (animationOnSelection)
        {
            case ActionOnSelection.ScaleAndColour:
                if (gameObject.activeInHierarchy) // Check if the GameObject is active in the hierarchy
                {
                    if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
                    scaleCoroutine = StartCoroutine(ConfirmSelected(true));
                    StartCoroutine(ScaleUpDown());
                }
                break;
            case ActionOnSelection.ShowHighlight:
                highlight.enabled = true;
                break;
            default:
                break;
        }
    }

    public void ToggleInputLock()
    {
        if (selectionState == SelectionState.Selected)
        {
            selectionState = SelectionState.SelectedInputLocked;
        }
        else if (selectionState == SelectionState.SelectedInputLocked)
        {
            selectionState = SelectionState.Selected;
        }
    }

    private void MoveSelection(InputData iData)
    {
        if ((iData.playerNum == controlledByPlayer || gm.singlePlayer) && selectionState != SelectionState.SelectedInputLocked)
        {
            UnityEvent directionEvent = null;
            iData.joystickDirection.x = gm.singlePlayer && controlledByPlayer != 1 ? iData.joystickDirection.x * -1 : iData.joystickDirection.x; //If in singleplayer mirror movement
            if (iData.joystickDirection.y < 0 && MoveSelectionUpEvent.GetPersistentEventCount() > 0)
            {
                directionEvent = MoveSelectionUpEvent;
            }
            else if (iData.joystickDirection.y > 0 && MoveSelectionDownEvent.GetPersistentEventCount() > 0)
            {
                directionEvent = MoveSelectionDownEvent;
            }
            else if (iData.joystickDirection.x < 0 && MoveSelectionLeftEvent.GetPersistentEventCount() > 0)
            {
                directionEvent = MoveSelectionLeftEvent;
            }
            else if (iData.joystickDirection.x > 0 && MoveSelectionRightEvent.GetPersistentEventCount() > 0)
            {
                directionEvent = MoveSelectionRightEvent;
            }

            if (directionEvent.IsUnityNull()) return;
            directionEvent?.Invoke();

            //If the called event has a selected method (meaning we move selection) deselect current Selectable
            if (CheckEventHasMethod(directionEvent, "Selected"))
                Deselected();
        }
    }

    // Checks if the given UnityEvent has any listener named "Selected". If so, invoke the event and return true.
    private bool CheckEventHasMethod(UnityEvent unityEvent, string methodName)
    {
        bool hasSelectedMethod = false;
        for (int i = 0; i < unityEvent.GetPersistentEventCount(); i++)
        {
            if (unityEvent.GetPersistentMethodName(i) == methodName)
            {
                hasSelectedMethod = true;
                break; // Exit the loop once we found a match.
            }
        }
        return hasSelectedMethod;
    }

    public void Deselected()
    {
        //print("Deselecteed");
        gm.JoystickInputEvent.RemoveListener(MoveSelection);
        DeselectedEvent?.Invoke();
        SelectionChangeEvent?.Invoke(controlledByPlayer, "Empty");
        selectionState = SelectionState.Unselected;

        switch (animationOnSelection)
        {
            case ActionOnSelection.ScaleAndColour:
                if (gameObject.activeInHierarchy) // Check if the GameObject is active in the hierarchy
                {
                    if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
                    scaleCoroutine = StartCoroutine(ConfirmSelected(false));
                }
                else
                {
                    objectToColour.color = Color.white;
                    objectToScale.transform.localScale = scaleObjectOriginalScale;
                }
                break;
            case ActionOnSelection.ShowHighlight:
                highlight.enabled = false;
                break;
            default:
                break;
        }
    }

    private IEnumerator ConfirmSelected(bool scalingDown)
    {
        Vector3 targetScale = scalingDown ? Vector3.zero : scaleObjectOriginalScale;
        if (!scalingDown) objectToColour.color = Color.white;
        float scaleSpeed = 0.1f;

        while (Vector3.Distance(objectToScale.transform.localScale, targetScale) > 0.01f)
        {
            objectToScale.transform.localScale = Vector3.Lerp(objectToScale.transform.localScale, targetScale, scaleSpeed);
            yield return null;
        }

        if (scalingDown) objectToColour.color = Color.green;
        objectToScale.transform.localScale = scaleObjectOriginalScale;

        if (scalingDown && !actionType.Equals(String.Empty))
        {
            SelectionChangeEvent?.Invoke(controlledByPlayer, actionType);
        }
    }

    //Scales the selectable slightly up and down as an indication of action and to add some juice to the game
    private IEnumerator ScaleUpDown()
    {
        float animationTime = 0.4f;
        // Play a little animation which scales x and y up and then down again to the original value with a smooth curve
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.2f; // Scale up by 20%

        // Scale up
        float counter = 0;
        while (Vector3.Distance(transform.localScale, targetScale) > 0.01f)
        {
            counter += Time.deltaTime;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, scaleUpDownCurve.Evaluate(counter / animationTime));
            yield return null;
        }

        // Scale down
        counter = 0;
        while (Vector3.Distance(transform.localScale, originalScale) > 0.01f)
        {
            counter += Time.deltaTime;
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, scaleUpDownCurve.Evaluate(counter / animationTime));
            yield return null;
        }
    }

    private void OnDisable()
    {
        Deselected();
    }
}
