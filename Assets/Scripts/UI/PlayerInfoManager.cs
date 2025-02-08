using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class PlayerInfoManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public Slider comboSlider;
    public AnimationCurve comboSliderCurve;
    private Coroutine comboSliderCoroutine;
    public TextMeshProUGUI nameText;

    public void SetScore(int score)
    {
        scoreText.text = score.ToString();
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
