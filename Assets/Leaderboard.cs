using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SaveSystem;
using TMPro;
using UnityEngine;

public class Leaderboard : MonoBehaviour
{
    public TextMeshProUGUI leaderboardText;

    private List<(string, Score)> scores;

    //When Starting up, look throught saved scores and populate lists
    private void Awake()
    {
        scores = new List<(string, Score)>();
        LoadScores();
    }

    private void LoadScores()
    {
        //Find all saveFiles
        string[] saveFiles = Directory.GetFiles(Application.persistentDataPath, "*.txt");

        //Iterate over save files
        foreach (string fileName in saveFiles)
        {
            string retrievedData = File.ReadAllText(Path.Combine(Application.persistentDataPath, fileName));
            SaveData saveData = JsonUtility.FromJson<SaveData>(retrievedData);
            print(Application.persistentDataPath + fileName);
            //Add top score for each category to list
            if (saveData.scores.Count > 0)
            {
                print(saveData.playerName + " " + saveData.scores.Max(x => x.score));
                foreach (GameModeType gameMode in new GameModeType[] { GameModeType.CLASSIC, GameModeType.GRID, GameModeType.CUSTOM })
                {
                    Score tempScoreClassic =
                        saveData.scores
                        .Where(x => x.score == saveData.scores.Max(y => y.score))
                        .Where(x => x.gameMode == gameMode)
                        .FirstOrDefault();
                    if (tempScoreClassic != null)
                    {
                        print(saveData.playerName + " " + tempScoreClassic.score);
                        scores.Add((saveData.playerName, tempScoreClassic));
                    }
                }
            }

        }

        //print out all gathered data
        PrintScoresToLeaderboard(scores);
    }

    public void ShowScores(string gameMode)
    {
        
        //Filter scores by game mode
        //List<(string, Score)> filteredScores = scores.Where(x => x.Item2.gameMode == gameMode).ToList();
        print(scores);
        foreach ((string, Score) playerScore in scores)
        {
            print(playerScore.Item1 + " - " + playerScore.Item2.score + " - " + playerScore.Item2.gameMode.ToString());
        }

        List<(string, Score)> filteredScores = gameMode == "ALL" ? scores : scores.Where(x => x.Item2.gameMode.ToString() == gameMode).ToList();

        //Sort scores by score
        filteredScores.Sort((x, y) => y.Item2.score.CompareTo(x.Item2.score));

        //Print scores to leaderboard
        PrintScoresToLeaderboard(filteredScores);

        Debug.LogWarning("Score has bug im working on");
    }

    private void PrintScoresToLeaderboard(List<(string, Score)> scores)
    {
        leaderboardText.text = "";
        foreach ((string, Score) score in scores)
        {
            var text = score.Item1 + " - " + score.Item2.score + " - " + score.Item2.gameMode + "\n";
            leaderboardText.text += text;
        }
    }

}
