using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private GameManager gm;

    public TextMeshProUGUI[] insertCoinTexts;
    public TextMeshProUGUI waitingForPlayerText;
    public GameObject gameHelp;
    public GameObject[] p2Objects;
    private bool coinInserted = false;
    private string[] playersSelectedActions = new string[2];

    public UnityEvent coinInsertedEvent;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameManager.instance;
        gm.InsertCoinPressed.AddListener(InsertCoinPressed);
        gm.SinglePlayerPressed.AddListener(() => ToggleSingleOrMultiplayer(true));
        gm.MultiplayerPressed.AddListener(() => ToggleSingleOrMultiplayer(false));
    }


    // Update is called once per frame
    private void InsertCoinPressed(bool isArcadeMode)
    {
        if (isArcadeMode && !coinInserted)
        {
            coinInsertedEvent?.Invoke();
            gm.arcadeMode = true;
            coinInserted = true;
            foreach (TextMeshProUGUI insertCoinText in insertCoinTexts) { insertCoinText.enabled = false; }
            print("Arcade mode active");
        }
        else if (!isArcadeMode && !coinInserted)
        {
            coinInsertedEvent?.Invoke();
            gm.arcadeMode = false;
            coinInserted = true;
            foreach (TextMeshProUGUI insertCoinText in insertCoinTexts) { insertCoinText.enabled = false; }
            print("Keyboard mode active");
        }
    }

    private void ToggleSingleOrMultiplayer(bool isSinglePlayer)
    {
        if (coinInserted)
        {
            if (isSinglePlayer)
            {
                gm.singlePlayer = true;
                foreach (GameObject obj in p2Objects)
                {
                    obj.SetActive(false);
                }
            }
            else
            {
                gm.singlePlayer = false;
                foreach (GameObject obj in p2Objects)
                {
                    obj.SetActive(true);
                }
                coinInsertedEvent?.Invoke();
            }
        }
    }

    public void PlayerSelectionChanged(int playerNum, string actionType)
    {
        playersSelectedActions[playerNum - 1] = actionType;

        if (playersSelectedActions[0].Equals(playersSelectedActions[1]) || gm.singlePlayer)
        {
            switch (playersSelectedActions[0])
            {
                case "READYTOPLAY":
                    GameManager.SwitchScene(CurrentScene.GAMESELECTION);
                    break;
                case "GameHelp":
                    Debug.Log("GameHelp");
                    gameHelp.SetActive(true);
                    break;
            }
        }else if (playersSelectedActions[0] == "READYTOPLAY" || playersSelectedActions[1] == "READYTOPLAY")
        {
            waitingForPlayerText.enabled = true;
        }
        else
        {
            waitingForPlayerText.enabled = false;
            gameHelp.SetActive(false);
        }
    }
}
