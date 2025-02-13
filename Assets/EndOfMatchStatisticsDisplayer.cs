using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class EndOfMatchStatisticsDisplayer : MonoBehaviour
{
    public InputActionReference skipAnimationsAction;
    
    [SerializeField] private Button restartGameButton;
    [SerializeField] private PlayerManager player1;
    [SerializeField] private PlayerManager player2;

    [SerializeField] private TextMeshProUGUI[] player1Stats;
    [SerializeField] private TextMeshProUGUI[] player2Stats;

    private void OnEnable()
    {
        skipAnimationsAction.action.performed += SkipAnimations;
    }

    private void OnDisable()
    {
        skipAnimationsAction.action.performed -= SkipAnimations;
    }

    public void DisplayEndOfMatchStatistics()
    {
        // Assign text values for both players
        player1Stats[0].text = player1.shapesCompleted.ToString();
        player1Stats[1].text = player1.linesPlaced.ToString();
        player1Stats[2].text = (player1.shapesAccuracy * 100).ToString("F2") + "%";
        player1Stats[3].text = player1.largestShapeCorrect.ToString();

        player2Stats[0].text = player2.shapesCompleted.ToString();
        player2Stats[1].text = player2.linesPlaced.ToString();
        player2Stats[2].text = (player2.shapesAccuracy * 100).ToString("F2") + "%";
        player2Stats[3].text = player2.largestShapeCorrect.ToString();

        // Hide all stats initially
        foreach (var stat in player1Stats) stat.enabled = false;
        foreach (var stat in player2Stats) stat.enabled = false;

        StartCoroutine(DisplayStatsSequence());
    }

    private void SkipAnimations(InputAction.CallbackContext obj)
    {
        StopAllCoroutines();
        StartCoroutine(DisplayStatsSequence(0));
    }

    private IEnumerator DisplayStatsSequence(float waitTime = 1f)
    {
        yield return HighlightWinningStatistic(player1Stats[0], player2Stats[0], player1.shapesCompleted, player2.shapesCompleted, waitTime);
        yield return HighlightWinningStatistic(player1Stats[1], player2Stats[1], player1.linesPlaced, player2.linesPlaced, waitTime);
        yield return HighlightWinningStatistic(player1Stats[2], player2Stats[2], player1.shapesAccuracy, player2.shapesAccuracy, waitTime);
        yield return HighlightWinningStatistic(player1Stats[3], player2Stats[3], player1.largestShapeCorrect, player2.largestShapeCorrect, waitTime);

        restartGameButton.Select();
    }

    /// <summary>
    /// Highlights the correct winning statistic dynamically.
    /// </summary>
    private IEnumerator HighlightWinningStatistic(TextMeshProUGUI player1Stat, TextMeshProUGUI player2Stat, float player1Value, float player2Value, float waitTime)
    {
        player1Stat.enabled = true;
        player2Stat.enabled = true;

        if (Mathf.Approximately(player1Value, player2Value))
        {
            player1Stat.color = Color.yellow;
            player2Stat.color = Color.yellow;
        }
        else
        {
            bool player1Wins = player1Value > player2Value;
            TextMeshProUGUI winningStat = player1Wins ? player1Stat : player2Stat;
            TextMeshProUGUI losingStat = player1Wins ? player2Stat : player1Stat;

            winningStat.color = Color.green;
            losingStat.color = Color.red;

            StartCoroutine(ScaleStatUpDown(winningStat));
        }

        yield return new WaitForSeconds(waitTime);
    }

    /// <summary>
    /// Creates a simple pop animation for the winning statistic.
    /// </summary>
    private IEnumerator ScaleStatUpDown(TextMeshProUGUI stat)
    {
        float duration = 0.5f;
        float startScale = 1f;
        float endScale = 1.5f;
        AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        for (float time = 0; time < duration * 2; time += Time.deltaTime)
        {
            float scale = Mathf.Lerp(startScale, endScale, curve.Evaluate(Mathf.PingPong(time, duration) / duration));
            stat.transform.localScale = Vector3.one * scale;
            yield return null;
        }

        stat.transform.localScale = Vector3.one;
    }
}
