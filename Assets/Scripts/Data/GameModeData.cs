using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModeData : ScriptableObject
{
    public string gameModeName;
    public Vector2 gridSize;
    public int numPlayers;
    public float roundTime;
    public int sideMultiplierPerLevel;
    public int sideStartingLevel;
    public int shapesNeededForUnlockStart;
    public int ShapesNeededForUnlockScalePerLevel;
    public bool instantArrivalShapes;
    public bool singlePlayer;
}
