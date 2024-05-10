using System.Collections.Generic;

namespace SaveSystem
{
    [System.Serializable]
    public class SaveData
    {
        public string playerName = "";
        public int roundsWon = 0;
        public int roundsPlayed = 0;
        public List<Score> scores = new List<Score>();
        public GameModeData preferredCustomSettings = new GameModeData(GameModeType.CUSTOM);

        public SaveData()
        {
            playerName = "";
            roundsWon = 0;
            roundsPlayed = 0;
            scores = new List<Score>();
            preferredCustomSettings = new GameModeData(GameModeType.CUSTOM);
        }

        public List<Score> getTopNScores(int n)
        {
            scores.Sort();
            List<Score> topScores = new List<Score>();
            //add top n scores to list by iterating from end of list downwards for n steps
            for (int i = scores.Count - 1; i >= scores.Count - n; i--)
            {
                topScores.Add(scores[i]);
            }

            return topScores;
        }

        override
        public string ToString()
        {
            string scoresString = "";
            foreach (Score score in scores)
            {
                scoresString += score.score + ", " + score.gameMode + "|";
            }
            return "Player Name: " + playerName + ", Rounds Won: " + roundsWon + ", Scores: " + scoresString + " \n" + preferredCustomSettings.ToString();
        }

        public int GetHighScore()
        {
            scores.Sort();
            if (scores.Count > 0)
            {
                return scores[scores.Count - 1].score;
            }
            else
            {
                return 0;
            }
        }
    }
}