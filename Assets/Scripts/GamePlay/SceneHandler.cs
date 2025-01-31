using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using SaveSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneType
{
    MAINMENU01 = 0,
    LEADERBOARD02 = 1,
    PLAYERAMOUNTSELECTION10 = 2,
    PLAYER1NAME11 = 3,
    PLAYER2NAME12 = 4,
    SELECTGAMEMODE20 = 5,
    GAME30 = 6,
    EMPTY = 7
}

public class SceneHandler : MonoBehaviour
{
    public static SceneHandler Instance { get; private set; }
    public SceneType currentScene { get; private set; } = SceneType.MAINMENU01;
    public SceneType nextScene { get; private set; } = SceneType.EMPTY;
    [SerializeField] private SceneTransitions sceneTransitions;

    [SerializeField] private SpriteRenderer transitionOverlay;
    [SerializeField] private AnimationCurve fadeOutCurve;
    [SerializeField] private AnimationCurve fadeInCurve;
    [SerializeField] private float fadeDuration = 1.5f;

    private AsyncOperation asyncLoad;
    private bool leavingSceneLeft = false;
    [SerializeField] private float transitionMoveDistance = 9f;

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

    private void OnEnable() {
        CustomUIEvents.OnMoveSceneForward += GoToNextScene;
        CustomUIEvents.OnMoveSceneBackward += GoToLastScene;
    }

    private void OnDisable() {
        CustomUIEvents.OnMoveSceneForward -= GoToNextScene;
        CustomUIEvents.OnMoveSceneBackward -= GoToLastScene;
    }

    public void GoToNextScene(int sceneIndexIncrement = 1)
    {
        int nextSceneIndex = (int)Instance.currentScene + sceneIndexIncrement;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            Instance.SwitchScene((SceneType)nextSceneIndex);
        }
        else
        {
            Debug.LogError("No next scene in build order!");
        }
    }

    public void GoToLastScene(int sceneIndexDecrement = 1)
    {
        int lastSceneIndex = (int)Instance.currentScene - sceneIndexDecrement;

        //handle exceptions for scenes that are not in the build order first
        SceneType currentScene = (SceneType)SceneManager.GetActiveScene().buildIndex;
        if (currentScene == SceneType.SELECTGAMEMODE20 && sceneIndexDecrement == 1)
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
        if (nextScene != SceneType.EMPTY)
        {
            Debug.LogWarning("Scene switch already in progress!");
            return;
        }

        int targetBuildIndex = (int)sceneType;
        nextScene = sceneType;

        if (targetBuildIndex == -1)
        {
            Debug.LogError($"Scene {sceneType} not found!");
            return;
        }

        if (sceneType == SceneType.PLAYERAMOUNTSELECTION10)
        {
            SaveManager.singleton.DeInitiate();
        }

        //Workaround so that leaderboard scene is animated as being to the left of the main menu without fudging around in Build-Order and build index
        if (sceneType == SceneType.LEADERBOARD02)
        {
            leavingSceneLeft = true;
        }
        else
            leavingSceneLeft = targetBuildIndex < (int)Instance.currentScene;

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

        Vector3 startPos = Camera.main.transform.position;
        SceneTransitionData transition = GetTransitionData(currentScene);
        Vector3 endPos = leavingSceneLeft ? new Vector3(transition.fadeInDirection.x * transitionMoveDistance, transition.fadeInDirection.y * transitionMoveDistance / 1.5f, Camera.main.transform.position.z) : new Vector3(transition.fadeOutDirection.x * transitionMoveDistance, transition.fadeOutDirection.y * transitionMoveDistance / 1.5f, Camera.main.transform.position.z);
        Color startColor = Color.clear;
        Color endColor = Color.clear;

        transitionOverlay.enabled = true;

        while (elapsedTime < fadeDuration && asyncLoad.progress <= 1f)
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

        SceneTransitionData transition = GetTransitionData(currentScene);
        Vector3 startPos = leavingSceneLeft ? new Vector3(transition.fadeOutDirection.x * transitionMoveDistance, transition.fadeOutDirection.y * transitionMoveDistance / 1.5f, Camera.main.transform.position.z) : new Vector3(transition.fadeInDirection.x * transitionMoveDistance, transition.fadeInDirection.y * transitionMoveDistance / 1.5f, Camera.main.transform.position.z);
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
        nextScene = SceneType.EMPTY;
    }
}
