using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private GameManager gm;

    private MainMenuSelectionHandler p1SelectionHandler;
    private MainMenuSelectionHandler p2SelectionHandler;

    public TextMeshProUGUI[] insertCoinTexts;
    private bool coinInserted = false;

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
            p1SelectionHandler.CoinInserted();
            p2SelectionHandler.CoinInserted();
            gm.arcadeMode = true;
            coinInserted = true;
            foreach (TextMeshProUGUI insertCoinText in insertCoinTexts) { insertCoinText.enabled = false; }
            print("Arcade mode active");
        }
        else if (Input.GetButtonDown("InsertCoinKeyboard") && !coinInserted)
        {
            p1SelectionHandler.CoinInserted();
            p2SelectionHandler.CoinInserted();
            gm.arcadeMode = false;
            coinInserted = true;
            foreach (TextMeshProUGUI insertCoinText in insertCoinTexts) { insertCoinText.enabled = false; }
            print("Keyboard mode active");
        }

        if (p1SelectionHandler.ReadyToPlay() && p2SelectionHandler.ReadyToPlay())
        {
            gm.SwitchScene(CurrentScene.GAMECLASSIC);
        }
    }

    public void SetSelectionHandler(MainMenuSelectionHandler selectionHandler, int playerNum)
    {
        if (playerNum == 1)
        {
            p1SelectionHandler = selectionHandler;
        }
        else if (playerNum == 2)
        {
            p2SelectionHandler = selectionHandler;
        }
        else
        {
            Debug.LogError("Player Number not set correctly on MainMenuSelectionHandler!!!");
        }
    }


}
