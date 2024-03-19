using UnityEngine;

public enum SelectState
{
    UNSELECTABLE,
    UNSELECTED,
    SELECTED,
    LOCKED,
    LOCKEDSELECTED
}

public class CustomShapeBuilder : MonoBehaviour
{
    //Variables used for Randomization purposes 
    [Header("Randomization")]
    public bool randomSidesNum;
    //Getter and setter for state
    public SelectState selectState = SelectState.UNSELECTABLE;
    public SelectState SelectState
    {
        get { return selectState; }
        set { selectState = value; }
    }

    //Variables used for shape generation
    [Header("Shape")]
    public float radius;
    private int numSides; //Determined upon initialization
    private string shapeCode = ""; //Determined during runtime (by player) or during initialization (random, by code)
    [HideInInspector]
    public int playerNum;

    //Switch between outline and filled textures
    public Sprite[] lineTextures;
    public Sprite[] highlightLineTextures;
    private SpriteRenderer[] lineSprites;

    [HideInInspector]
    public int currNodePathID = 0;

    //Variables needed for texture scaling
    private float textureHeightPixel = 32;
    private float texturePixelsPerUnityUnit = 100;
    private float textureHeightUnityUnit;

    //Variables needed for outline generation
    private Vector2[] corners;
    private int linesPlaced = 0;
    //public setter for highlightedindex
    private int highlightedLineIndex = 0;
    public int HighlightedLineIndex
    {
        get { return highlightedLineIndex; }
        set { highlightedLineIndex = value; }
    }

    public ShapeAudioPlayer sap;

    #region Shape Creation
    #region Shape Initialization
    //builds a shape based on the shapeCode, each shape has a different number of sides
    public string InitializeShape(bool generateShapeLines, int numberOfSides, string preShapeCode = "X")
    {
        DestroyLines();

        //calculate the height of the texture in unity units, needed for proper scaling
        textureHeightUnityUnit = textureHeightPixel / texturePixelsPerUnityUnit;

        //if randomSides is true, then randomize the number of sides
        numSides = randomSidesNum ? Random.Range(2, 8) : numberOfSides;
        lineSprites = new SpriteRenderer[numSides];
        //print("Inizializing shape which is selected" + selectState.ToString() + " with shapecode" + preShapeCode + " with " + numSides + " sides");

        //initialize array, otherwise fucky wucky
        corners = new Vector2[numSides];
        //print("Shape generated has " + numSides + " sides");

        CreateCorners();

        //If shape is Hole (Challenge piece), create a random code and then add lines
        if (generateShapeLines)
        {
            string tempShapeCode = preShapeCode.Equals("X") ? GenerateShapeCode(numSides, false) : preShapeCode;

            //Add all lines by iterating over shapecode
            for (int i = 0; i < tempShapeCode.Length; i++)
            {
                //print("This should be one char " + tempShapeCode[i]);
                int lineCode = (int)char.GetNumericValue(tempShapeCode[i]);
                AddLine(lineCode);
            }

        }

        if (IsSelected())
        {
            //print("Initialization highlight at index " + highlightedLineIndex);
            SetLineHighlight(0, true);
        }

        return shapeCode;
    }

    //calculates the positions for the corners of this shape based on the number of sides
    private void CreateCorners()
    {
        for (int i = 0; i < numSides; i++)
        {
            float angle = (360 / numSides) * i;

            Vector2 pos = new Vector3(Mathf.Sin(angle * Mathf.PI / 180) * radius, Mathf.Cos(angle * Mathf.PI / 180) * radius);

            corners[i] = pos;
        }
    }
    #endregion

    #region Shape Management

    public void DestroyLines()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        linesPlaced = 0;
        lineSprites = new SpriteRenderer[numSides];
        highlightedLineIndex = 0;
        shapeCode = "";
    }

    //function which generates a random shape code dependent on amount of sides
    public string GenerateShapeCode(int numSides, bool debug = false)
    {
        string code = "";
        for (int i = 0; i < numSides; i++)
        {
            code += Random.Range(0, 6).ToString();
        }

        if (debug) { print("Generated shapecode: " + code); }

        if (IsSelected())
        {
            StartCoroutine(sap.PlayShapeCode(code));
        }

        return code;
    }

    public string GetShapecode() { return shapeCode; }
    #endregion


    #region Shape Completion

    //adds a line between the current node and the next node in the shape based on the shapeCode
    //returns whether shape has been finished or not
    public bool AddLine(int lineCode)
    {
        //print("adding line with code: " + lineCode);
        shapeCode += lineCode.ToString();
        //Debug.Log(shapeCode);

        //get nodes based on current and next node numbers
        int currentNodeNum = linesPlaced;
        int nextNodeNum = linesPlaced == numSides - 1 ? 0 : currentNodeNum + 1;

        Vector2 currCorner = corners[currentNodeNum];
        Vector2 nextCorner = corners[nextNodeNum];

        //add gameobject under node with texture2d corresponding to code
        GameObject textureObject = new();
        textureObject.transform.parent = this.transform;

        //set position of textureObject to the middle of the line
        textureObject.transform.localPosition = (currCorner + nextCorner) / 2;

        //if the shape only has 2 sides, push the first line to the right and the second line to the left
        if (numSides == 2)
        {
            if (currentNodeNum == 0)
            {
                textureObject.transform.localPosition += new Vector3(textureHeightUnityUnit * 0.5f, 0, 0);
            }
            else
            {
                textureObject.transform.localPosition += new Vector3(textureHeightUnityUnit * -0.5f, 0, 0);
            }
        }

        //rotate textureObject to face the correct direction
        Vector2 nodeDir = nextCorner - currCorner;
        float angle = Mathf.Atan2(nodeDir.y, nodeDir.x) * Mathf.Rad2Deg; // Use Atan2 to get the angle in radians and convert to degrees
        textureObject.transform.rotation = Quaternion.Euler(0, 0, angle - 90); // Adjust rotation to align with Unity's coordinate system

        //scale textureobject.y to the correct length of nodeDir
        float nodeDirLength = nodeDir.magnitude;
        float textureLengthModifier = nodeDirLength / textureHeightUnityUnit;
        textureObject.transform.localScale = new Vector3(1, textureLengthModifier, 1);

        //print("linecode: " + lineCode.ToString());
        //Use random code variable to determine which texture to use

        textureObject.AddComponent<SpriteRenderer>();
        SpriteRenderer newLine = textureObject.GetComponent<SpriteRenderer>();
        newLine.sprite = lineTextures[lineCode];
        lineSprites[linesPlaced] = newLine;

        linesPlaced += 1;


        //print("placed line between " + currCorner + " and " + nextCorner + " at angle: " + angle);
        //check if the pixelsperunit on the sprite is the same as the pixelsperunit of the texture, otherwise debug error
        if (textureObject.GetComponent<SpriteRenderer>().sprite.pixelsPerUnit != texturePixelsPerUnityUnit)
        {
            UnityEngine.Debug.LogError("Texture " + lineTextures[lineCode].name + " has a different pixels per unit than the texture, this will cause scaling issues!");
        }

        //Check if the height of the texture is the same as the height of the node, otherwise debug error
        if (textureObject.GetComponent<SpriteRenderer>().sprite.textureRect.height != textureHeightPixel)
        {
            UnityEngine.Debug.LogError("Texture " + lineTextures[lineCode].name + " has a different height than the texture, this will cause scaling issues!");
        }

        if (linesPlaced == numSides)
        {
            //print("Finished shape with code: " + shapeCode);
            return true;
        }
        else
        {
            return false;
        }
    }

    #endregion

    #endregion

    #region Shape Highlighting

    private void SetLineHighlight(int lineIndex, bool isHighlight)
    {
        if (isHighlight)
        {
            //print full debug information for highlighted line
            //print("Highlighting line " + lineIndex + " with code: " + shapeCode[lineIndex].ToString() + " which translates to " + int.Parse(shapeCode[lineIndex].ToString()) + " in the array");
            lineSprites[lineIndex].sprite = highlightLineTextures[int.Parse(shapeCode[lineIndex].ToString())];
        }
        else
        {
            //print("DeHighlighting line " + lineIndex + " with code: " + shapeCode[lineIndex].ToString());
            lineSprites[lineIndex].sprite = lineTextures[int.Parse(shapeCode[lineIndex].ToString())];
        }
    }

    public void StartLineHighlight(int playerNum, int lineIndex)
    {
        //print("registered highlight begin for player: " + playerNum + " at index: " + lineIndex);
        highlightedLineIndex = lineIndex;
        selectState = selectState == SelectState.LOCKED ? SelectState.LOCKEDSELECTED : SelectState.SELECTED;
        this.playerNum = playerNum;
        SetLineHighlight(highlightedLineIndex, true);

        StartCoroutine(sap.PlayShapeCode(shapeCode));

        //Subscribe to the event
        GameManager.instance.LineInputEvent.AddListener(HighlightNextLine);
    }

    public void EndLineHighlight()
    {
        //print("Dehighlighting line " + highlightedLineIndex + " with code: " + shapeCode[highlightedLineIndex].ToString() + " for player: " + selectState.ToString() + " at index: " + highlightedLineIndex);

        if (selectState == SelectState.LOCKEDSELECTED)
        {
            selectState = SelectState.LOCKED;
        }
        else if (selectState == SelectState.LOCKED || selectState == SelectState.SELECTED)
        {
            selectState = SelectState.UNSELECTED;
        }

        playerNum = 0;

        SetLineHighlight(highlightedLineIndex, false);

        GameManager.instance.LineInputEvent.RemoveListener(HighlightNextLine);
    }

    public bool IsLocked()
    {
        return selectState == SelectState.LOCKED || selectState == SelectState.LOCKEDSELECTED;
    }

    public bool IsSelected()
    {
        return selectState == SelectState.SELECTED || selectState == SelectState.LOCKEDSELECTED;
    }

    public void HighlightNextLine(InputData iData)
    {
        if (iData.playerNum == playerNum)
        {
            if (selectState == SelectState.SELECTED)
            {
                //print("Highlighting next line for player: " + iData.playerNum + " at index: " + highlightedLineIndex + " with sides: " + numSides + " and code: " + shapeCode);
                SetLineHighlight(highlightedLineIndex, false);
                highlightedLineIndex = highlightedLineIndex == numSides - 1 ? 0 : highlightedLineIndex + 1;
                SetLineHighlight(highlightedLineIndex, true);
            }
        }
    }
}

#endregion