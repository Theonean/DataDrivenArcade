using SaveSystem;
using System;
using System.Linq;
using UnityEngine;
using System.IO;
using TMPro;
using System.Collections.Generic;
using Steamworks;

public class Leaderboard : MonoBehaviour
{
    public TextMeshProUGUI leaderboardText;
    [SerializeField] private List<(string, int)> localScores;
    [SerializeField] private List<(string, int)> onlineScores;

    private void Awake()
    {
        localScores = new List<(string, int)>();
        onlineScores = new List<(string, int)>();

        SteamLeaderboardHandler.OnScoresDownloaded += OnScoresDownloaded;
        SteamLeaderboardHandler.OnScoreUploaded += OnScoreUploaded;
    }

    private void Start()
    {
        LoadLocalScores();
        LoadOnlineScores();
    }

    private void LoadOnlineScores()
    {
        Debug.Log("Loading online scores...");
        SteamLeaderboardHandler.DownloadScores();
    }

    private void OnScoresDownloaded(List<(string, int)> downloadedScores)
    {
        onlineScores = downloadedScores;
        ShowScores(downloadedScores);
    }

    private void LoadLocalScores()
    {
        string[] saveFiles = Directory.GetFiles(Application.persistentDataPath, "*.txt");
        print("Found " + saveFiles.Length + " local save files at " + Application.persistentDataPath);
        int i = 0;
        foreach (string fileName in saveFiles)
        {
            string retrievedData = File.ReadAllText(Path.Combine(Application.persistentDataPath, fileName));
            print(i + " | Retrieved data: " + retrievedData + " from file: " + fileName);
            SaveData saveData = JsonUtility.FromJson<SaveData>(retrievedData);
            AddScores(saveData);
            i++;
        }
        
    }

    private void AddScores(SaveData saveData)
    {
        Debug.Log("Player name: " + saveData.playerName);
        Debug.Log("Scores: " + saveData.scores.Count);
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
                    localScores.Add((saveData.playerName, topScore.score));
                }
            }
        }
    }

    public void ShowScores(List<(string, int)> scores)
    {
        scores.Sort((x, y) => y.Item2.CompareTo(x.Item2));

        PrintScoresToLeaderboard(scores);
    }

    private void PrintScoresToLeaderboard(List<(string, int)> scores)
    {
        leaderboardText.text = "";

        int maxScores = 10;
        int scoreI = 0;
        foreach ((string, int) score in scores)
        {
            scoreI++;
            if (scoreI > maxScores) break;

            var text = scoreI + ". " + score.Item1 + " - " + score.Item2 + "\n";
            leaderboardText.text += text;
        }
    }

    private void OnScoreUploaded(bool success)
    {
        if (success)
        {
            Debug.Log("Score uploaded successfully!");
            LoadOnlineScores(); // Refresh scores after upload
        }
        else
        {
            Debug.LogError("Score upload failed.");
        }
    }
}
