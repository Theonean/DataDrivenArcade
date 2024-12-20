using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MainMenu : MonoBehaviour
{
    /*
        Changed the way Main Menu and UI Works now.
        Used to work so that both players have to be ready before the game starts.
        Now first player decides how many players by clicking one or two player buttons (or not pressing anything -> 1p)
        when P1 has clicked ready, the second player can start using the input.
    */
    private GameManager gm;
    public TextMeshProUGUI waitingForPlayerText;
    [SerializeField]
    private JoystickSelectable[] startSelected = new JoystickSelectable[2];
    [SerializeField]
    private InputVisualizer[] inputVisualizers = new InputVisualizer[2];
    public GameObject gameHelp;
    public GameObject gameAbout;
    public GameObject[] p2Objects;
    private string[] playersSelectedActions = new string[2];
    private bool switchingScenes = false;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameManager.instance;

        //gm.SinglePlayerPressed.AddListener(() => ToggleSingleOrMultiplayer(true));
        //gm.MultiplayerPressed.AddListener(() => ToggleSingleOrMultiplayer(false));
        Debug.LogError("Repair Input System");

        foreach (JoystickSelectable js in startSelected)
        {
            js.Selected();
        }

        inputVisualizers[0].ToggleActive(true);
        inputVisualizers[1].ToggleActive(true);

        ToggleSingleOrMultiplayer(true);
        Debug.LogError("Gamemanager is not switched to coop when player dont touch the input fields");
    }

    public void ToggleSingleOrMultiplayer(bool isSinglePlayer)
    {
        if (!switchingScenes)
        {
            switch (isSinglePlayer)
            {
                case true:
                    gm.singlePlayer = true;

                    //disable all P2 objects which also deselects selectables
                    foreach (GameObject obj in p2Objects)
                    {
                        obj.SetActive(false);
                    }

                    //If P1 has the ready to play button, immediately switch
                    if (playersSelectedActions[0].Equals("READYTOPLAY"))
                        GotoSelectionScene();

                    break;
                case false:
                    gm.singlePlayer = false;
                    startSelected[1].Selected();
                    foreach (GameObject obj in p2Objects)
                    {
                        obj.SetActive(true);
                    }
                    break;
            }
        }
    }

    public void PlayerSelectionChanged(int playerNum, string actionType)
    {
        playersSelectedActions[playerNum - 1] = actionType;

        //Dont parse empty action types
        if (actionType == "Empty") return;

        //block all actions when scene switch starts
        if (switchingScenes) return;

        if (playersSelectedActions[0].Equals(playersSelectedActions[1]) || gm.singlePlayer)
        {
            gameHelp.SetActive(false);
            gameAbout.SetActive(false);

            switch (playersSelectedActions[0])
            {
                case "READYTOPLAY":
                    GotoSelectionScene();
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
                    //if (gm.arcadeMode) GameManager.SwitchScene(SceneType.WELCOME);
                    //else Application.Quit();
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
        }
    }

    private void GotoSelectionScene()
    {
        switchingScenes = true;

        //Find both player names by searching for editable components
        string p1Name = GameObject.Find("P1NameInputField").GetComponent<Editable>().GetValue();
        //GameManager.instance.SetPlayerName(1, p1Name);
        print("Player 1 name: " + p1Name);

        if (!GameManager.instance.singlePlayer)
        {
            string p2Name = GameObject.Find("P2NameInputField").GetComponent<Editable>().GetValue();
            //GameManager.instance.SetPlayerName(2, p2Name);
            print("Player 2 name: " + p2Name);
        }

        //GameManager.SwitchScene(SceneType.GAMESELECTION);
    }
}
