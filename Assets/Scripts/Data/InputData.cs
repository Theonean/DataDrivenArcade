using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputData
{
    public int lineCode;
    public int playerNum;

    /// <summary>
    /// Generates a new InputData object with the given lineCode and playerNum
    /// </summary>
    /// <param name="lCode"></param>
    /// <param name="pNum"></param>
    public InputData(int lCode, int pNum){
        lineCode = lCode;
        playerNum = pNum;
    }
    
    /// <summary>
    /// Generates a new InputData object with the given playerNum and a default lineCode of -1
    /// </summary>
    /// <param name="pNum"></param>
    public InputData(int pNum){
        lineCode = -1;
        playerNum = pNum;
    }
}
