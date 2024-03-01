using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum CurrentScene{
    LOGIN,
    GAMESELECTION,
    GAMECLASSIC,
    GAMEFACTORY,
    GAMEMEGASHAPE
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public CurrentScene gameState;

    public float roundTime = 60f;
    public TextMeshProUGUI countdownTimer;

    private float timeLeft = 0f;

    public ScoreManager[] players;

    void Awake() {

        if (instance == null) instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(this);
    }

    private void Start() {
        //This won't work on final gamemanager but currently Im not using it as a proper singleton so ya know whateva
        if (gameState == CurrentScene.GAMECLASSIC || gameState == CurrentScene.GAMEFACTORY) {
            timeLeft = roundTime;
        }
        else
        {
            foreach (ScoreManager player in players)
            {
                player.playerInfoManager.enabled = false;
            }
        }
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
        else if (gameState == CurrentScene.GAMEMEGASHAPE)
        {
            timeLeft += Time.deltaTime;
            countdownTimer.text = timeLeft.ToString("F2");

            //Check if one of the players score is higher than 0, if yes, then we can end the game
            foreach (ScoreManager player in players)
            {
                if (player.GetScore() > 0)
                {
                    gameState = CurrentScene.GAMESELECTION;
                    timeLeft = 0;
                    countdownTimer.text = timeLeft.ToString("F2");

                    //reset all players
                    foreach (ScoreManager tempP in players)
                    {
                        tempP.ResetPlayer();
                    }
                }
            }
        }
    }

}
