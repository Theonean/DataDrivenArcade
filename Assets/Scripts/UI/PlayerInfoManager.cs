using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class PlayerInfoManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI scoreRightText;
    private Vector3 scoreRightTextOriginalPos;
    private Coroutine scoreRightTextCoroutine;
    public Slider comboSlider;
    public AnimationCurve comboSliderCurve;
    private Coroutine comboSliderCoroutine;
    public TextMeshProUGUI nameText;

    private void Start()
    {
        scoreRightTextOriginalPos = scoreRightText.transform.position;
    }

    public void SetScore(int score)
    {
        int scoreDifference = score - int.Parse(scoreText.text);

        if (scoreRightTextCoroutine != null)
            StopCoroutine(scoreRightTextCoroutine);

        scoreRightTextCoroutine = StartCoroutine(FlyScoreUp(scoreDifference));
        scoreText.text = score.ToString();
    }

    private IEnumerator FlyScoreUp(int score)
    {
        float time = 1f;
        float counter = 0f;
        float scale = 2f + Mathf.Sqrt(Mathf.Sqrt(score));
        scoreRightText.enabled = true;
        scoreRightText.text = score.ToString();

        //slightly displace text randomly to the left or right to give it some taste
        scoreRightText.transform.position += new Vector3(Random.Range(-0.5f, 0.5f), 0, 0);

        while (counter < time)
        {
            counter += Time.deltaTime;
            float xMovement = 0.05f * Mathf.Sin(counter * 20f);
            scoreRightText.transform.position = scoreRightTextOriginalPos + new Vector3(xMovement, counter * 2f, 0);
            scoreRightText.color = Color.Lerp(Color.white, Color.clear, counter / time);
            scoreRightText.transform.localScale = Vector3.Lerp(Vector3.one * scale, Vector3.zero, counter / time);
            yield return null;
        }

        scoreRightText.transform.position = scoreRightTextOriginalPos;
        scoreRightText.enabled = false;
    }

    public void SetCombo(float combo)
    {
        if (comboSliderCoroutine != null)
        {
            StopCoroutine(comboSliderCoroutine);
        }


        if (combo == 0)
            comboSliderCoroutine = StartCoroutine(DeflateComboSlider());
        else
            comboSlider.value = combo / PlayerManager.comboNeededForMaxMultiplier;
    }

    public void SetName(string name)
    {
        nameText.text = name;
    }

    private IEnumerator DeflateComboSlider()
    {
        float time = 1.5f;
        float elapsedTime = 0;
        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            comboSlider.value = Mathf.Lerp(comboSlider.value, 0, comboSliderCurve.Evaluate(elapsedTime / time));
            yield return null;
        }
    }
}
