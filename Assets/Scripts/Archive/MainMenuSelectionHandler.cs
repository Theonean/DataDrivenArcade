using UnityEngine;
using UnityEngine.Events;

public enum LoginScreenPlayerState
{
    WAITINGFORCOIN,
    NAME_INPUTTING,
    NAME_SELECTED,
    CONFIRM_SELECTED,
    READYTOPLAY
}

public class MainMenuSelectionHandler : MonoBehaviour
{
    /*
        DEPRECATED, PART OF OLD UI INPUT SYSTEM, THIS ONE (OLD) WAS LINKING STUFF TOGETHER IN SCRIPT
        NOW IT'S MORE EVENTBASED AND EACH UI COMPONENT ADDS / REMOVES ITSELF FROM GAMEMANAGERS INPUT EVENTS AND THEY ARE INTERCONNECTED THROUGH EVENTS IN THE EDITOR
    */
    private GameManager gm;
    [HideInInspector]
    public bool insertedCoin = false;
    public int playerNum;

    [Header("UI Navigation")]
    public NameCreator nameCreator;
    public UISelectable confirmText;
    private GameObject currentSelectedBG;
    [Header("Prefabs")]
    public GameObject uiSelectionPrefab;
    private LoginScreenPlayerState loginState = LoginScreenPlayerState.WAITINGFORCOIN;
    public UnityEvent<LoginScreenPlayerState, int> loginStateChangedEvent;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameManager.instance;
        
        //gm.LineInputEvent.AddListener(TryEnterNameInput);
        //gm.JoystickInputEvent.AddListener(TryChangeSelection);
        Debug.LogError("Repair Input System");

        //Instance and set selection to first element of array
        currentSelectedBG = Instantiate(uiSelectionPrefab, nameCreator.transform.position, Quaternion.identity);
        currentSelectedBG.transform.position = new Vector3(currentSelectedBG.transform.position.x, currentSelectedBG.transform.position.y, 2);
    }

    // Update is called once per frame
    void TryChangeSelection(InputData iData)
    {
        if (iData.playerNum == playerNum && loginState != LoginScreenPlayerState.NAME_INPUTTING)
        {
            //Player Selection Management only possible when not inputting Name or looking at leaderboard (leaderboard is not implemented yet)

            if (iData.joystickDirection.y < 0 && loginState != LoginScreenPlayerState.NAME_SELECTED)
            {
                ChangeLoginState(LoginScreenPlayerState.NAME_SELECTED);
                confirmText.Deselected();
                currentSelectedBG.transform.position = nameCreator.transform.position;
            }
            else if (iData.joystickDirection.y > 0 && loginState != LoginScreenPlayerState.CONFIRM_SELECTED)
            {
                ChangeLoginState(LoginScreenPlayerState.CONFIRM_SELECTED);
                confirmText.Selected();
                currentSelectedBG.transform.position = confirmText.transform.position;
            }

        }
    }
    public void SetReadyToPlay()
    {
        ChangeLoginState(LoginScreenPlayerState.READYTOPLAY);
        //gm.SetPlayerName(playerNum, nameCreator.GetName());
        print("Ready to play for player " + playerNum);
    }

    private void ChangeLoginState(LoginScreenPlayerState newState)
    {
        loginState = newState;
        loginStateChangedEvent?.Invoke(loginState, playerNum);
    }

    //Connected to Line Input Event
    private void TryEnterNameInput(InputData iData)
    {
        if (iData.playerNum == playerNum)
        {
            switch (loginState)
            {
                case LoginScreenPlayerState.NAME_SELECTED:
                    ChangeLoginState(LoginScreenPlayerState.NAME_INPUTTING);
                    //nameCreator.ToggleSelected();
                    print("Inputting name for player " + playerNum);
                    break;
                case LoginScreenPlayerState.NAME_INPUTTING:
                    ChangeLoginState(LoginScreenPlayerState.NAME_SELECTED);
                    //nameCreator.ToggleSelected();
                    print("Canceled Inputting name for player " + playerNum);
                    break;
            }
        }
    }

    public void CoinInserted()
    {
        insertedCoin = true;
        loginState = LoginScreenPlayerState.NAME_SELECTED;
    }

}
