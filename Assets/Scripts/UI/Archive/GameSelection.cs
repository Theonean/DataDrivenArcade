using SaveSystem;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameSelection : MonoBehaviour
{
    public JoystickSelectable[] selectedOnStart = new JoystickSelectable[2];
    public GameObject waitingOtherPlayer;
    [Header("Info Panel")]
    public GameObject infoPanel;
    public TextMeshProUGUI infoPanelTitle;
    public TextMeshProUGUI infoPanelText;
    public Image infoPanelGamePreview;
    public Sprite[] gamePreviews = new Sprite[3];

    [Header("Custom Game Settings")]
    public GameObject customGameSettings;
    public JoystickSelectable[] customGameSettingsStartSelections = new JoystickSelectable[2];

    [Header("Game Descriptions")]
    [Multiline]
    public string classicDescription;
    [Multiline]
    public string gridDescription;
    [Multiline]
    public string customDescription;
    private string[] playersSelectedActions = new string[2] { "Nothing", "Nothing" };
    public GameObject[] p2Objects;
    private GameManager gm;
    private bool switchingScenes = false;
    private GameModeData gameModeData;

    private void Start()
    {
        gm = GameManager.instance;

        foreach (JoystickSelectable js in selectedOnStart)
        {
            js.Selected();
        }

        infoPanel.GetComponent<Canvas>().enabled = false;
        SetCustomGameSettingsActive(false);

        foreach (GameObject obj in p2Objects)
        {
            obj.SetActive(!gm.singlePlayer);
        }

    }

    public void SelectionChanged(int playernum, string actionType)
    {
        playersSelectedActions[playernum - 1] = actionType;
        //print("Player " + playernum + " selected " + actionType + " and is active in hierarchy " + gameObject.activeInHierarchy);

        if (switchingScenes) return;

        if ((playersSelectedActions[0].Equals(playersSelectedActions[1]) || gm.singlePlayer)
         && !waitingOtherPlayer.IsUnityNull()) //Waitingother in if fixes bug that was happening when switching scenes
        {
            waitingOtherPlayer.SetActive(false);

            switch (playersSelectedActions[0])
            {
                case "GoBack":
                    GameManager.SwitchScene(SceneType.LOGIN);
                    break;
                case "ClassicInfo":
                    infoPanelTitle.text = "Classic Mode Description";
                    infoPanelText.text = classicDescription;
                    infoPanelGamePreview.sprite = gamePreviews[0];
                    SetInfoPanelActive(true);
                    break;
                case "GridInfo":
                    infoPanelTitle.text = "Grid Mode Description";
                    infoPanelText.text = gridDescription;
                    infoPanelGamePreview.sprite = gamePreviews[1];
                    SetInfoPanelActive(true);
                    break;
                case "CustomInfo":
                    infoPanelTitle.text = "Custom Mode Description";
                    infoPanelText.text = customDescription;
                    infoPanelGamePreview.sprite = gamePreviews[2];
                    SetInfoPanelActive(true);
                    break;
                case "ClassicStart":
                    //Start the game with the game mode data initialized to classic
                    switchingScenes = true;
                    gm.gameModeData = new GameModeData(GameModeType.CLASSIC);
                    GameManager.SwitchScene(SceneType.GAME);
                    break;
                case "GridStart":
                    //Start the game with the game mode data initialized to grid
                    switchingScenes = true;
                    gm.gameModeData = new GameModeData(GameModeType.GRID);
                    GameManager.SwitchScene(SceneType.GAME);
                    break;
                //Open custom game settings
                case "CustomStart":
                    SetCustomGameSettingsActive(true);
                    customGameSettingsStartSelections[0].Selected();
                    customGameSettingsStartSelections[1].Selected();
                    break;
                //Close custom game settings
                case "CancelCustomGame":
                    SetCustomGameSettingsActive(false);
                    selectedOnStart[0].Selected();
                    selectedOnStart[1].Selected();
                    break;
                //Start game with custom game settings
                case "StartCustomGame":
                    switchingScenes = true;
                    gm.gameModeData = new GameModeData(GameModeType.CUSTOM); //Create empty hull of data which is filled with save

                    //Set gamemodedata on gamemanager
                    foreach (Editable editable in customGameSettings.GetComponentsInChildren<Editable>())
                    {
                        gm.gameModeData.SetField(editable.name, editable.GetValue());
                    }

                    gameModeData = gm.gameModeData; //Save gamemodedata to variable for later use

                    //HACKEDY HACK HACK YEEHAW
                    SaveManager.singleton.playersData[0].preferredCustomSettings = gm.gameModeData;
                    if (!gm.singlePlayer) SaveManager.singleton.playersData[1].preferredCustomSettings = gm.gameModeData;

                    print("Custom game settings: " + gm.gameModeData.ToString());

                    GameManager.SwitchScene(SceneType.GAME);
                    break;
            }
        }
        else if (!playersSelectedActions[0].Equals(playersSelectedActions[1]))
        {
            if (!waitingOtherPlayer.IsUnityNull()) waitingOtherPlayer.SetActive(true);
            SetInfoPanelActive(false);
        }
    }

    private void SetInfoPanelActive(bool active)
    {
        infoPanel.GetComponent<Canvas>().enabled = active;
        infoPanelText.enabled = active;
        infoPanelTitle.enabled = active;

        infoPanelTitle.rectTransform.sizeDelta = new Vector2(infoPanelTitle.preferredWidth, infoPanelTitle.preferredHeight);
    }

    private void SetCustomGameSettingsActive(bool active)
    {
        if (customGameSettings.activeInHierarchy == active && active == true) return;

        customGameSettings.SetActive(active);

        //Find all the editables and toggle them
        Editable[] editableFields = customGameSettings.GetComponentsInChildren<Editable>();
        //print("Found " + editableFields.Length + " editable fields with the names " + string.Join(", ", editableFields.Select(x => x.name).ToArray()));
        foreach (Editable field in editableFields)
        {
            field.SetVisibility(active);
            //print("Field " + field.name + " is active " + field.gameObject.activeSelf + " and should be " + active);
        }
    }
}
