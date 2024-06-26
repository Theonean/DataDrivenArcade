using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InputField : MonoBehaviour
{
    //What dis class do?
    /*
    When this character is selected, it will blink and the player can change the character by pressing up or down with the joystick

    */
    public InputDataType inputDataType;
    public Vector2 minMaxValue; //Only visible with InputType float
    public string startValue; //Only visible with InputType float
    public float stepValue;
    public TextMeshPro characterText;
    public GameObject underline;
    private Vector3 underlineStartPosition;
    private float invisibleTime = 0.25f;
    private float visibleTime = 0.6f;
    private float visibilityTimer;
    private bool visible = true;
    private bool selected = false;

    private void Start()
    {
        visibilityTimer = visibleTime;

        SetValue(startValue);
        underlineStartPosition = underline.transform.localPosition;
    }

    public void ChangeValueUp()
    {
        if (selected)
        {
            switch (inputDataType)
            {
                case InputDataType.BOOL:
                    SetValue((characterText.text == "X") ? " " : "X");
                    break;
                case InputDataType.FLOAT:
                    float textValue = float.Parse(characterText.text);

                    //if this doesn't make us go out of bounds, add the value
                    if (textValue + stepValue <= minMaxValue.y)
                        SetValue((textValue + stepValue).ToString());

                    break;
                case InputDataType.INT:
                    int textValueInt = int.Parse(characterText.text);
                    //if this doesn't make us go out of bounds, add the value
                    if (textValueInt + Mathf.CeilToInt(stepValue) <= minMaxValue.y)
                        SetValue((textValueInt + Mathf.CeilToInt(stepValue)).ToString());
                    break;
                case InputDataType.STRING:
                    int charInt = (int)characterText.text[0];
                    int nextCharInt = charInt < 90 ? charInt + 1 : 65;
                    SetValue(((char)nextCharInt).ToString());
                    break;
            }
        }
    }

    private void SetValue(string value)
    {
        if (inputDataType == InputDataType.BOOL)
        {
            characterText.text = value == "true" ? "X" : " ";
        }
        else
        {
            characterText.text = value;
        }

        //Scale the underline so its same size as the text
        underline.transform.localScale = new Vector3(characterText.preferredWidth * 0.9f, underline.transform.localScale.y, 1f);
        underline.transform.position = new Vector3(characterText.transform.position.x + characterText.preferredWidth, underline.transform.position.y, underline.transform.position.z);
    }

    public void ChangeValueDown()
    {
        if (selected)
        {
            switch (inputDataType)
            {
                case InputDataType.BOOL:
                    characterText.text = (characterText.text == "X") ? " " : "X";
                    break;
                case InputDataType.FLOAT or InputDataType.FLOAT:
                    float textValue = float.Parse(characterText.text);

                    //if this doesn't make us go out of bounds, add the value
                    if (textValue - stepValue >= minMaxValue.x)
                        characterText.text = (textValue - stepValue).ToString();
                    break;
                case InputDataType.INT:
                    int textValueInt = int.Parse(characterText.text);
                    //if this doesn't make us go out of bounds, subtract the value
                    if (textValueInt - Mathf.CeilToInt(stepValue) >= minMaxValue.x)
                        characterText.text = (textValueInt - Mathf.CeilToInt(stepValue)).ToString();
                    break;
                case InputDataType.STRING:
                    int charInt = (int)characterText.text[0];
                    int nextCharInt = charInt > 65 ? charInt - 1 : 90;
                    characterText.text = ((char)nextCharInt).ToString();
                    //print(charInt + " " + characterText.text);
                    break;
                case InputDataType.VECTOR2:
                    break;
            }

            SetVisibility(true);
        }
    }


    public string GetValue()
    {
        if (inputDataType == InputDataType.BOOL)
        {
            return characterText.text == "X" ? "true" : "false";
        }
        else
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
    public void ToggleSelected()
    {
        selected = !selected;

        if (!selected)
        {
            characterText.color = Color.black;
        }
        else
        {
            SetVisibility(true);
        }
    }

    public void SetVisibility(bool targetVisibility)
    {
        //Switch between two timers
        visibilityTimer = targetVisibility ? visibleTime : invisibleTime;

        //Toggle visibility
        visible = targetVisibility;
        characterText.color = visible ? Color.white : Color.clear;
    }
}
