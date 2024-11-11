using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[ExecuteInEditMode]
public class NameCreator : MonoBehaviour
{
    //Class Description
    /*
    This class generates X Underscores next to each other which each represent a Character for the players name
    When an Underscore is selected, it blinks and the players Input can change the character at that index by either pressing up or down with the Joystick
    this Class functions as an intermediary between all the chars and the MainMenu Class 
    */
    public bool selected = false;
    public int nameLength = 8;
    public GameObject nameCharacterPrefab;
    private GameManager gm;
    private NameCharacter[] nameCharacters;
    private float interCharDistance = 0.3f;
    private int selectedNameCharIndex = 0;
    public int playerNum;

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
        }
    }

    public void ToggleSelected()
    {
        selected = !selected;
        print("NameCreator selected: " + selected);
        if (selected)
        {
            gm.JoystickInputEvent.AddListener(ChangeLetter);
        }
        else
        {
            gm.JoystickInputEvent.RemoveListener(ChangeLetter);
        }

        nameCharacters[selectedNameCharIndex].ToggleSelected();
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
                if (selectedNameCharIndex < nameLength - 1)
                {
                    nameCharacters[selectedNameCharIndex].ToggleSelected();
                    selectedNameCharIndex++;
                    nameCharacters[selectedNameCharIndex].ToggleSelected();
                }
            }
            else if (iData.joystickDirection.x == -1)
            {
                if (selectedNameCharIndex > 0)
                {
                    nameCharacters[selectedNameCharIndex].ToggleSelected();
                    selectedNameCharIndex--;
                    nameCharacters[selectedNameCharIndex].ToggleSelected();
                }
            }
        }
    }
}