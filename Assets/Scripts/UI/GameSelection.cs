using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameSelection : MonoBehaviour
{
    public JoystickSelectable[] selectedOnStart = new JoystickSelectable[2];
    [Header("Info Panel")]
    public GameObject infoPanel;
    public TextMeshProUGUI infoPanelTitle;
    public TextMeshProUGUI infoPanelText;

    [Header("Game Descriptions")]
    [Multiline]
    public string classicDescription;
    [Multiline]
    public string gridDescription;
    [Multiline]
    public string customDescription;
    private string[] playersSelectedActions = new string[2];
    private GameManager gm;

    private void Start()
    {
        gm = GameManager.instance;

        foreach (JoystickSelectable js in selectedOnStart)
        {
            js.Selected();
        }
    }

    public void SelectionChanged(int playernum, string actionType)
    {
        playersSelectedActions[playernum - 1] = actionType;

        if (playersSelectedActions[0].Equals(playersSelectedActions[1]))
        {
            switch (playersSelectedActions[0])
            {
                case "ClassicInfo":
                    infoPanelTitle.text = "Classic Mode Description";
                    infoPanelText.text = classicDescription;
                    SetInfoPanelActive(true);
                    break;
                case "GridInfo":
                    infoPanelTitle.text = "Grid Mode Description";
                    infoPanelText.text = gridDescription;
                    SetInfoPanelActive(true);
                    break;
                case "CustomInfo":
                    infoPanelTitle.text = "Custom Mode Description";
                    infoPanelText.text = customDescription;
                    SetInfoPanelActive(true);
                    break;
                case "ClassicStart":
                    //Start the game with the game mode data initialized to classic
                    break;
                case "GridStart":
                    //Start the game with the game mode data initialized to grid
                    break;
                case "CustomStart":
                    //Start the game with the game mode data initialized to custom
                    break;
            }
        }
        else
        {
            SetInfoPanelActive(false);
        }
    }

    private void SetInfoPanelActive(bool active)
    {
        infoPanel.SetActive(active);
        infoPanelText.enabled = active;
        infoPanelTitle.enabled = active;

        infoPanelTitle.rectTransform.sizeDelta = new Vector2(infoPanelTitle.preferredWidth, infoPanelTitle.preferredHeight);
    }
}
