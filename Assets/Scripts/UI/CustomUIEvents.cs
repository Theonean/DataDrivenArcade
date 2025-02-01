using UnityEngine;


[CreateAssetMenu(fileName = "CustomUIEvents", menuName = "CustomUIEvents", order = 1)]
public class CustomUIEvents : ScriptableObject
{
    public bool DebugEvents = true;
    #region Resume Game
    public delegate void ResumeGame();
    public static event ResumeGame OnResumeGame;
    public void VirtualResumeGame()
    {
        OnResumeGame?.Invoke();

        if (DebugEvents)
        {
            Debug.Log("Resume Game Event Invoked");
            Debug.Log("Called by: " + this);
        }
    }
    #endregion

    #region Restart Round
    public delegate void RestartRound();
    public static event RestartRound OnRestartRound;
    public void VirtualRestartRound()
    {
        OnRestartRound?.Invoke();
    }
    #endregion

    #region  Quit Game
    public delegate void QuitGame();
    public static event QuitGame OnQuitGame;
    public void VirtualQuitGame()
    {
        OnQuitGame?.Invoke();
    }
    #endregion

    #region  Goto Main Menu
    public delegate void GotoMainMenu();
    public static event GotoMainMenu OnGotoMainMenu;
    public void VirtualGotoMain()
    {
        OnGotoMainMenu?.Invoke();
    }
    #endregion

    //This is a lil bit silly because I could just be using positive and negative numbers inside on function but whatever, we're too deep in now and the project won't continue on for much longer
    //love you future me, have fun <3
    #region Move Scene forward
    public delegate void MoveSceneForward(int buildIndexIncrement);
    public static event MoveSceneForward OnMoveSceneForward;
    public static void VirtualMoveSceneForward(int buildIndexIncrement)
    {
        OnMoveSceneForward?.Invoke(buildIndexIncrement);
    }
    #endregion

    #region Move Scene Backward
    public delegate void MoveSceneBackward(int buildIndexDecrement);
    public static event MoveSceneBackward OnMoveSceneBackward;
    public static void VirtualMoveSceneBackward(int buildIndexDecrement)
    {
        OnMoveSceneBackward?.Invoke(buildIndexDecrement);
    }
    #endregion

    #region Toggle Name Input Selected
    public delegate void ToggleNameInputSelected(int playerNum);
    public static event ToggleNameInputSelected OnToggleNameInputSelected;
    public void VirtualToggleNameInputSelected(int playerNum)
    {
        OnToggleNameInputSelected?.Invoke(playerNum);
    }
    #endregion

    #region  Save Player Name
    public delegate void SavePlayerName();
    public static event SavePlayerName OnSavePlayerName;
    public void VirtualSavePlayerName()
    {
        OnSavePlayerName?.Invoke();
    }
    #endregion

    #region Set Player Count
    public delegate void SetPlayerCount(int playerCount);
    public static event SetPlayerCount OnSetPlayerCount;
    public void VirtualSetPlayerCount(int playerCount)
    {
        OnSetPlayerCount?.Invoke(playerCount);
    }
    #endregion

    #region Toggle Player Ready
    public delegate void TogglePlayerReady(int playerNum);
    public static event TogglePlayerReady OnTogglePlayerReady;
    public void VirtualTogglePlayerReady(int playerNum)
    {
        OnTogglePlayerReady?.Invoke(playerNum);
    }
    #endregion


}
