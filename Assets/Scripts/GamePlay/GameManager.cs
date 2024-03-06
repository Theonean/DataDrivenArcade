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
    public bool arcadeMode = false;
    public string p1Name;
    public string p2Name;

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

    public void SetState(CurrentScene newState) {
        gameState = newState;
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

    //Returns the number Input pressed by the player (out int) and how many inputs there were (normal function return)
    public int[] GetInputNumber(int playerNum){
        List<int> AmountKeysPressedThisFrame = new List<int>();
        int i = 0;
        
        if(Input.GetButtonDown("P" + playerNum + "L1")){
            AmountKeysPressedThisFrame.Add(0);
            i += 1;
        }

        if(Input.GetButtonDown("P" + playerNum + "L2")){
            AmountKeysPressedThisFrame.Add(1);
            i += 1;
        }

        if(Input.GetButtonDown("P" + playerNum + "L3")){
            AmountKeysPressedThisFrame.Add(2);
            i += 1;
        }

        if(Input.GetButtonDown("P" + playerNum + "L4")){
            AmountKeysPressedThisFrame.Add(3);
            i += 1;
        }

        if(Input.GetButtonDown("P" + playerNum + "L5")){
            AmountKeysPressedThisFrame.Add(4);
            i += 1;
        }

        if(Input.GetButtonDown("P" + playerNum + "L6")){
            AmountKeysPressedThisFrame.Add(5);
            i += 1;
        }

        return AmountKeysPressedThisFrame.ToArray();
    }

    public string GetPlayerName(int playerNum){
        if (playerNum == 1) return p1Name;
        else if (playerNum == 2) return p2Name;
        else{
            Debug.Log("Error in GetPlayerName");
            return "Error";
        }
    }

}
