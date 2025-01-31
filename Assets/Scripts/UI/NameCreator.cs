using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class NameCreator : MonoBehaviour
{
    //Class Description
    /*
    This class generates X Underscores next to each other which each represent a Character for the players name
    When an Underscore is selected, it blinks and the players Input can change the character at that index by either pressing up or down with the Joystick
    this Class functions as an intermediary between all the chars and the MainMenu Class 
    */
    [SerializeField] InputActionReference backsapceAction;
    public bool selected = false;
    //FLAG BOOLEAN TO LATER USE WHEN SWITCHING BETWEEN CONTROLLER / KEYBOARD INPUT
    public bool INPUTWITHKEYBOARD = true;
    public int nameLength = 8;
    public GameObject nameCharacterPrefab;
    public InputActionReference navigateAction;
    private GameManager gm;
    private NameCharacter[] nameCharacters;
    private float interCharDistance = 0.3f;
    private int selectedNameCharIndex = 0;
    public Image buttonImage;
    public int playerNum;
    public Sprite editingNameSprite;
    public Sprite defaultNameSprite;
    [SerializeField] private Button button;

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

    private void OnEnable()
    {
        CustomUIEvents.OnToggleNameInputSelected += ToggleSelected;
        CustomUIEvents.OnSavePlayerName += SaveName;
        backsapceAction.action.performed += OnDeleteCharacter;
    }

    private void OnDisable()
    {
        CustomUIEvents.OnToggleNameInputSelected -= ToggleSelected;
        CustomUIEvents.OnSavePlayerName -= SaveName;
        backsapceAction.action.performed -= OnDeleteCharacter;
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
            }
        }
    }

    private void OnDeleteCharacter(InputAction.CallbackContext ctx)
    {
        if (IsLastCharacterInName() && selectedNameCharIndex > 0)
        {
            nameCharacters[selectedNameCharIndex].SetCharacter(' ');
        }

        MoveCharacterSelectionBackward();
    }

    private bool IsValidInput(char input)
    {
        return nameCharacters[selectedNameCharIndex].isCharLegal(input);
    }

    private bool IsLastCharacterInName()
    {
        return selectedNameCharIndex == nameLength - 1 ||
            nameCharacters[selectedNameCharIndex + 1].GetCharacter()[0].Equals(' ');
    }

    public void ToggleSelected()
    {
        if (SceneHandler.Instance.nextScene != SceneType.EMPTY)
        {
            return;
        }

        selected = !selected;
        print("NameCreator selected: " + selected);
        if (selected)
        {
            // Set source image to editing name sprite
            buttonImage.sprite = editingNameSprite;
            button.navigation = new Navigation { mode = Navigation.Mode.None };

            // Subscribe to the navigateAction event
            navigateAction.action.performed += OnNavigateActionPerformed;
        }
        else
        {
            // Unsubscribe from the navigateAction event
            navigateAction.action.performed -= OnNavigateActionPerformed;
            Debug.Log("Deselected NameCreator and unsubscribed from navigateAction");

            buttonImage.sprite = defaultNameSprite;
            button.navigation = new Navigation { mode = Navigation.Mode.Automatic };

            EventSystem.current.SetSelectedGameObject(transform.parent.gameObject);
        }

        nameCharacters[selectedNameCharIndex].ToggleSelected();
    }
    private void OnNavigateActionPerformed(InputAction.CallbackContext ctx)
    {
        ChangeLetter(ctx);
    }


    public void SaveName()
    {
        //Deselect button
        selected = false;

        nameCharacters[selectedNameCharIndex].ToggleSelected();

        //EventSystem.current.SetSelectedGameObject(transform.parent.gameObject);

        //Save name
        gm.SetPlayerName(playerNum, GetName());

        //change scene, if singleplayer switch to game selection, otherwise to player 2 name input
        if (gm.singlePlayer || SceneHandler.Instance.currentScene == SceneType.PLAYER2NAME12)
        {
            SceneHandler.Instance.SwitchScene(SceneType.SELECTGAMEMODE20);
        }
        else
        {
            SceneHandler.Instance.SwitchScene(SceneType.PLAYER2NAME12);
        }
    }

    public string GetName()
    {
        string name = "";
        for (int i = 0; i < nameLength; i++)
        {
            name += nameCharacters[i].GetCharacter();
        }
        name.TrimEnd(' ');

        return name;
    }

    private void ChangeLetter(InputAction.CallbackContext ctx)
    {
        if (!selected)
        {
            return;
        }

        Vector2 direction = ctx.ReadValue<Vector2>();
        //Exclude 'a' and 'd' from navigation because we don't want to move selected character with A/D when inputting name
        if (direction.x == 1 && ctx.control.name != "d")
        {
            MoveCharacterSelectionForward();
        }//Check direction left and not 'a'
        else if (direction.x == -1 && ctx.control.name != "a")
        {
            MoveCharacterSelectionBackward();
        }


        //Input Up/Down changes the letter and Input right/left changes the selected index
        if (direction.y == 1)
        {
            nameCharacters[selectedNameCharIndex].SetCharacter(true);
        }
        else if (direction.y == -1)
        {
            nameCharacters[selectedNameCharIndex].SetCharacter(false);
        }
    }

    private void MoveCharacterSelectionForward()
    {
        if (nameCharacters[selectedNameCharIndex].GetCharacter()[0] == ' ')
        {
            return;
        }

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