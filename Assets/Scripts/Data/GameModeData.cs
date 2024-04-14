using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameModeType
{
    CLASSIC,
    GRID,
    CUSTOM
}

[System.Serializable]
public class GameModeData
{
    public GameModeType gameMode;
    public Vector2 gridSize;
    public bool singlePlayer; //Only changed with P1 or P2 button in Main Menu
    public float roundTime;
    public int sideMultiplierPerLevel;
    public int sideStartingLevel;
    public int shapesNeededForUnlockStart;
    public int ShapesNeededForUnlockScalePerLevel;
    public bool instantArrivalShapes;
    public bool allShapesSameSpeed;

    public GameModeData()
    {
        gridSize = new Vector2(0, 0);
        singlePlayer = false;
        roundTime = 0f;
        sideMultiplierPerLevel = 0;
        sideStartingLevel = 0;
        shapesNeededForUnlockStart = 0;
        ShapesNeededForUnlockScalePerLevel = 0;
        instantArrivalShapes = false;
        allShapesSameSpeed = true;
    }

    public GameModeData(GameModeType gameMode)
    {
        switch (gameMode)
        {
            case GameModeType.CLASSIC:
                gridSize = new Vector2(2, 1);
                singlePlayer = false;
                roundTime = 60f;
                sideMultiplierPerLevel = 1;
                sideStartingLevel = 1;
                shapesNeededForUnlockStart = 0;
                ShapesNeededForUnlockScalePerLevel = 0;
                instantArrivalShapes = false;
                allShapesSameSpeed = true;
                break;
            case GameModeType.GRID:
                gridSize = new Vector2(4, 3);
                singlePlayer = false;
                roundTime = 120;
                sideMultiplierPerLevel = 1;
                sideStartingLevel = 1;
                shapesNeededForUnlockStart = 3;
                ShapesNeededForUnlockScalePerLevel = 3;
                instantArrivalShapes = false;
                allShapesSameSpeed = false;
                break;
            case GameModeType.CUSTOM:
                gridSize = new Vector2(0, 0);
                singlePlayer = false;
                roundTime = 0f;
                sideMultiplierPerLevel = 0;
                sideStartingLevel = 0;
                shapesNeededForUnlockStart = 0;
                ShapesNeededForUnlockScalePerLevel = 0;
                instantArrivalShapes = false;
                allShapesSameSpeed = true;
                break;
        }
        this.gameMode = gameMode;
    }

    public void SetField(string fieldName, string value)
    {
        switch (fieldName)
        {
            case "GridWidthValue":
                gridSize.x = float.Parse(value);
                break;
            case "GridDepthValue":
                gridSize.y = float.Parse(value);
                break;
            case "RoundTimeValue":
                roundTime = float.Parse(value);
                break;
            case "SidesYScalingValue":
                sideMultiplierPerLevel = int.Parse(value);
                break;
            case "SidesStartingValue":
                sideStartingLevel = int.Parse(value);
                break;
            case "StartingLockNumValue":
                shapesNeededForUnlockStart = int.Parse(value);
                break;
            case "LockYScalingValue":
                ShapesNeededForUnlockScalePerLevel = int.Parse(value);
                break;
            case "ShapeInstantArrivalValue":
                instantArrivalShapes = bool.Parse(value);
                break;
        }
    }

    override public string ToString()
    {
        return "GameModeData: " + gameMode + " " + gridSize + " " + singlePlayer + " " + roundTime + " " + sideMultiplierPerLevel + " " + sideStartingLevel + " " + shapesNeededForUnlockStart + " " + ShapesNeededForUnlockScalePerLevel + " " + instantArrivalShapes;
    }
}
