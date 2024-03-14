using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Text;
using System.Security.Policy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    private GameManager gm;
    private MainMenu mainMenu;
    [HideInInspector]
    public bool insertedCoin = false;
    public int playerNum;

    [Header("UI Navigation")]
    public NameCreator nameCreator;
    public Image confirmButton;
    public TextMeshProUGUI confirmText;
    public float inputCooldown = 0.2f;
    private float cooldownTimer;
    private GameObject currentSelectedBG;
    [Header("Prefabs")]
    public GameObject uiSelectionPrefab;

    private Vector3 confirmTxtOriginalScale;
    private LoginScreenPlayerState loginState = LoginScreenPlayerState.WAITINGFORCOIN;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameManager.instance;
        gm.LineInputEvent.AddListener(TryEnterNameInput);

        mainMenu = GetComponent<MainMenu>();
        mainMenu.SetSelectionHandler(this, playerNum);
        cooldownTimer = inputCooldown;
        confirmTxtOriginalScale = confirmButton.transform.localScale;

        //Instance and set selection to first element of array
        currentSelectedBG = Instantiate(uiSelectionPrefab, nameCreator.transform.position, Quaternion.identity);
        currentSelectedBG.transform.position = new Vector3(currentSelectedBG.transform.position.x, currentSelectedBG.transform.position.y, 2);
    }

    // Update is called once per frame
    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        if (loginState != LoginScreenPlayerState.WAITINGFORCOIN && cooldownTimer < 0f)
        {
            Vector2 playerInput = new Vector2(Input.GetAxis("P" + playerNum + "Horizontal"), Input.GetAxis("P" + playerNum + "Vertical"));

            if (loginState != LoginScreenPlayerState.NAME_INPUTTING)
            {
                //Player Selection Management only possible when not inputting Name or looking at leaderboard (leaderboard is not implemented yet)
                if (playerInput != Vector2.zero)
                {
                    if (playerInput.y == -1)
                    {
                        loginState = LoginScreenPlayerState.NAME_SELECTED;
                        currentSelectedBG.transform.position = nameCreator.transform.position;
                        print("Name selected");
                    }
                    else if (playerInput.y == 1)
                    {
                        loginState = LoginScreenPlayerState.CONFIRM_SELECTED;
                        currentSelectedBG.transform.position = confirmText.transform.position;
                        print(nameCreator.GetName());
                    }
                    cooldownTimer = inputCooldown;
                }
            }

            //When the player has confirm selected, lerp the texts scale towards 0 and when it reaches there, colour the button green
            if (loginState == LoginScreenPlayerState.CONFIRM_SELECTED)
            {
                confirmText.transform.localScale = Vector3.Lerp(confirmText.transform.localScale, Vector3.zero, 0.1f);
                if (confirmText.transform.localScale.x < 0.1f)
                {
                    confirmButton.color = Color.green;
                    loginState = LoginScreenPlayerState.READYTOPLAY;
                    print("Ready to play for player " + playerNum);
                }
            }
            else if (confirmText.transform.localScale != confirmTxtOriginalScale && loginState != LoginScreenPlayerState.READYTOPLAY)
            {
                confirmText.transform.localScale = Vector3.Lerp(confirmText.transform.localScale, confirmTxtOriginalScale, 0.1f);
                confirmButton.color = Color.white;
            }
        }
    }

    //Connected to Line Input Event
    private void TryEnterNameInput(InputData iData)
    {
        if (iData.playerNum == playerNum)
        {
            if (loginState == LoginScreenPlayerState.NAME_SELECTED)
            {
                loginState = LoginScreenPlayerState.NAME_INPUTTING;
                nameCreator.SetSelected(true, playerNum);
                print("Inputting name for player " + playerNum);
            }
            else if (loginState == LoginScreenPlayerState.NAME_INPUTTING)
            {
                loginState = LoginScreenPlayerState.NAME_SELECTED;
                nameCreator.SetSelected(false, playerNum);
                print("Canceled Inputting name for player " + playerNum);
            }
            cooldownTimer = inputCooldown;
        }
    }

    public void CoinInserted()
    {
        insertedCoin = true;
        loginState = LoginScreenPlayerState.NAME_SELECTED;
    }

    public bool ReadyToPlay()
    {
        return loginState == LoginScreenPlayerState.READYTOPLAY;
    }
}
