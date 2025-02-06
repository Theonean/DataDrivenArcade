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
    private List<(string, int)> scores;

    private void Awake()
    {
        scores = new List<(string, int)>();
    }

    private void Start()
    {
        LoadScores();
    }

    private void LoadScores()
    {
        print("Loading online scores");
        LoadOnlineScores();
    }

    private void LoadOnlineScores()
    {
        if (SteamManager.Initialized)
        {
            SteamAPICall_t handle = SteamUserStats.DownloadLeaderboardEntries((SteamLeaderboard_t)3501630, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 1, 10);
            CallResult<LeaderboardScoresDownloaded_t> callResult = CallResult<LeaderboardScoresDownloaded_t>.Create(OnLeaderboardScoresDownloaded);
            callResult.Set(handle);
        }
        else
        {
            Debug.LogWarning("SteamManager not initialized, cannot load online scores");
        }
    }

    private void OnLeaderboardScoresDownloaded(LeaderboardScoresDownloaded_t pCallback, bool bIOFailure)
    {
        if (bIOFailure)
        {
            Debug.LogError("Failed to download leaderboard scores");
            return;
        }

        for (int i = 0; i < pCallback.m_cEntryCount; i++)
        {
            LeaderboardEntry_t entry;
            int[] details = new int[1];
            SteamUserStats.GetDownloadedLeaderboardEntry(pCallback.m_hSteamLeaderboardEntries, i, out entry, details, details.Length);

            string playerName = SteamFriends.GetFriendPersonaName(entry.m_steamIDUser);
            int scoreValue = entry.m_nScore;
            GameModeType gameMode = (GameModeType)details[0]; // Assuming game mode is stored in details

            scores.Add((playerName, scoreValue));
        }

        Debug.Log($"SCORES: {scores}");

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
            print(i + " | Retrieved data: " + retrievedData + " from file: " + fileName);
            SaveData saveData = JsonUtility.FromJson<SaveData>(retrievedData);
            AddScores(saveData);
            i++;
        }
    }

    private void AddScores(SaveData saveData)
    {
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
                    scores.Add((saveData.playerName, topScore.score));
                }
            }
        }
    }

    public void ShowScores(string gameMode)
    {
        scores.Sort((x, y) => y.Item2.CompareTo(x.Item2));

        PrintScoresToLeaderboard(scores);

        Debug.LogWarning("Score has bug im working on");
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

            var text = scoreI + ". " + score.Item1 + " - " + score.Item2;
            leaderboardText.text += text;
        }
    }

    public static bool UploadScore(int score)
    {
        if (SteamManager.Initialized)
        {
            SteamAPICall_t handle = SteamUserStats.UploadLeaderboardScore((SteamLeaderboard_t)3501630, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, score, null, 0);
            CallResult<LeaderboardScoreUploaded_t> callResult = CallResult<LeaderboardScoreUploaded_t>.Create(OnLeaderboardScoreUploaded);
            callResult.Set(handle);
            return true;
        }
        else
        {
            Debug.LogWarning("SteamManager not initialized, cannot upload score");
            return false;
        }
    }

    private static void OnLeaderboardScoreUploaded(LeaderboardScoreUploaded_t pCallback, bool bIOFailure)
    {
        if (bIOFailure)
        {
            Debug.LogError("Failed to upload leaderboard score");
            return;
        }

        Debug.Log("Successfully uploaded leaderboard score");
    }
}
