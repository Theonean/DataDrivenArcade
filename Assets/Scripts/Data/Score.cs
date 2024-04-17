using UnityEngine;

[System.Serializable]
public class Score
{
    public int score = 0;
    public GameModeType gameMode = GameModeType.CLASSIC;

    public Score(int score, GameModeType gameMode)
    {
        this.score = score;
        this.gameMode = gameMode;
    }
}