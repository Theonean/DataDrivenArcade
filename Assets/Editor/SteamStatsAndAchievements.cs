#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SteamStatsAndAchievements))]
public class SteamStatsAndAchievementsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Reset Achievements"))
        {
            if (SteamManager.Initialized)
                SteamStatsAndAchievements.Instance.ResetAchievements();
            else
                Debug.LogWarning("SteamManager not initialized, cannot reset achievements");
        }
    }
}
#endif