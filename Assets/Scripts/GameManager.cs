using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState {
    INTRO,
    PLAYING
}

public enum ColourState{
    GREEN = 1,
    RED = 2,
    BLUE = 3,
    YELLOW = 4
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public ColourState Player1Colour;
    public ColourState Player2Colour;

    void Awake() {

        if (instance == null) instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(this);
    }

    void Start() {
        //Set default colours from playerprefs
        Player1Colour = (ColourState)PlayerPrefs.GetInt("Player1Colour");
        Player2Colour = (ColourState)PlayerPrefs.GetInt("Player2Colour");
    }

    public void SetPlayer1Colour(ColourState colour) {
        Player1Colour = colour;
        PlayerPrefs.SetInt("Player1Colour", (int)colour);
    }

    public void SetPlayer2Colour(ColourState colour) {
        Player2Colour = colour;
        PlayerPrefs.SetInt("Player2Colour", (int)colour);
    }

    public ColourState GetPlayerColour(bool isPlayer1) {
        if (isPlayer1) {
            return Player1Colour;
        }
        else {
            return Player2Colour;
        }
    }

}
