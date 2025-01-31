using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NameCharacter : MonoBehaviour
{
    //What dis class do?
    /*
    When this character is selected, it will blink and the player can change the character by pressing up or down with the joystick

    */

    [SerializeField] private TextMeshProUGUI characterText;
    [SerializeField] private Image characterUnderline;
    public Color defaultColor = Color.white;
    public Color blinkColor = Color.black;
    private float invisibleTime = 0.25f;
    private float visibleTime = 0.6f;
    private float visibilityTimer;
    private bool visible = true;
    private bool selected = false;

    private int minAllowedChar = 97;
    private int maxAllowedChar = 122;

    void Start()
    {
        visibilityTimer = visibleTime;
        SetCharacter(' ');
    }

    public void ToggleSelected()
    {
        selected = !selected;

        if (selected && (characterText.text.Length == 0 || characterText.text[0] == ' '))
        {
            SetCharacter('a');
        }

        SetVisibility(selected);

    }

    public bool isCharLegal(char c)
    {
        int charInt = (int)c;
        return charInt >= minAllowedChar && charInt <= maxAllowedChar;
    }

    //Handles setting the character either up or down
    public void SetCharacter(bool isUp)
    {
        SetVisibility(selected);
        int charInt = (int)characterText.text[0];

        if (isUp)
        {
            int nextCharInt = charInt > minAllowedChar ? charInt - 1 : maxAllowedChar;
            characterText.text = ((char)nextCharInt).ToString();
            //print("Num: " + (int)characterText.text[0]);
        }
        else
        {
            int nextCharInt = charInt < maxAllowedChar ? charInt + 1 : minAllowedChar;
            characterText.text = ((char)nextCharInt).ToString();
            //print("Num: " + (int)characterText.text[0]);
        }
    }

    public void SetCharacter(int charInt)
    {
        characterText.text = ((char)charInt).ToString();
        SetVisibility(selected);
    }

    public string GetCharacter()
    {
        return characterText.text;
    }

    void Update()
    {
        if (selected)
        {
            visibilityTimer -= Time.deltaTime;
            if (visibilityTimer <= 0)
            {
                SetVisibility(!visible);
            }
        }
    }

    private void SetVisibility(bool targetVisibility)
    {
        //Switch between two timers
        visibilityTimer = targetVisibility ? visibleTime : invisibleTime;

        //Toggle visibility
        visible = targetVisibility;
        characterText.color = visible ? blinkColor : defaultColor;
        characterUnderline.color = visible ? blinkColor : defaultColor;
    }
}
