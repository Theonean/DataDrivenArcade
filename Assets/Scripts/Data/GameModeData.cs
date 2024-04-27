using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
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
                roundTime = 120;
                sideMultiplierPerLevel = 1;
                sideStartingLevel = 1;
                shapesNeededForUnlockStart = 3;
                ShapesNeededForUnlockScalePerLevel = 3;
                instantArrivalShapes = false;
                allShapesSameSpeed = false;
                break;
            case GameModeType.CUSTOM:
                gridSize = new Vector2(4, 3);
                roundTime = 120;
                sideMultiplierPerLevel = 1;
                sideStartingLevel = 1;
                shapesNeededForUnlockStart = 3;
                ShapesNeededForUnlockScalePerLevel = 3;
                instantArrivalShapes = false;
                allShapesSameSpeed = true;
                break;
        }
        this.gameMode = gameMode;
    }

    public bool SetField(string fieldName, string value)
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
            default:
                return false;
        }
        return true;
    }

    public string GetField(string fieldName)
    {
        switch (fieldName)
        {
            case "GridWidthValue":
                return gridSize.x.ToString();
            case "GridDepthValue":
                return gridSize.y.ToString();
            case "RoundTimeValue":
                return roundTime.ToString();
            case "SidesYScalingValue":
                return sideMultiplierPerLevel.ToString();
            case "SidesStartingValue":
                return sideStartingLevel.ToString();
            case "StartingLockNumValue":
                return shapesNeededForUnlockStart.ToString();
            case "LockYScalingValue":
                return ShapesNeededForUnlockScalePerLevel.ToString();
            case "ShapeInstantArrivalValue":
                return instantArrivalShapes.ToString();
        }
        return "";
    }

    override public string ToString()
    {
        return "GameModeData: " + gameMode + " " + gridSize + " " + roundTime + " " + sideMultiplierPerLevel + " " + sideStartingLevel + " " + shapesNeededForUnlockStart + " " + ShapesNeededForUnlockScalePerLevel + " " + instantArrivalShapes;
    }
}
