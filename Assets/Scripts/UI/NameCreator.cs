using System.Drawing.Text;
using UnityEngine;
using UnityEngine.UI;

public class NameCreator : MonoBehaviour
{
    //Class Description
    /*
    This class generates X Underscores next to each other which each represent a Character for the players name
    When an Underscore is selected, it blinks and the players Input can change the character at that index by either pressing up or down with the Joystick
    this Class functions as an intermediary between all the chars and the MainMenu Class 
    */
    public bool selected = false;
    //FLAG BOOLEAN TO LATER USE WHEN SWITCHING BETWEEN CONTROLLER / KEYBOARD INPUT
    public bool INPUTWITHKEYBOARD = true;
    public int nameLength = 8;
    public GameObject nameCharacterPrefab;
    private GameManager gm;
    private NameCharacter[] nameCharacters;
    private float interCharDistance = 0.3f;
    private int selectedNameCharIndex = 0;
    public Image buttonImage;
    public int playerNum;
    public Sprite editingNameSprite;
    public Sprite defaultNameSprite;

    void Start()
    {
        gm = GameManager.instance;

        nameCharacters = new NameCharacter[nameLength];

        Vector3 startOffset = new Vector3(-(nameLength * interCharDistance) / 2, 0, 0);
        for (int i = 0; i < nameLength; i++)
        {
            //Instatiate Chars in line and save to array
            nameCharacters[i] = Instantiate(nameCharacterPrefab,
                transform.position + new Vector3(i * interCharDistance, 0, -1) + startOffset,
                Quaternion.identity)
                .GetComponent<NameCharacter>();

            nameCharacters[i].transform.SetParent(transform);
            nameCharacters[i].transform.localScale = Vector3.one;
        }
    }

    void Update()
    {
        if (selected && INPUTWITHKEYBOARD && Input.inputString.Length > 0)
        {
            //Take any character input and change the character at the selected index, move the selected index to the right
            char input = Input.inputString[0];
            if (IsValidInput(input))
            {
                nameCharacters[selectedNameCharIndex].SetCharacter(input);
                MoveCharacterSelectionForward();

            }//Otherwise if backspace is pressed, remove the last character
            else if (Input.GetKeyDown(KeyCode.Backspace))
            {
                nameCharacters[selectedNameCharIndex].SetCharacter('X');
                MoveCharacterSelectionBackward();
            }//On ENTER, save name and continue scene
            else if (Input.GetKeyDown(KeyCode.Return))
            {
                //Deselect current char so it doesn't blink before scene change
                ToggleSelected();

                //Save and change scene
                SaveName();
            }
        }
    }

    private bool IsValidInput(char input)
    {
        return (input >= 'a' && input <= 'z') || (input >= 'A' && input <= 'Z');
    }

    public void ToggleSelected()
    {
        selected = !selected;
        print("NameCreator selected: " + selected);
        if (selected)
        {
            if (!INPUTWITHKEYBOARD) gm.JoystickInputEvent.AddListener(ChangeLetter);
            //Set source image to editing name sprite
            buttonImage.sprite = editingNameSprite;
        }
        else
        {
            //Disable event system for changing UI Selection
            if (!INPUTWITHKEYBOARD) gm.JoystickInputEvent.RemoveListener(ChangeLetter);
            //Set source image to default name sprite
            buttonImage.sprite = defaultNameSprite;
        }

        nameCharacters[selectedNameCharIndex].ToggleSelected();
    }

    public void SaveName()
    {
        //Deselect button
        selected = false;
        if (!INPUTWITHKEYBOARD) gm.JoystickInputEvent.RemoveListener(ChangeLetter);
        nameCharacters[selectedNameCharIndex].ToggleSelected();

        //Set source image to default name sprite
        buttonImage.sprite = defaultNameSprite;

        //Save name
        gm.SetPlayerName(playerNum, GetName());

        //change scene, if singleplayer switch to game selection, otherwise to player 2 name input
        if (gm.singlePlayer || gm.gameState == SceneType.PLAYER2NAME12)
        {
            GameManager.SwitchScene(SceneType.SELECTGAMEMODE20);
        }
        else
        {
            GameManager.SwitchScene(SceneType.PLAYER2NAME12);
        }
    }

    public string GetName()
    {
        string name = "";
        for (int i = 0; i < nameLength; i++)
        {
            name += nameCharacters[i].GetCharacter();
        }
        return name;
    }

    private void ChangeLetter(InputData iData)
    {
        if (iData.playerNum == playerNum)
        {        //Input Up/Down changes the letter and Input right/left changes the selected index
            if (iData.joystickDirection.y == 1)
            {
                nameCharacters[selectedNameCharIndex].SetCharacter(true);
            }
            else if (iData.joystickDirection.y == -1)
            {
                nameCharacters[selectedNameCharIndex].SetCharacter(false);
            }

            if (iData.joystickDirection.x == 1)
            {
                MoveCharacterSelectionForward();
            }
            else if (iData.joystickDirection.x == -1)
            {
                MoveCharacterSelectionBackward();
            }
        }

    }

    private void MoveCharacterSelectionForward()
    {
        if (selectedNameCharIndex < nameLength - 1)
        {
            nameCharacters[selectedNameCharIndex].ToggleSelected();
            selectedNameCharIndex++;
            nameCharacters[selectedNameCharIndex].ToggleSelected();
        }
    }
    private void MoveCharacterSelectionBackward()
    {
        if (selectedNameCharIndex > 0)
        {
            nameCharacters[selectedNameCharIndex].ToggleSelected();
            selectedNameCharIndex--;
            nameCharacters[selectedNameCharIndex].ToggleSelected();
        }
    }
}