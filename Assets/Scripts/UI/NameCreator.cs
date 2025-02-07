using System.Collections;
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
    [SerializeField] InputActionReference backSpaceAction;
    public bool selected = false;
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
    public InputDevice activeDevice;

    [SerializeField] private Button button;

    void Awake()
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
        CustomUIEvents.OnSavePlayerName += SaveName;
        backSpaceAction.action.performed += OnDeleteCharacter;
    }

    private void OnDisable()
    {
        CustomUIEvents.OnSavePlayerName -= SaveName;
        backSpaceAction.action.performed -= OnDeleteCharacter;

        // Unsubscribe just in case when the object is disabled
        navigateAction.action.performed -= OnNavigateActionPerformed;
        backSpaceAction.action.performed -= OnDeleteCharacter;
    }


    void Update()
    {
        if (selected && activeDevice is Keyboard && Input.inputString.Length > 0)
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

    private bool IsValidInput(char input)
    {
        return nameCharacters[selectedNameCharIndex].isCharLegal(input);
    }

    private bool IsLastCharacterInName()
    {
        return selectedNameCharIndex == nameLength - 1 ||
            nameCharacters[selectedNameCharIndex + 1].GetCharacter()[0].Equals(' ');
    }

    public void ToggleSelected(InputDevice device)
    {
        if (SceneHandler.Instance.nextScene != SceneType.EMPTY)
        {
            return;
        }

        selected = !selected;
        activeDevice = device; // Store the active device
        print("NameCreator selected: " + selected + " using device: " + device);

        if (selected)
        {
            buttonImage.sprite = editingNameSprite;
            button.navigation = new Navigation { mode = Navigation.Mode.None };

            // Subscribe to navigateAction with filtering
            navigateAction.action.performed += OnNavigateActionPerformed;
            backSpaceAction.action.performed += OnDeleteCharacter;
        }
        else
        {
            // Unsubscribe when deselecting
            navigateAction.action.performed -= OnNavigateActionPerformed;
            backSpaceAction.action.performed -= OnDeleteCharacter;
            Debug.Log("Deselected NameCreator and unsubscribed from navigateAction");

            buttonImage.sprite = defaultNameSprite;
            button.navigation = new Navigation { mode = Navigation.Mode.Automatic };

            EventSystem.current.SetSelectedGameObject(transform.parent.gameObject);
        }

        nameCharacters[selectedNameCharIndex].ToggleSelected();
    }

    public void SetName(string name) =>
        StartCoroutine(SetNameDelayed(name));

    //Coroutine because fixes bug where characters are not yet initialized and so setting them instantly doesn't work
    private IEnumerator SetNameDelayed(string name)
    {
        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < Mathf.Min(name.Length, nameLength); i++)
        {
            nameCharacters[i].SetCharacter(name[i]);
        }
    }

    private void OnNavigateActionPerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.control.device == activeDevice) // Ensure input comes from correct device
        {
            ChangeLetter(ctx);
        }
    }

    private void OnDeleteCharacter(InputAction.CallbackContext ctx)
    {
        if (ctx.control.device == activeDevice) // Ensure input comes from correct device
        {
            if (IsLastCharacterInName() && selectedNameCharIndex > 0)
            {
                nameCharacters[selectedNameCharIndex].SetCharacter(' ');
            }

            MoveCharacterSelectionBackward();
        }
    }



    public void SaveName()
    {
        //Deselect button
        selected = false;

        nameCharacters[selectedNameCharIndex].ToggleSelected();

        //EventSystem.current.SetSelectedGameObject(transform.parent.gameObject);

        //Save name
        gm.SetPlayerName(playerNum, GetName());
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