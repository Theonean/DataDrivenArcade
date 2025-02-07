using UnityEngine;
using System.Collections;
using System.ComponentModel;
using Steamworks;

// This is a port of StatsAndAchievements.cpp from SpaceWar, the official Steamworks Example.
public class SteamStatsAndAchievements : MonoBehaviour
{
    private enum Achievement : int
    {
        ACH_FINISHED_TUTORIAL,
        ACH_SINGLEPLAYER_1WIN,
        ACH_SINGLEPLAYER_5WIN,
        ACH_MULTIPLAYER_1WIN,
        ACH_MULTIPLAYER_5WIN,
        ACH_SCORE_100,
        ACH_SCORE_500,
        ACH_SCORE_1000,
    };

    private Achievement_t[] m_Achievements = new Achievement_t[] {
        new Achievement_t(Achievement.ACH_FINISHED_TUTORIAL, "Tutorial Destroyer", "Started the tutorial, and beat it!"),
        new Achievement_t(Achievement.ACH_SINGLEPLAYER_1WIN, "Rookie Shaper", "Beat HenryAI once"),
        new Achievement_t(Achievement.ACH_SINGLEPLAYER_5WIN, "Expert Shaper", "Beat HenryAI five times"),
        new Achievement_t(Achievement.ACH_MULTIPLAYER_1WIN, "Rookie reshaper", "Win against a friend once"),
        new Achievement_t(Achievement.ACH_MULTIPLAYER_5WIN, "Professional relationship reshaper", "Win against a friend 5 times"),
        new Achievement_t(Achievement.ACH_SCORE_100, "Rookie", "Get a highscore of at least 100"),
        new Achievement_t(Achievement.ACH_SCORE_500, "Shape Shifter", "Get a highscore of at least 500"),
        new Achievement_t(Achievement.ACH_SCORE_1000, "Master of geometric manipulation", "Get a highscore of at least 1000"),

    };

    public static SteamStatsAndAchievements Instance { get; private set; }
    [SerializeField] private bool debug = false;

    // Our GameID
    private CGameID m_GameID;

    // Did we get the stats from Steam?
    private bool m_bRequestedStats;
    private bool m_bStatsValid;

    // Should we store stats this frame?
    private bool m_bStoreStats;

    // Current Steam user stats
    private int m_tutorialFinished;
    private int m_singlePlayerGamesWon;
    private int m_multiPlayerGamesWon;
    private int m_shapesCreated;
    private int m_highScore;

    protected Callback<UserStatsReceived_t> m_UserStatsReceived;
    protected Callback<UserStatsStored_t> m_UserStatsStored;
    protected Callback<UserAchievementStored_t> m_UserAchievementStored;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        if (!SteamManager.Initialized)
            return;

        // Cache the GameID for use in the Callbacks
        m_GameID = new CGameID(SteamUtils.GetAppID());

        m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
        m_UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
        m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);

        // These need to be reset to get the stats upon an Assembly reload in the Editor.
        m_bRequestedStats = false;
        m_bStatsValid = false;
    }

    private void Update()
    {
        if (!SteamManager.Initialized)
            return;

        if (!m_bRequestedStats)
        {
            // Is Steam Loaded? if no, can't get stats, done
            if (!SteamManager.Initialized)
            {
                m_bRequestedStats = true;
                return;
            }

            // If yes, request our stats
            bool bSuccess = SteamUserStats.RequestCurrentStats();

            // This function should only return false if we weren't logged in, and we already checked that.
            // But handle it being false again anyway, just ask again later.
            m_bRequestedStats = bSuccess;
        }

        if (!m_bStatsValid)
            return;

        // Get info from sources

        // Evaluate achievements
        foreach (Achievement_t achievement in m_Achievements)
        {
            if (achievement.m_bAchieved)
                continue;

            switch (achievement.m_eAchievementID)
            {
                case Achievement.ACH_FINISHED_TUTORIAL:
                    if (m_tutorialFinished == 1)
                        UnlockAchievement(achievement);
                    break;
                case Achievement.ACH_SINGLEPLAYER_1WIN:
                    if (m_singlePlayerGamesWon >= 1)
                        UnlockAchievement(achievement);
                    break;
                case Achievement.ACH_SINGLEPLAYER_5WIN:
                    if (m_singlePlayerGamesWon >= 5)
                        UnlockAchievement(achievement);
                    break;
                case Achievement.ACH_MULTIPLAYER_1WIN:
                    if (m_multiPlayerGamesWon >= 1)
                        UnlockAchievement(achievement);
                    break;
                case Achievement.ACH_MULTIPLAYER_5WIN:
                    if (m_multiPlayerGamesWon >= 5)
                        UnlockAchievement(achievement);
                    break;
                case Achievement.ACH_SCORE_100:
                    if (m_highScore >= 100)
                        UnlockAchievement(achievement);
                    break;
                case Achievement.ACH_SCORE_500:
                    if (m_highScore >= 500)
                        UnlockAchievement(achievement);
                    break;
                case Achievement.ACH_SCORE_1000:
                    if (m_highScore >= 1000)
                        UnlockAchievement(achievement);
                    break;
            }
        }

        //Store stats in the Steam database if necessary
        if (m_bStoreStats)
        {
            // already set any achievements in UnlockAchievement

            // set stats
            SteamUserStats.SetStat("finishedTutorial", m_tutorialFinished);

            bool bSuccess = SteamUserStats.StoreStats();
            // If this failed, we never sent anything to the server, try
            // again later.
            m_bStoreStats = !bSuccess;
        }
    }

    public void FinishedTutorial()
    {
        m_tutorialFinished = 1;
        m_bStoreStats = true;
    }

    public void FinishedSinglePlayerGame(int score)
    {
        m_singlePlayerGamesWon++;
        m_highScore = m_highScore > score ? m_highScore : score;
        m_bStoreStats = true;
    }

    public void FinishedMultiPlayerGame(int score)
    {
        m_multiPlayerGamesWon++;
        m_highScore = m_highScore > score ? m_highScore : score;
        m_bStoreStats = true;
    }

    public void CreatedShape()
    {
        m_shapesCreated++;
        m_bStoreStats = true;
    }

    //-----------------------------------------------------------------------------
    // Purpose: Unlock this achievement
    //-----------------------------------------------------------------------------
    private void UnlockAchievement(Achievement_t achievement)
    {
        achievement.m_bAchieved = true;

        // the icon may change once it's unlocked
        //achievement.m_iIconImage = 0;

        // mark it down
        SteamUserStats.SetAchievement(achievement.m_eAchievementID.ToString());

        // Store stats end of frame
        m_bStoreStats = true;
    }

    //-----------------------------------------------------------------------------
    // Purpose: We have stats data from Steam. It is authoritative, so update
    //			our data with those results now.
    //-----------------------------------------------------------------------------
    private void OnUserStatsReceived(UserStatsReceived_t pCallback)
    {
        if (!SteamManager.Initialized)
            return;

        // we may get callbacks for other games' stats arriving, ignore them
        if ((ulong)m_GameID == pCallback.m_nGameID)
        {
            if (EResult.k_EResultOK == pCallback.m_eResult)
            {
                Debug.Log("Received stats and achievements from Steam\n");

                m_bStatsValid = true;

                // load achievements
                foreach (Achievement_t ach in m_Achievements)
                {
                    bool ret = SteamUserStats.GetAchievement(ach.m_eAchievementID.ToString(), out ach.m_bAchieved);
                    if (ret)
                    {
                        ach.m_strName = SteamUserStats.GetAchievementDisplayAttribute(ach.m_eAchievementID.ToString(), "name");
                        ach.m_strDescription = SteamUserStats.GetAchievementDisplayAttribute(ach.m_eAchievementID.ToString(), "desc");
                    }
                    else
                    {
                        Debug.LogWarning("SteamUserStats.GetAchievement failed for Achievement " + ach.m_eAchievementID + "\nIs it registered in the Steam Partner site?");
                    }
                }

                // load stats
                SteamUserStats.GetStat("nTimesFinishedTutorial", out m_tutorialFinished);
                SteamUserStats.GetStat("singleplayerGamesWon", out m_singlePlayerGamesWon);
                SteamUserStats.GetStat("multiplayerGamesWon", out m_multiPlayerGamesWon);
                SteamUserStats.GetStat("shapesCreated", out m_shapesCreated);
                SteamUserStats.GetStat("highscore", out m_highScore);
            }
            else
            {
                Debug.Log("RequestStats - failed, " + pCallback.m_eResult);
            }
        }
    }

    //-----------------------------------------------------------------------------
    // Purpose: Our stats data was stored!
    //-----------------------------------------------------------------------------
    private void OnUserStatsStored(UserStatsStored_t pCallback)
    {
        // we may get callbacks for other games' stats arriving, ignore them
        if ((ulong)m_GameID == pCallback.m_nGameID)
        {
            if (EResult.k_EResultOK == pCallback.m_eResult)
            {
                Debug.Log("StoreStats - success");
            }
            else if (EResult.k_EResultInvalidParam == pCallback.m_eResult)
            {
                // One or more stats we set broke a constraint. They've been reverted,
                // and we should re-iterate the values now to keep in sync.
                Debug.Log("StoreStats - some failed to validate");
                // Fake up a callback here so that we re-load the values.
                UserStatsReceived_t callback = new UserStatsReceived_t();
                callback.m_eResult = EResult.k_EResultOK;
                callback.m_nGameID = (ulong)m_GameID;
                OnUserStatsReceived(callback);
            }
            else
            {
                Debug.Log("StoreStats - failed, " + pCallback.m_eResult);
            }
        }
    }

    //-----------------------------------------------------------------------------
    // Purpose: An achievement was stored
    //-----------------------------------------------------------------------------
    private void OnAchievementStored(UserAchievementStored_t pCallback)
    {
        // We may get callbacks for other games' stats arriving, ignore them
        if ((ulong)m_GameID == pCallback.m_nGameID)
        {
            if (0 == pCallback.m_nMaxProgress)
            {
                Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!");
            }
            else
            {
                Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' progress callback, (" + pCallback.m_nCurProgress + "," + pCallback.m_nMaxProgress + ")");
            }
        }
    }

    private void OnGUI() {
        if(debug)
            Render();
    }

    //-----------------------------------------------------------------------------
    // Purpose: Display the user's stats and achievements
    //-----------------------------------------------------------------------------
    public void Render()
    {
        if (!SteamManager.Initialized)
        {
            GUILayout.Label("Steamworks not Initialized");
            return;
        }

        GUILayout.Label("Tutorial Finished: " + m_tutorialFinished);
        GUILayout.Label("Singleplayer Games Won: " + m_singlePlayerGamesWon);
        GUILayout.Label("Multiplayer Games Won: " + m_multiPlayerGamesWon);
        GUILayout.Label("Shapes Created: " + m_shapesCreated);
        GUILayout.Label("Highscore: " + m_highScore);

        GUILayout.BeginArea(new Rect(Screen.width - 300, 0, 300, 800));
        foreach (Achievement_t ach in m_Achievements)
        {
            GUILayout.Label(ach.m_eAchievementID.ToString());
            GUILayout.Label(ach.m_strName + " - " + ach.m_strDescription);
            GUILayout.Label("Achieved: " + ach.m_bAchieved);
            GUILayout.Space(20);
        }

        // FOR TESTING PURPOSES ONLY!
        if (GUILayout.Button("RESET STATS AND ACHIEVEMENTS"))
        {
            SteamUserStats.ResetAllStats(true);
            SteamUserStats.RequestCurrentStats();
        }
        GUILayout.EndArea();
    }

    public void ResetAchievements()
    {
        SteamUserStats.ResetAllStats(true);
        SteamUserStats.RequestCurrentStats();
    }

    private class Achievement_t
    {
        public Achievement m_eAchievementID;
        public string m_strName;
        public string m_strDescription;
        public bool m_bAchieved;

        /// <summary>
        /// Creates an Achievement. You must also mirror the data provided here in https://partner.steamgames.com/apps/achievements/yourappid
        /// </summary>
        /// <param name="achievement">The "API Name Progress Stat" used to uniquely identify the achievement.</param>
        /// <param name="name">The "Display Name" that will be shown to players in game and on the Steam Community.</param>
        /// <param name="desc">The "Description" that will be shown to players in game and on the Steam Community.</param>
        public Achievement_t(Achievement achievementID, string name, string desc)
        {
            m_eAchievementID = achievementID;
            m_strName = name;
            m_strDescription = desc;
            m_bAchieved = false;
        }
    }
}