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
    private bool coinInserted = false;
    private string[] playersSelectedActions = new string[2];

    public UnityEvent coinInsertedEvent;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("InsertCoinArcade") && !coinInserted)
        {
            coinInsertedEvent?.Invoke();
            gm.arcadeMode = true;
            coinInserted = true;
            foreach (TextMeshProUGUI insertCoinText in insertCoinTexts) { insertCoinText.enabled = false; }
            print("Arcade mode active");
        }
        else if (Input.GetButtonDown("InsertCoinKeyboard") && !coinInserted)
        {
            coinInsertedEvent?.Invoke();
            gm.arcadeMode = false;
            coinInserted = true;
            foreach (TextMeshProUGUI insertCoinText in insertCoinTexts) { insertCoinText.enabled = false; }
            print("Keyboard mode active");
        }
    }

    public void PlayerSelectionChanged(int playerNum, string actionType)
    {
        playersSelectedActions[playerNum - 1] = actionType;

        if (playersSelectedActions[0] == "READYTOPLAY" && playersSelectedActions[1] == "READYTOPLAY")
        {
            gm.SwitchScene(CurrentScene.GAMESELECTION);
        }
        else if (playersSelectedActions[0] == "READYTOPLAY" || playersSelectedActions[1] == "READYTOPLAY")
        {
            waitingForPlayerText.enabled = true;
        }
        else
        {
            waitingForPlayerText.enabled = false;
        }
    }
}
