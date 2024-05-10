using System;
using UnityEngine;

[System.Serializable]
public class Score : IComparable<Score>
{
    public int score = 0;
    public string opponentName = "";
    public bool isCoop = false;
    public GameModeType gameMode = GameModeType.CLASSIC;

    public Score(int score, GameModeType gameMode, string opponentName, bool isCoop = false)
    {
        this.score = score;
        this.opponentName = opponentName;
        this.gameMode = gameMode;
        this.isCoop = isCoop;
    }

    public int CompareTo(Score other)
    {
        if (other == null)
        {
            return 1;
        }

        if (score > other.score)
        {
            return 1;
        }
        else if (score < other.score)
        {
            return -1;
        }
        else
        {
            return 0;
        }

    }
}