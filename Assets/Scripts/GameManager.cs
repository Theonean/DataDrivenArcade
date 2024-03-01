using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum CurrentScene{
    LOGIN,
    GAMESELECTION,
    GAMECLASSIC,
    GAMEFACTORY
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public CurrentScene gameState;

    public float roundTime = 60f;
    public TextMeshProUGUI countdownTimer;

    private float timeLeft;

    public ScoreManager[] players;

    void Awake() {

        if (instance == null) instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(this);
    }

    private void Start() {
        timeLeft = roundTime;
    }

    private void Update() {
        if (gameState == CurrentScene.GAMECLASSIC || gameState == CurrentScene.GAMEFACTORY) {
            timeLeft -= Time.deltaTime;
            countdownTimer.text = timeLeft.ToString("F2");
            if (timeLeft < 0) {
                timeLeft = roundTime;
                
                foreach (ScoreManager player in players)
                {
                    player.ResetPlayer();
                }
            }
        }
    }

}
