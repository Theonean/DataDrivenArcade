using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MainMenu : MonoBehaviour
{
    private GameManager gm;

    public TextMeshProUGUI[] insertCoinTexts;
    public TextMeshProUGUI waitingForPlayerText;
    public GameObject gameHelp;
    public GameObject gameAbout;
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

    private void InsertCoinPressed(bool isArcadeMode)
    {
        if (!coinInserted) return;
        switch (isArcadeMode)
        {
            case true:
                coinInsertedEvent?.Invoke();
                gm.arcadeMode = true;
                coinInserted = true;
                foreach (TextMeshProUGUI insertCoinText in insertCoinTexts) { insertCoinText.enabled = false; }
                print("Arcade mode active");
                break;
            case false:
                coinInsertedEvent?.Invoke();
                gm.arcadeMode = false;
                coinInserted = true;
                foreach (TextMeshProUGUI insertCoinText in insertCoinTexts) { insertCoinText.enabled = false; }
                print("Keyboard mode active");
                break;
        }
    }

    private void ToggleSingleOrMultiplayer(bool isSinglePlayer)
    {
        if (!coinInserted) return;

        switch (isSinglePlayer)
        {
            case true:
                gm.singlePlayer = true;
                foreach (GameObject obj in p2Objects)
                {
                    obj.SetActive(false);
                }
                break;
            case false:
                gm.singlePlayer = false;
                foreach (GameObject obj in p2Objects)
                {
                    obj.SetActive(true);
                }
                coinInsertedEvent?.Invoke();
                break;
        }
    }

    public void PlayerSelectionChanged(int playerNum, string actionType)
    {
        playersSelectedActions[playerNum - 1] = actionType;

        if (playersSelectedActions[0].Equals(playersSelectedActions[1]) || gm.singlePlayer && actionType != "")
        {
            switch (playersSelectedActions[0])
            {
                case "READYTOPLAY":
                    //Find both player names by searching for editable components
                    string p1Name = GameObject.Find("P1NameInputField").GetComponent<Editable>().GetValue();
                    GameManager.instance.SetPlayerName(1, p1Name);
                    print("Player 1 name: " + p1Name);

                    if (!GameManager.instance.singlePlayer)
                    {
                        string p2Name = GameObject.Find("P2NameInputField").GetComponent<Editable>().GetValue();
                        GameManager.instance.SetPlayerName(2, p2Name);
                        print("Player 2 name: " + p2Name);
                    }

                    GameManager.instance.SwitchScene(CurrentScene.GAMESELECTION);
                    break;
                case "GameHelp":
                    gameHelp.SetActive(true);
                    gameAbout.SetActive(false);
                    break;
                case "GameAbout":
                    gameAbout.SetActive(true);
                    gameHelp.SetActive(false);
                    break;
                case "QuitGame":
                    if (gm.arcadeMode) GameManager.instance.SwitchScene(CurrentScene.LOGIN);
                    else Application.Quit();
                    break;
            }
        }
        else if (playersSelectedActions[0] == "READYTOPLAY" || playersSelectedActions[1] == "READYTOPLAY")
        {
            waitingForPlayerText.enabled = true;
        }
        else
        {
            waitingForPlayerText.enabled = false;
            gameHelp.SetActive(false);
            gameAbout.SetActive(false);
        }
    }
}
