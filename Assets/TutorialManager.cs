using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using UnityEngine.InputSystem;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private InputActionReference navigateTextAction;
    [SerializeField] private InputActionReference submitSkipTextScrollAction;
    [SerializeField] private CanvasGroup tutorialCanvasGroup;
    [SerializeField] private List<string> tutorialTexts;
    [SerializeField] private GameObject[] tutorialHighlightObjects;
    [SerializeField] private GameObject highlightObjectMask;
    [SerializeField] private GameObject henry;
    private int currentTextIndex = 0;

    //Variables for animated text and Henry
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
    }

    private void OnEnable()
    {
        navigateTextAction.action.Enable();
        submitSkipTextScrollAction.action.Enable();

        navigateTextAction.action.performed += NavigateText;
        submitSkipTextScrollAction.action.performed += SkipScrollingText;

    }

    private void OnDisable()
    {
        navigateTextAction.action.Disable();
        submitSkipTextScrollAction.action.Disable();

        navigateTextAction.action.performed -= NavigateText;
        submitSkipTextScrollAction.action.performed -= SkipScrollingText;
    }

    private void NavigateText(InputAction.CallbackContext context)
    {
        //Protect from animation jumping around when irrelevant input is happening
        if (context.ReadValue<Vector2>().y != 0 && context.ReadValue<Vector2>().x == 0)
        {
            return;
        }

        if (context.ReadValue<Vector2>().x > 0)
        {
            //Navigate right
            if (currentTextIndex < tutorialTexts.Count - 1)
            {
                currentTextIndex++;
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

        scrollTextCoroutine = StartCoroutine(ScrollShowText());

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

    private void SkipScrollingText(InputAction.CallbackContext context)
    {
        StopCoroutine(scrollTextCoroutine);
        TextMeshProUGUI text = tutorialCanvasGroup.GetComponentInChildren<TextMeshProUGUI>();
        text.text = tutorialTexts[currentTextIndex];
    }

    public void GiveTutorialAchievement()
    {
        if (SteamManager.Initialized)
        {
            SteamUserStats.SetAchievement("TUTORIAL_COMPLETE");
            SteamUserStats.StoreStats();
        }
        else
        {
            Debug.LogWarning("SteamManager not initialized, cannot give achievement");
        }
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
}
