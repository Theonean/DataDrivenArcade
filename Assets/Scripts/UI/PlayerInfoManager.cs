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
    public TextMeshProUGUI comboMultiplierValue;
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

    public void SetCombo(float combo, int multiplier)
    {
        if (comboSliderCoroutine != null)
        {
            StopCoroutine(comboSliderCoroutine);
        }

        //If combomultiplier has changed up or down, scale the combo slider up and down, dont scale if it is the same value
        int previousComboMultiplier = int.Parse(comboMultiplierValue.text.Substring(1));
        Debug.Log("Previous: " + previousComboMultiplier + " Current: " + multiplier);

        comboMultiplierValue.color = Color.Lerp(Color.white, Color.red, multiplier / PlayerManager.maximumComboMultiplier);
        comboMultiplierValue.text = "x" + multiplier;


        if (combo == 0)
        {
            comboSliderCoroutine = StartCoroutine(DeflateComboSlider());
            StartCoroutine(ScaleComboUpDown(true));
        }
        else
        {
            comboSlider.value = combo / PlayerManager.comboNeededForMaxMultiplier;

            if (previousComboMultiplier != multiplier)
                StartCoroutine(ScaleComboUpDown(false));

        }
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

    //Scale combo up and down when value changes with mathf.pingpong
    private IEnumerator ScaleComboUpDown(bool reverse = false)
    {
        float time = 0.5f;
        float elapsedTime = 0;
        float startScale = comboSlider.transform.localScale.x;
        float endScale = reverse ? 0.6f : 1.4f;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            comboMultiplierValue.transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, comboSliderCurve.Evaluate(Mathf.PingPong(elapsedTime, time)));
            yield return null;
        }

        comboMultiplierValue.transform.localScale = Vector3.one;
    }
}
