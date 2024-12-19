using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneType
{
    //NEW SCENES; KEEP OLD ONES FOR NOW
    MAINMENU01,
    LEADERBOARD02,
    PLAYERAMOUNTSELECTION10,
    PLAYER1NAME11,
    PLAYER2NAME12,
    SELECTGAMEMODE20,
    GAME30
}

public class SceneHandler : MonoBehaviour
{
    public static SceneHandler Instance { get; private set; }
    public SceneType currentScene { get; private set; }
    private SceneType nextScene;
    [SerializeField] private SceneTransitions sceneTransitions;

    [SerializeField] private SpriteRenderer transitionOverlay;
    [SerializeField] private AnimationCurve fadeOutCurve;
    [SerializeField] private AnimationCurve fadeInCurve;

    private AsyncOperation asyncLoad;
    private bool leavingSceneLeft = false;
    private float horizontalMove = 9f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void GoToNextScene(int sceneIndexIncrement = 1)
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + sceneIndexIncrement;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            Instance.SwitchScene((SceneType)nextSceneIndex);
        }
        else
        {
            Debug.LogError("No next scene in build order!");
        }
    }

    public static void GoToLastScene(int sceneIndexDecrement = 1)
    {
        int lastSceneIndex = SceneManager.GetActiveScene().buildIndex - sceneIndexDecrement;

        //handle exceptions for scenes that are not in the build order first
        SceneType currentScene = (SceneType)SceneManager.GetActiveScene().buildIndex;
        if (currentScene == SceneType.SELECTGAMEMODE20)
        {
            if (GameManager.instance.singlePlayer)
            {
                lastSceneIndex = (int)SceneType.PLAYER1NAME11;
            }
            else
            {
                lastSceneIndex = (int)SceneType.PLAYER2NAME12;
            }
        }


        if (lastSceneIndex >= 0)
        {
            Instance.SwitchScene((SceneType)lastSceneIndex);
        }
        else
        {
            Debug.LogError("No last scene in build order!");
        }
    }

    public void SwitchScene(SceneType sceneType)
    {
        int targetBuildIndex = GetSceneBuildIndex(sceneType);
        nextScene = sceneType;

        if (targetBuildIndex == -1)
        {
            Debug.LogError($"Scene {sceneType} not found!");
            return;
        }

        leavingSceneLeft = targetBuildIndex < SceneManager.GetActiveScene().buildIndex;

        asyncLoad = SceneManager.LoadSceneAsync(targetBuildIndex);
        asyncLoad.allowSceneActivation = false;

        StartCoroutine(LoadSceneWithTransition());
    }

    private IEnumerator LoadSceneWithTransition()
    {
        yield return StartCoroutine(FadeSceneOut());

        asyncLoad.allowSceneActivation = true;

        yield return new WaitUntil(() => asyncLoad.isDone);

        //yield return null; // Wait for one frame after loading

        StartCoroutine(FadeSceneIn());
    }

    private SceneTransitionData GetTransitionData(SceneType sceneType)
    {
        return sceneTransitions.sceneTransitionsData.FirstOrDefault(x => x.sceneName == sceneType);
    }

    private IEnumerator FadeSceneOut()
    {
        float elapsedTime = 0f;
        float fadeDuration = 1.5f;
        Vector3 startPos = Camera.main.transform.position;
        SceneTransitionData transition = GetTransitionData(currentScene);
        Vector3 endPos = leavingSceneLeft ? new Vector3(transition.fadeInDirection.x * horizontalMove, transition.fadeInDirection.y * horizontalMove, Camera.main.transform.position.z) : new Vector3(transition.fadeOutDirection.x * horizontalMove, transition.fadeOutDirection.y * horizontalMove, Camera.main.transform.position.z);
        Color startColor = Color.clear;
        Color endColor = Color.clear;

        transitionOverlay.enabled = true;

        while (elapsedTime < fadeDuration || asyncLoad.progress < 0.9f)
        {
            float t = fadeOutCurve.Evaluate(elapsedTime / fadeDuration);

            Camera.main.transform.position = Vector3.Lerp(startPos, endPos, t);
            transitionOverlay.color = Color.Lerp(startColor, endColor, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator FadeSceneIn()
    {
        currentScene = nextScene;

        float elapsedTime = 0f;
        float fadeDuration = 1.5f;
        SceneTransitionData transition = GetTransitionData(currentScene);
        Vector3 startPos = leavingSceneLeft ? new Vector3(transition.fadeOutDirection.x * horizontalMove, transition.fadeOutDirection.y * horizontalMove, Camera.main.transform.position.z) : new Vector3(transition.fadeInDirection.x * horizontalMove, transition.fadeInDirection.y * horizontalMove, Camera.main.transform.position.z);
        Vector3 endPos = new Vector3(0, 0, Camera.main.transform.position.z);
        Color startColor = Color.clear;
        Color endColor = Color.clear;

        transitionOverlay.enabled = true;

        while (elapsedTime < fadeDuration)
        {
            float t = fadeInCurve.Evaluate(elapsedTime / fadeDuration);

            Camera.main.transform.position = Vector3.Lerp(startPos, endPos, t);
            transitionOverlay.color = Color.Lerp(startColor, endColor, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transitionOverlay.enabled = false;
    }

    private int GetSceneBuildIndex(SceneType sceneType)
    {
        switch (sceneType)
        {
            case SceneType.MAINMENU01: return 0;
            case SceneType.LEADERBOARD02: return 1;
            case SceneType.PLAYERAMOUNTSELECTION10: return 2;
            case SceneType.PLAYER1NAME11: return 3;
            case SceneType.PLAYER2NAME12: return 4;
            case SceneType.SELECTGAMEMODE20: return 5;
            case SceneType.GAME30: return 6;
            default: return -1;
        }
    }
}
