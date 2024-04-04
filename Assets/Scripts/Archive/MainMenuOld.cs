using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MainMenuOld : MonoBehaviour
{
    private GameManager gm;
    public MainMenuSelectionHandler[] mainMenuSelectionHandlers = new MainMenuSelectionHandler[2];

    public TextMeshProUGUI[] insertCoinTexts;
    public TextMeshProUGUI waitingForPlayerText;
    private bool coinInserted = false;
    private bool[] playersReady = new bool[2];

    public UnityEvent coinInsertedEvent;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameManager.instance;

        if (mainMenuSelectionHandlers[0].IsUnityNull() || mainMenuSelectionHandlers[1].IsUnityNull())
        {
            Debug.LogError("MainMenuSelectionHandlers are not set in the inspector");
        }
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

    public void PlayerSelectionChanged(LoginScreenPlayerState lsps, int playerNum)
    {
        if (lsps == LoginScreenPlayerState.READYTOPLAY)
        {
            playersReady[playerNum - 1] = true;
        }
        else
        {
            playersReady[playerNum - 1] = false;
        }

        if (playersReady[0] && playersReady[1])
        {
            gm.SwitchScene(CurrentScene.GAMESELECTION);
        }
        else if (playersReady[0] || playersReady[1])
        {
            waitingForPlayerText.enabled = true;
        }
        else
        {
            waitingForPlayerText.enabled = false;
        }

    }
}
