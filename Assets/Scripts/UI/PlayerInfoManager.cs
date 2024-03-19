using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting.AssemblyQualifiedNameParser;

public class PlayerInfoManager : MonoBehaviour
{

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI lastScoreText;
    public TextMeshProUGUI nameText;

    public void SetScore(int score)
    {
        scoreText.text = score.ToString();
    }

    public void SetCombo(float combo)
    {
        //If the new combo is larger than previous combo, Scale the text slightly up and down over time
        if (combo > int.Parse(comboText.text))
        {
            StartCoroutine(ScaleText(combo));
        }
        //Otherwise color text red and slowly lerp to white again over 1 second
        else
        {
            comboText.color = Color.red;
            comboText.text = combo.ToString();
            StartCoroutine(LerpColor(Color.white, 1));
        }

        comboText.text = combo.ToString();
    }

    public void SetLastScore(int score)
    {
        lastScoreText.text = score.ToString();
    }

    public void SetName(string name)
    {
        scoreText.text = name;
    }

    public void Reset()
    {
        print("Resetting score: " + scoreText.text + " last score: " + lastScoreText.text);
        lastScoreText.text = int.Parse(scoreText.text) > int.Parse(lastScoreText.text) ? scoreText.text : lastScoreText.text;
        scoreText.text = "00";
        comboText.text = "00";
    }

    private IEnumerator ScaleText(float combo)
    {
        float time = 0.5f;
        float elapsedTime = 0;
        //Scale text up and down using Mathf.pingpong
        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            float scaleValue = Mathf.PingPong(elapsedTime, time);
            comboText.transform.localScale = new Vector3(1 + scaleValue, 1 + scaleValue, 1);
            yield return null;
        }
        comboText.transform.localScale = new Vector3(1, 1, 1);
    }

    private IEnumerator LerpColor(Color color, float time)
    {
        float elapsedTime = 0;
        Color startColor = comboText.color;
        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            comboText.color = Color.Lerp(startColor, color, elapsedTime / time);
            yield return null;
        }
        comboText.color = color;
    }
}
