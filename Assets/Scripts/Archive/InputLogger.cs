using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InputLogger : MonoBehaviour
{
    public TextMeshProUGUI logText;
    
    void Update() {
        if(Input.inputString.Length > 0) {
            logText.text = Input.inputString;
        }
    }
}
