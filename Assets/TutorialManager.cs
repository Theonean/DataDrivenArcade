using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using UnityEngine.InputSystem;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private InputActionReference navigateTextAction;
    [SerializeField] private InputActionReference quitTutorialAction;
    [SerializeField] private FadeElementInOut pauseGroupFader;
    [SerializeField] private CanvasGroup tutorialCanvasGroup;
    [SerializeField] private List<string> tutorialTexts;
    [SerializeField] private GameObject[] tutorialHighlightObjects;
    [SerializeField] private GameObject highlightObjectMask;
    [SerializeField] private GameObject henry;
    private int currentTextIndex = 0;
    private PlayerManager playerManager;

    //Tutorial completion management and achievement
    private const int shapesToMakeForAchievement = 3;
    private int shapesCompleted = 0;
    private bool finishedTutorial = false;
    private string playerChallengeMessage = "You've completed the tutorial! Now make " + shapesToMakeForAchievement + " shapes so we know you're ready!";
    private int playerChallengeIndex;

    //animated text and Henry
    private Coroutine scrollTextCoroutine;
    private Coroutine animatedHenryRoutine;
    private Vector3 henryOriginalPosition;
    private Quaternion henryOriginalRotation;
    private bool isScrolling = false;
    private int charactersPerSecond = 20;

    private void Awake()
    {
        highlightObjectMask.SetActive(false);
        henryOriginalPosition = henry.transform.position;
        henryOriginalRotation = henry.transform.rotation;

        scrollTextCoroutine = StartCoroutine(ScrollShowText());

        playerManager = FindObjectOfType<PlayerManager>();
        playerManager.OnFinishedShape.AddListener(PlayerFinishedShape);

        playerChallengeIndex = tutorialTexts.Count - 2;
    }

    private void Start()
    {
        tutorialTexts[playerChallengeIndex] = playerChallengeMessage;
    }

    private void OnEnable()
    {
        navigateTextAction.action.Enable();
        quitTutorialAction.action.Enable();

        navigateTextAction.action.performed += NavigateText;
        quitTutorialAction.action.performed += OnQuitTutorial;
    }

    private void OnDisable()
    {
        navigateTextAction.action.Disable();

        navigateTextAction.action.performed -= NavigateText;
        quitTutorialAction.action.performed -= OnQuitTutorial;
    }

    private void OnQuitTutorial(InputAction.CallbackContext context)
    {
        pauseGroupFader.ToggleFadeElement();

        FindObjectOfType<PlayerInput>().enabled = pauseGroupFader.isFadingOut; //WORKAROUND: Fixes Bug where PlayerInput Component makes UI not react to mouse-over
    }

    #region Text Management

    private void NavigateText(InputAction.CallbackContext context)
    {
        //Protect from animation jumping around when irrelevant input is happening
        if (context.ReadValue<Vector2>().y != 0 && context.ReadValue<Vector2>().x == 0)
            return;


        if (finishedTutorial)
            return;

        if (context.ReadValue<Vector2>().x != 0 && isScrolling)
        {
            SkipScrollingText(context);
            return;
        }
        else if (context.ReadValue<Vector2>().x > 0)
        {
            //Navigate right
            if (currentTextIndex < playerChallengeIndex || finishedTutorial) //prevent skipping to the last text / the challenge part of the tutorial before completion
            {
                currentTextIndex = Mathf.Min(currentTextIndex + 1, tutorialTexts.Count - 1);

            }
        }
        else if (context.ReadValue<Vector2>().x < 0)
        {
            //Navigate left
            if (currentTextIndex > 0)
            {
                currentTextIndex--;
            }
        }

        DisplayText();
    }

    private void DisplayText(bool SkipScrollingText = false)
    {
        if (SkipScrollingText)
        {
            TextMeshProUGUI text = tutorialCanvasGroup.GetComponentInChildren<TextMeshProUGUI>();
            text.text = tutorialTexts[currentTextIndex];
            isScrolling = false;
        }
        else
        {
            scrollTextCoroutine = StartCoroutine(ScrollShowText());
        }

        if (currentTextIndex < tutorialHighlightObjects.Length && tutorialHighlightObjects[currentTextIndex] != null)
        {
            highlightObjectMask.SetActive(true);
            highlightObjectMask.transform.position = tutorialHighlightObjects[currentTextIndex].transform.position;
        }
        else
        {
            highlightObjectMask.SetActive(false);
        }
    }

    private void SkipScrollingText(InputAction.CallbackContext context)
    {
        StopCoroutine(scrollTextCoroutine);
        TextMeshProUGUI text = tutorialCanvasGroup.GetComponentInChildren<TextMeshProUGUI>();
        text.text = tutorialTexts[currentTextIndex];
        isScrolling = false;
    }
    #endregion

    #region Tutorial Completion

    public void GiveTutorialAchievement()
    {
        if (SteamManager.Initialized)
        {
            SteamStatsAndAchievements.Instance.FinishedTutorial();
        }
        else
        {
            Debug.LogWarning("SteamManager not initialized, cannot give achievement");
        }
    }

    private void PlayerFinishedShape(string shapeCode)
    {
        if (finishedTutorial)
            return;


        if (currentTextIndex == playerChallengeIndex)
        {
            shapesCompleted++;
            playerChallengeMessage = "You've completed the tutorial! Now make " + Mathf.Max(shapesToMakeForAchievement - shapesCompleted, 0) + " shapes so we know you're ready!";
            tutorialTexts[currentTextIndex] = playerChallengeMessage;
            if (shapesCompleted == shapesToMakeForAchievement)
            {
                finishedTutorial = true;
                GiveTutorialAchievement();
                currentTextIndex++;
                tutorialHighlightObjects[currentTextIndex].SetActive(true);
                DisplayText(false);

                StartCoroutine(HideHighlightMaskAfterDelay());
            }
            else
            {
                DisplayText(true);
            }
        }
    }

    private IEnumerator HideHighlightMaskAfterDelay()
    {
        yield return new WaitForSeconds(5);
        highlightObjectMask.SetActive(false);
    }


    #endregion
    #region Animation

    private IEnumerator ScrollShowText()
    {
        if (isScrolling)
        {
            StopCoroutine(scrollTextCoroutine);
            isScrolling = false;
        }

        //Allow time for Henry to finish his animation and properly exit the coroutine
        yield return null;

        isScrolling = true;
        animatedHenryRoutine = StartCoroutine(ShakeAndRotateHenryWhileSpeaking());

        TextMeshProUGUI text = tutorialCanvasGroup.GetComponentInChildren<TextMeshProUGUI>();
        string fullText = tutorialTexts[currentTextIndex];
        text.text = "";

        for (int i = 0; i < fullText.Length; i++)
        {
            text.text += fullText[i];
            yield return new WaitForSeconds(1f / charactersPerSecond);
        }
        isScrolling = false;
    }

    public IEnumerator ShakeAndRotateHenryWhileSpeaking()
    {
        henry.transform.position = henryOriginalPosition;
        henry.transform.rotation = henryOriginalRotation;

        float jumpHeight = 0.25f;
        float jumpSpeed = 4f;
        float rotationAngle = 20f;
        float rotationSpeed = 3f;

        float time = 0;

        while (isScrolling)
        {
            henry.transform.position = henryOriginalPosition + new Vector3(0, Mathf.Sin(Time.time * jumpSpeed) * jumpHeight, 0);
            henry.transform.rotation = henryOriginalRotation * Quaternion.Euler(0, 0, Mathf.Sin(Time.time * rotationSpeed) * rotationAngle);

            time += Time.deltaTime;
            yield return null;
        }

        henry.transform.position = henryOriginalPosition;
        henry.transform.rotation = henryOriginalRotation;
    }
    #endregion
}
