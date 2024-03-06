using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameCreator : MonoBehaviour
{
    //Class Description
    /*
    This class generates X Underscores next to each other which each represent a Character for the players name
    When an Underscore is selected, it blinks and the players Input can change the character at that index by either pressing up or down with the Joystick
    this Class functions as an intermediary between all the chars and the MainMenu Class 
    */
    private bool selected = false;
    public int nameLength = 8;
    public GameObject nameCharacterPrefab;
    private NameCharacter[] nameCharacters;
    private float interCharDistance = 0.3f;
    private int selectedNameCharIndex = 0;
    private int playerNum;

    private float timeBetweenMoves = 0.1f;
    private float timeSinceLastMove;

    void Start()
    {
        timeSinceLastMove = timeBetweenMoves;
        nameCharacters = new NameCharacter[nameLength];

        Vector3 startOffset = new Vector3(-(nameLength * interCharDistance) / 2, 0, 0);
        for (int i = 0; i < nameLength; i++)
        {
            //Instatiate Chars in line and save to array
            nameCharacters[i] = Instantiate(nameCharacterPrefab,
                transform.position + new Vector3(i * interCharDistance, 0, -1) + startOffset,
                Quaternion.identity)
                .GetComponent<NameCharacter>();
        }
    }

    public void SetSelected(bool isSelected, int controlledByPlayer)
    {
        selected = isSelected;
        playerNum = controlledByPlayer;

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

    void Update()
    {
        if (selected)
        {
            timeSinceLastMove -= Time.deltaTime;
            if (timeSinceLastMove <= 0)
            {
                timeSinceLastMove = timeBetweenMoves;

                Vector2 inputDir = Vector2.zero;
                inputDir.x = Input.GetAxis("P" + playerNum + "Horizontal");
                inputDir.y = Input.GetAxis("P" + playerNum + "Vertical");

                //Input Up/Down changes the letter and Input right/left changes the selected index
                if (inputDir.y == 1)
                {
                    nameCharacters[selectedNameCharIndex].SetCharacter(true);
                }
                else if (inputDir.y == -1)
                {
                    nameCharacters[selectedNameCharIndex].SetCharacter(false);
                }

                if (inputDir.x == 1)
                {
                    if (selectedNameCharIndex < nameLength - 1)
                    {
                        nameCharacters[selectedNameCharIndex].ToggleSelected();
                        selectedNameCharIndex++;
                        nameCharacters[selectedNameCharIndex].ToggleSelected();
                    }
                }
                else if (inputDir.x == -1)
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
}
