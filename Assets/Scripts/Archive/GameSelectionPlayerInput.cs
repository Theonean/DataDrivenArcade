using UnityEngine;

namespace Archive
{
    public enum SelectionState
    {
        GameModeClassic = 0,
        GameModeGrid = 1,
        GameModeCustom = 2,
        ChallengeFactory = 3,
    }
}

public class GameSelectionPlayerInput : MonoBehaviour
{
    /*
        What does this script do?
        It receives the Input of a specific player and Selects / Deselects the necessary Components in the UI
        Because the Input System is custom with the Joystick we need to hardcode this sheeez
        -> I don't know a smarter way to do this tbh and at some point I have to start working on it and that point is now
    */

    public int playerNum;
    public UIGameModeSelectable[] gameModes = new UIGameModeSelectable[3];
    private ChallengeFactory challengeFactory;
    private PlayerManager playerManager;
    private Archive.SelectionState selectionState = Archive.SelectionState.ChallengeFactory;
    private bool hasNewGameSelected = true;
    private GameObject[] selectables = new GameObject[5];
    private GameManager gm;

    private void Start()
    {
        playerManager = GetComponentInChildren<PlayerManager>();
        challengeFactory = GetComponentInChildren<ChallengeFactory>();
        gm = GameManager.instance;

        //gm.JoystickInputEvent.AddListener(MoveSelection);
        Debug.LogError("Repair Input System");

        selectables = new GameObject[] {
            gameModes[0].gameObject,
            gameModes[1].gameObject,
            gameModes[2].gameObject,
            challengeFactory.gameObject,
        };

        challengeFactory.ResetCF();
        playerManager.ReadyPlayer();
    }

    private void MoveSelection(InputData iData)
    {
        if (iData.playerNum == playerNum)
        {
            //Moving up the selectables if joystick pushed up and not already at the top
            if (iData.joystickDirection.y < 0 && (int)selectionState > 0)
            {
                selectionState--;
                //Selection between 0 and inclusive 2 are GameModeSelectables
                if ((int)selectionState <= 2)
                {
                    if ((int)selectionState == 2)
                        playerManager.UnreadyPlayer();

                    gameModes[(int)selectionState].Selected(playerNum, hasNewGameSelected);

                    //deselect previous gameModeSelectable if it was selected
                    if ((int)selectionState < 2)
                    {
                        gameModes[(int)selectionState + 1].Deselected(playerNum);
                    }
                }

            }
            //Moving down the selectables if joystick pushed down and not already at the bottom
            else if (iData.joystickDirection.y > 0 && (int)selectionState < 3)
            {
                selectionState++;

                //Selection between 0 and inclusive 2 are GameModeSelectables
                if ((int)selectionState <= 2)
                {
                    gameModes[(int)selectionState].Selected(playerNum, hasNewGameSelected);
                    gameModes[(int)selectionState - 1].Deselected(playerNum);
                }
                else
                {
                    //3 means we switched from gamemodeselectables to challengefactory, so deselect last gamemodeselectable
                    if ((int)selectionState == 3)
                        gameModes[(int)selectionState - 1].Deselected(playerNum);

                    playerManager.ReadyPlayer();
                }
            }

            //If joystick pushed left or right, toggle between new game and game info
            if (iData.joystickDirection.x != 0)
            {
                if ((int)selectionState <= 2)
                {
                    hasNewGameSelected = !hasNewGameSelected;
                    print("New Game Selected: " + hasNewGameSelected);
                    gameModes[(int)selectionState].SwitchSelection(playerNum);
                }
                else
                {
                    //On move to the left, decrease challenge factory shape sides and vice versa
                    if (iData.joystickDirection.x < 0 && challengeFactory.maxFacesFloorMIN > 1)
                    {
                        challengeFactory.maxFacesFloorMIN -= 1;
                        challengeFactory.ResetCF();
                        playerManager.playerShape.InitializeShape(false, challengeFactory.maxFacesFloorMIN);
                    }
                    else if (iData.joystickDirection.x > 0 && challengeFactory.maxFacesFloorMIN < 25)
                    {
                        challengeFactory.maxFacesFloorMIN += 1;
                        challengeFactory.ResetCF();
                        playerManager.playerShape.InitializeShape(false, challengeFactory.maxFacesFloorMIN);
                    }
                }
            }
        }
    }
}
