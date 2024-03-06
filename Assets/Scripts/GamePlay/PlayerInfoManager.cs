using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInfoManager : MonoBehaviour
{
    
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI lastScoreText;

    public void SetScore(int score) {
        scoreText.text = score.ToString();
    }

    public void SetCombo(float combo) {
        comboText.text = combo.ToString();
    }

    public void SetLastScore(int score) {
        lastScoreText.text = score.ToString();
    }

    public void Reset(){
        scoreText.text = "00";
        comboText.text = "00";
        lastScoreText.text = "00";
    }
}
