using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class TutorialManager : MonoBehaviour
{
    public void GiveTutorialAchievement()
    {
        if(SteamManager.Initialized)
        {
            SteamUserStats.SetAchievement("TUTORIAL_COMPLETE");
            SteamUserStats.StoreStats();
        }
        else
        {
            Debug.LogWarning("SteamManager not initialized, cannot give achievement");
        }
    }
}
