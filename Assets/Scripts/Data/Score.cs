using System;
using UnityEngine;

[System.Serializable]
public class Score : IComparable<Score>
{
    public int score = 0;
    public string playerName = "";
    public GameModeType gameMode = GameModeType.CLASSIC;

    /// <summary>
    /// This is quite the legacy class from back in the arcade days when score was also saving opponents name to check for "coop mode"
    /// Im keeping it to keep the option open If I ever want to add coop mode or to have this neat little class to save scores
    /// </summary>
    /// <param name="score"></param>
    /// <param name="gameMode"></param>
    /// <param name="playerName"></param>
    public Score(int score, GameModeType gameMode, string playerName)
    {
        this.score = score;
        this.gameMode = gameMode;
        this.playerName = playerName;
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