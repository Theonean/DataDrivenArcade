using MongoDB.Driver;
using MongoDB.Bson;
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
        if (GameManager.instance.arcadeMode || !DONOTCOMMIT_MongoConnector.isOnline)
        {
            print("Loading local scores");
            LoadLocalScores();
        }
        else
        {
            print("Loading online scores from MongoDB");
            LoadOnlineScores();
        }

        //print out all gathered data
        ShowScores("ALL");
    }

    private void LoadLocalScores()
    {
        string[] saveFiles = Directory.GetFiles(Application.persistentDataPath, "*.txt");
        print("Found " + saveFiles.Length + " local save files at " + Application.persistentDataPath);
        foreach (string fileName in saveFiles)
        {
            string retrievedData = File.ReadAllText(Path.Combine(Application.persistentDataPath, fileName));
            SaveData saveData = JsonUtility.FromJson<SaveData>(retrievedData);
            AddScores(saveData);
        }
    }

    private async void LoadOnlineScores()
    {
        string uri = "mongodb://ShapeShifters_poetrywar:56002aab95ab3ac337400c7136750b4a83c2962c@4uz.h.filess.io:27018/ShapeShifters_poetrywar";
        var client = new MongoClient(uri);
        var database = client.GetDatabase("ShapeShifters_poetrywar");
        var collection = database.GetCollection<BsonDocument>("PlayersData");

        var filter = new BsonDocument();
        var findOptions = new FindOptions<BsonDocument> { NoCursorTimeout = false };

        try
        {
            using var cursor = await collection.FindAsync(filter, findOptions);
            while (await cursor.MoveNextAsync())
            {
                var batch = cursor.Current;
                foreach (var document in batch)
                {
                    document.Remove("_id"); //Otherwise Unity will throw an error as it can't serialize an ObjectId (SaveData doesn't have that attribute)
                    var json = document.ToJson();

                    SaveData saveData = JsonUtility.FromJson<SaveData>(json);
                    AddScores(saveData);
                }
            }
        }
        catch
        {
            Debug.Log("Error while loading scores from database");
            return;
        }

    }

    private void AddScores(SaveData saveData)
    {
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

        int maxScores = 15;
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
