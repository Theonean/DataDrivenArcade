using SaveSystem;
using System;
using System.Linq;
using UnityEngine;
using System.IO;
using TMPro;
using System.Collections.Generic;

public class Leaderboard : MonoBehaviour
{
    public TextMeshProUGUI leaderboardText;
    private List<(string, Score)> scores;

    private void Awake()
    {
        scores = new List<(string, Score)>();
    }

    private void Start()
    {
        LoadScores();
    }

    private void LoadScores()
    {
        print("Loading local scores");
        LoadLocalScores();
       

        //print out all gathered data
        ShowScores("CLASSIC");
    }

    private void LoadLocalScores()
    {
        string[] saveFiles = Directory.GetFiles(Application.persistentDataPath, "*.txt");
        print("Found " + saveFiles.Length + " local save files at " + Application.persistentDataPath);
        int i = 0;
        foreach (string fileName in saveFiles)
        {
            string retrievedData = File.ReadAllText(Path.Combine(Application.persistentDataPath, fileName));
            //Print out the retrieved data
            print(i + " | Retrieved data: " + retrievedData + " from file: " + fileName);
            SaveData saveData = JsonUtility.FromJson<SaveData>(retrievedData);
            AddScores(saveData);
            i++;
        }
    }

    private void AddScores(SaveData saveData)
    {
        //Print out the saved data
        print("Player name: " + saveData.playerName);
        print("Scores: " + saveData.scores.Count);
        if (saveData.scores.Count > 0)
        {
            foreach (GameModeType gameMode in Enum.GetValues(typeof(GameModeType)))
            {
                Score topScore =
                    saveData.scores
                    .Where(x => x.gameMode == gameMode)
                    .OrderByDescending(x => x.score)
                    .FirstOrDefault();
                if (topScore != null)
                {
                    scores.Add((saveData.playerName, topScore));
                }
            }
        }
    }
    public void ShowScores(string gameMode)
    {
        /*
        print(scores);
        foreach ((string, Score) playerScore in scores)
        {
            print(playerScore.Item1 + " - " + playerScore.Item2.score + " - " + playerScore.Item2.gameMode.ToString());
        }
        */

        //Filter scores by game mode
        List<(string, Score)> filteredScores = gameMode == "ALL" ? scores : scores.Where(x => x.Item2.gameMode.ToString() == gameMode).ToList();

        //Sort scores descending
        filteredScores.Sort((x, y) => y.Item2.score.CompareTo(x.Item2.score));

        //Print scores to leaderboard
        if (gameMode == "ALL")
            PrintScoresToLeaderboard(filteredScores, true);
        else
            PrintScoresToLeaderboard(filteredScores);

        Debug.LogWarning("Score has bug im working on");
    }

    private void PrintScoresToLeaderboard(List<(string, Score)> scores, bool withGameMode = false)
    {
        leaderboardText.text = "";

        int maxScores = 10;
        int scoreI = 0;
        foreach ((string, Score) score in scores)
        {
            scoreI++;
            if (scoreI > maxScores) break;

            var text = scoreI + ". " + score.Item1 + " - " + score.Item2.score;
            if (score.Item2.isCoop)
                text += " - COOP";
            text += withGameMode ? " - " + score.Item2.gameMode.ToString() + "\n" : "\n";
            leaderboardText.text += text;
        }
    }
}
