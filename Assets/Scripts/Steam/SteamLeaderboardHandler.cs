using System;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class SteamLeaderboardHandler : MonoBehaviour
{
    private static SteamLeaderboard_t leaderboardHandle;
    private static CallResult<LeaderboardFindResult_t> leaderboardFindResult;
    private static CallResult<LeaderboardScoreUploaded_t> leaderboardScoreUploadedResult;
    private static CallResult<LeaderboardScoresDownloaded_t> leaderboardScoresDownloadedResult;

    public static Action<List<(string, int)>> OnScoresDownloaded;
    public static Action<bool> OnScoreUploaded;
    private const string leaderboardName = "Highscores";

    private void Awake()
    {
        InitializeLeaderboard(leaderboardName);
    }

    public static void InitializeLeaderboard(string leaderboardName)
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogWarning("SteamManager not initialized, cannot initialize leaderboard");
            return;
        }

        SteamAPICall_t handle = SteamUserStats.FindLeaderboard(leaderboardName);
        leaderboardFindResult = CallResult<LeaderboardFindResult_t>.Create(OnLeaderboardFound);
        leaderboardFindResult.Set(handle);
    }

    private static void OnLeaderboardFound(LeaderboardFindResult_t pCallback, bool bIOFailure)
    {
        if (bIOFailure || pCallback.m_bLeaderboardFound == 0)
        {
            Debug.LogError("Failed to find leaderboard");
            return;
        }

        leaderboardHandle = pCallback.m_hSteamLeaderboard;
        Debug.Log("Leaderboard found! ID: " + leaderboardHandle.m_SteamLeaderboard);
    }

    public static void UploadScore(int score)
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogWarning("SteamManager not initialized, cannot upload score");
            return;
        }

        if (leaderboardHandle.m_SteamLeaderboard == 0)
        {
            Debug.LogWarning("Leaderboard not initialized, finding leaderboard first...");
            InitializeLeaderboard(leaderboardName); // Replace with your leaderboard name
            return;
        }

        SteamAPICall_t uploadHandle = SteamUserStats.UploadLeaderboardScore(
            leaderboardHandle,
            ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest,
            score,
            null,
            0
        );

        leaderboardScoreUploadedResult = CallResult<LeaderboardScoreUploaded_t>.Create(OnLeaderboardScoreUploaded);
        leaderboardScoreUploadedResult.Set(uploadHandle);
    }

    private static void OnLeaderboardScoreUploaded(LeaderboardScoreUploaded_t pCallback, bool bIOFailure)
    {
        if (bIOFailure)
        {
            Debug.LogError("Failed to upload leaderboard score");
            OnScoreUploaded?.Invoke(false);
            return;
        }

        Debug.Log("Successfully uploaded leaderboard score");
        OnScoreUploaded?.Invoke(true);
    }

    public static void DownloadScores()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogWarning("SteamManager not initialized, cannot download scores");
            return;
        }

        if (leaderboardHandle.m_SteamLeaderboard == 0)
        {
            Debug.LogWarning("Leaderboard not initialized, finding leaderboard first...");
            InitializeLeaderboard(leaderboardName); // Replace with your leaderboard name
            return;
        }

        SteamAPICall_t handle = SteamUserStats.DownloadLeaderboardEntries(
            leaderboardHandle,
            ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal,
            1,
            10
        );

        leaderboardScoresDownloadedResult = CallResult<LeaderboardScoresDownloaded_t>.Create(OnLeaderboardScoresDownloaded);
        leaderboardScoresDownloadedResult.Set(handle);
    }

    private static void OnLeaderboardScoresDownloaded(LeaderboardScoresDownloaded_t pCallback, bool bIOFailure)
    {
        if (bIOFailure)
        {
            Debug.LogError("Failed to download leaderboard scores");
            return;
        }

        List<(string, int)> scores = new List<(string, int)>();

        for (int i = 0; i < pCallback.m_cEntryCount; i++)
        {
            LeaderboardEntry_t entry;
            int[] details = new int[1];
            SteamUserStats.GetDownloadedLeaderboardEntry(pCallback.m_hSteamLeaderboardEntries, i, out entry, details, details.Length);

            string playerName = SteamFriends.GetFriendPersonaName(entry.m_steamIDUser);
            int scoreValue = entry.m_nScore;

            scores.Add((playerName, scoreValue));
        }

        Debug.Log($"Downloaded {scores.Count} scores.");
        OnScoresDownloaded?.Invoke(scores);
    }
}
