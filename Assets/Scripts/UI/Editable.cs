using System.Collections;
using System.Collections.Generic;
using SaveSystem;
using UnityEngine;
using UnityEngine.Events;
public enum InputDataType
{
    BOOL,
    FLOAT,
    INT,
    STRING,
    VECTOR2,
}


public class Editable : MonoBehaviour, ISaveable
{
    /*
    This class may only be used in combination with a Joystickselectable
    Joystickselectable tells this script whether it's editing and allows for player to enter input or change value of an input field
    */
    public GameObject fieldValuePrefab;
    private int controlledByPlayer;
    private JoystickSelectable joystickSelectable;
    public UnityEvent<int, string> buttonPressedEvent;
    public UnityEvent<int, string> onDisabledEvent;

    private GameManager gm;
    public InputDataType inputType;
    public Vector2 minMaxValue; //Only visible with InputType float
    public string startValue; //Only visible with InputType float
    public float stepValue; //Only visible with InputType float
    public int nameLength = 8; //Only visible with InputType String
    public bool editing = false;
    private InputField[] nameCharacters;
    private float interCharDistance = 0.3f;
    private int editingNameCharIndex = 0;

    private void Awake()
    {
        gm = GameManager.instance;

        joystickSelectable = GetComponent<JoystickSelectable>();
        controlledByPlayer = joystickSelectable.controlledByPlayer;

        //Hook this class up to the JoystickSelectable
        joystickSelectable.SelectedEvent.AddListener(Activate);
        joystickSelectable.DeselectedEvent.AddListener(Deactivate);

        nameLength = inputType == InputDataType.STRING ? nameLength : 1; //Only strings get their own length
        nameCharacters = new InputField[nameLength];

        //print("Creating inputfields for " + nameLength + " characters");

        Vector3 startOffset = new Vector3(-nameLength * interCharDistance / 2, 0, 0);
        Vector3 downOffset = new Vector3(0, -0.125f, 0);
        for (int i = 0; i < nameLength; i++)
        {
            //Create new Gameobject for each Inputfield//Instatiate Chars in line and save to array
            GameObject InputFieldObject = Instantiate(fieldValuePrefab,
                transform.position + new Vector3(i * interCharDistance, 0, -1) + startOffset + downOffset,
                Quaternion.identity);
            //Reparent object to this gameobject
            InputFieldObject.transform.SetParent(transform);
            InputField inputField = InputFieldObject.GetComponent<InputField>();


            //Set data type for each inputfield
            inputField.inputDataType = inputType;
            inputField.stepValue = stepValue;
            inputField.startValue = startValue;
            inputField.minMaxValue = minMaxValue;
            nameCharacters[i] = inputField;
        }
    }

    public void SetVisibility(bool visible)
    {
        foreach (InputField inputField in nameCharacters)
        {
            inputField.gameObject.SetActive(visible);
        }
    }

    public void Activate()
    {
        gm.LineInputEvent.AddListener(ButtonPressed);
    }

    public void Deactivate()
    {
        gm.LineInputEvent.RemoveListener(ButtonPressed);
    }

    private void ButtonPressed(InputData iData)
    {
        if (iData.playerNum == controlledByPlayer)
        {
            joystickSelectable.ToggleInputLock();
            //print("Button pressed: " + iData.playerNum + " " + GetValue());
            buttonPressedEvent?.Invoke(iData.playerNum, GetValue());
            ToggleEditingInputField();
        }
    }

    private void ToggleEditingInputField()
    {
        editing = !editing;
        //print("NameCreator editing: " + editing);
        if (editing)
        {
            gm.JoystickInputEvent.AddListener(ChangeValue);
        }
        else
        {
            gm.JoystickInputEvent.RemoveListener(ChangeValue);
        }

        nameCharacters[editingNameCharIndex].ToggleSelected();
    }

    public string GetValue()
    {
        string name = "";
        for (int i = 0; i < nameLength; i++)
        {
            name += nameCharacters[i].GetValue();
        }
        return name;
    }

    private void ChangeValue(InputData iData)
    {
        //Input Up/Down changes the letter and Input right/left changes the editing index
        if (iData.playerNum == controlledByPlayer)
        {
            //Changing the value of the InputField
            if (iData.joystickDirection.y == 1)
            {
                nameCharacters[editingNameCharIndex].ChangeValueUp();
            }
            else if (iData.joystickDirection.y == -1)
            {
                nameCharacters[editingNameCharIndex].ChangeValueDown();
            }

            //Moving left and right in the selection
            if (iData.joystickDirection.x == 1)
            {
                if (editingNameCharIndex < nameLength - 1)
                {
                    nameCharacters[editingNameCharIndex].ToggleSelected();
                    editingNameCharIndex++;
                    nameCharacters[editingNameCharIndex].ToggleSelected();
                }
            }
            else if (iData.joystickDirection.x == -1)
            {
                if (editingNameCharIndex > 0)
                {
                    nameCharacters[editingNameCharIndex].ToggleSelected();
                    editingNameCharIndex--;
                    nameCharacters[editingNameCharIndex].ToggleSelected();
                }
            }
        }
    }

    void OnDisable()
    {
        print("editable disabled and we have value: " + GetValue());
        onDisabledEvent?.Invoke(controlledByPlayer, GetValue());
    }

    public void LoadData(SaveData data)
    {
        switch (gameObject.name)
        {
            case "GridWidthValue":
                startValue = data.preferredCustomSettings.gridSize.x.ToString();
                break;
            case "GridDepthValue":
                startValue = data.preferredCustomSettings.gridSize.y.ToString();
                break;
            case "RoundTimeValue":
                startValue = data.preferredCustomSettings.roundTime.ToString();
                break;
            case "SidesYScalingValue":
                startValue = data.preferredCustomSettings.sideMultiplierPerLevel.ToString();
                break;
            case "SidesStartingValue":
                startValue = data.preferredCustomSettings.sideStartingLevel.ToString();
                break;
            case "StartingLockNumValue":
                startValue = data.preferredCustomSettings.shapesNeededForUnlockStart.ToString();
                break;
            case "LockYScalingValue":
                startValue = data.preferredCustomSettings.ShapesNeededForUnlockScalePerLevel.ToString();
                break;
            case "P1NameInputField" or "P2NameInputField":
            //name is getting set in gamemanager over event call set in editor
                break;
        }
    }

    public void SaveData(SaveData data)
    {
        switch (gameObject.name)
        {
            case "P1NameInputField":
                print("saving Name data: " + GetValue());
                data.playerName = GetValue();
                break;
            case "P2NameInputField":
                print("saving Name data: " + GetValue());
                data.playerName = GetValue();
                break;
            default:
                data.preferredCustomSettings.SetField(joystickSelectable.actionType, GetValue());
                break;
        }
    }
}