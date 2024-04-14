using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaveSystem
{
    [System.Serializable]
    public class SaveData
    {
        public string playerName = "";
        public int roundsPlayed = 0;
        public List<int> scores = new List<int>();
        public GameModeData preferredCustomSettings;

        public List<int> getTopNScores(int n)
        {
            scores.Sort();
            List<int> topScores = new List<int>();
            //add top n scores to list by iterating from end of list downwards for n steps
            for (int i = scores.Count - 1; i >= scores.Count - n; i--)
            {
                topScores.Add(scores[i]);
            }

            return topScores;
        }
    }
}