using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public enum SelectState
{
    UNSELECTABLE,
    UNSELECTED,
    SELECTED,
    LOCKED,
    LOCKEDSELECTED
}

public enum LineState
{
    REGULAR,
    HIGHLIGHT,
    STENCIL,
    SHADOW
}

public class CustomShapeBuilder : MonoBehaviour
{
    //Variables used for Randomization purposes 
    [Header("Randomization")]
    public bool randomSidesNum;
    public bool IncreaseRadiusInsteadOfReduceLineScale = true;
    //Getter and setter for state
    public SelectState selectState = SelectState.UNSELECTABLE;
    public SelectState SelectState
    {
        get { return selectState; }
        set { selectState = value; }
    }

    //Variables used for shape generation
    [Header("Shape")]
    public float maxRadius = 100.5f;
    public float radius;
    public int numSides; //Determined upon initialization
    private string shapeCode = ""; //Determined during runtime (by player) or during initialization (random, by code)
    [HideInInspector]
    public int playerNum;

    public LineTextureVariant linesRegular;
    public LineTextureVariant linesStencil;
    public LineTextureVariant linesHighlighted;
    public LineTextureVariant linesShadow;
    private SpriteRenderer[] lineSprites;

    [HideInInspector]
    public int currNodePathID = 0;

    //Variables needed for texture scaling
    private float textureHeightPixel = 10;
    private float texturePixelsPerUnityUnit = 12.5f;
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
    public Coroutine flashingRoutine;
    private float lineZDepthModifier = 0.002f;

    #region Shape Creation
    #region Shape Initialization
    //builds a shape based on the shapeCode, each shape has a different number of sides

    /// <summary>
    /// 
    /// </summary>
    /// <param name="generateShapeLines">Whether initialization should add lines (random code)</param>
    /// <param name="numberOfSides">Number of corners the shape will have</param>
    /// <param name="preShapeCode">(optional) adds lines according to the string</param>
    /// <returns></returns>
    public string InitializeShape(bool generateShapeLines, int numberOfSides, string preShapeCode = "X", LineState lineState = LineState.STENCIL)
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
                AddLine(lineCode, lineState);
            }

        }

        if (IsSelected())
        {
            //print("Initialization highlight at index " + highlightedLineIndex);
            SetLineState(0, LineState.HIGHLIGHT);
        }

        return shapeCode;
    }

    //calculates the positions for the corners of this shape based on the number of sides
    private void CreateCorners()
    {
        if (numSides > 1 && IncreaseRadiusInsteadOfReduceLineScale)
        {
            if (numSides > 1)
            {
                float fixedLineLength = textureHeightUnityUnit; // Use the fixed height of the texture
                float scaleFactor = Mathf.Lerp(2f, 1f, Mathf.InverseLerp(2, 8, numSides)); // Adjust radius for low-sided shapes
                radius = (fixedLineLength / (2 * Mathf.Sin(Mathf.PI / numSides))) * scaleFactor;
                radius = Mathf.Clamp(radius, 0, maxRadius);
            }
        }

        if (numSides == 1)
        {
            // For a single-sided shape, define two corners to represent the ends of the line
            corners = new Vector2[2]; // Adjusted to ensure space for two points
            corners[0] = new Vector2(-radius, 0); // Start at one side of the radius
            corners[1] = new Vector2(radius, 0);  // End at the other side of the radius
        }
        else
        {
            for (int i = 0; i < numSides; i++)
            {
                float angle = (360 / numSides) * i;
                Vector2 pos = new Vector2(Mathf.Sin(angle * Mathf.PI / 180) * radius, Mathf.Cos(angle * Mathf.PI / 180) * radius);
                corners[i] = pos;
            }
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

    public void DeleteLastLine()
    {
        if (linesPlaced > 0)
        {
            Destroy(lineSprites[linesPlaced - 1].gameObject);
            linesPlaced -= 1;
            shapeCode = shapeCode.Substring(0, shapeCode.Length - 1);
        }
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

        if (IsSelected() && gameObject.activeSelf) // Check if the game object is active
        {
            StartCoroutine(sap.PlayShapeCode(code));
        }

        return code;
    }

    public void OverrideAllLineStates(LineState lineState)
    {
        for (int i = 0; i < lineSprites.Length; i++)
        {
            SetLineState(i, lineState);
        }
    }

    public string GetShapecode() { return shapeCode; }

    public int GetCurrentLineCode()
    {
        return int.Parse(shapeCode[highlightedLineIndex].ToString());
    }   
    
    #endregion


    #region Shape Completion

    //adds a line between the current node and the next node in the shape based on the shapeCode
    //returns whether shape has been finished or not
    public bool AddLine(int lineCode, LineState lineState = LineState.STENCIL)
    {
        //print("adding line with code: " + lineCode);
        shapeCode += lineCode.ToString();
        //Debug.Log(shapeCode);

        //get nodes based on current and next node numbers
        int currentNodeNum = linesPlaced;
        int nextNodeNum = linesPlaced == numSides - 1 ? 0 : currentNodeNum + 1; //select the first corner for last line

        Vector2 currCorner = corners[currentNodeNum];
        Vector2 nextCorner = numSides == 1 ? corners[1] : corners[nextNodeNum]; //override for single sided shapes

        //add gameobject under node with texture2d corresponding to code
        GameObject textureObject = new();
        textureObject.transform.parent = this.transform;

        //set position of textureObject to the middle of the line
        textureObject.transform.localPosition = (currCorner + nextCorner) / 2;

        //TESTING: Maybe this is a "social workaround" thats not needed and can be solved by just plaaying the game -> lacking directional consistency 
        //if the shape only has 2 sides, push the first line to the right and the second line to the left

        if (numSides == 2)
        {
            if (currentNodeNum == 0)
            {
                textureObject.transform.localPosition = new Vector3(0, 5f / 12.5f, 0);
            }
            else
            {
                textureObject.transform.localPosition += new Vector3(0, -5f / 12.5f, 0);
            }
        }

        // Offset each line slightly downwards so they their Z-Layer corresponds to the order of creating the lines
        textureObject.transform.localPosition = new Vector3(
            textureObject.transform.localPosition.x,
            textureObject.transform.localPosition.y,
            (-corners.Length + linesPlaced) * lineZDepthModifier);

        //rotate textureObject to face the correct direction
        Vector2 nodeDir = nextCorner - currCorner;
        float angle = Mathf.Atan2(nodeDir.y, nodeDir.x) * Mathf.Rad2Deg; // Use Atan2 to get the angle in radians and convert to degrees

        textureObject.transform.rotation = Quaternion.Euler(0, 0, angle - 90); // Adjust rotation to align with Unity's coordinate system

        if (numSides == 2) textureObject.transform.rotation = Quaternion.Euler(0, 0, angle); //Adjust for two sided

        //scale textureobject.y to the correct length of nodeDir
        float nodeDirLength = nodeDir.magnitude;
        float textureLengthModifier = nodeDirLength / textureHeightUnityUnit;

        if (IncreaseRadiusInsteadOfReduceLineScale)
            textureObject.transform.localScale = new Vector3(1, 1, 1); // Keep the line size fixed
        else
            textureObject.transform.localScale = new Vector3(textureLengthModifier / 2f, textureLengthModifier / 2f, 1);

        //Create debug information to make the calculation of the scale more traceable
        //print("nodeDirLength: " + nodeDirLength + " textureHeightUnityUnit: " + textureHeightUnityUnit + " textureLengthModifier: " + textureLengthModifier);


        //print("linecode: " + lineCode.ToString());
        //Use random code variable to determine which texture to use

        SpriteRenderer newLine = textureObject.AddComponent<SpriteRenderer>();
        
        Sprite spr = null;
        switch (lineState)
        {
            case LineState.REGULAR:
                spr = linesRegular.GetLineSprite(lineCode);
                break;
            case LineState.HIGHLIGHT:
                spr = linesHighlighted.GetLineSprite(lineCode);
                break;
            case LineState.STENCIL:
                spr = linesStencil.GetLineSprite(lineCode);
                break;
            case LineState.SHADOW:
                spr = linesShadow.GetLineSprite(lineCode);
                break;
            default:
                Debug.LogError("LineState not recognized + " + lineState);
                break;
        }

        newLine.sprite = spr;
        lineSprites[linesPlaced] = newLine;

        linesPlaced += 1;


        //print("placed line between " + currCorner + " and " + nextCorner + " at angle: " + angle);
        //check if the pixelsperunit on the sprite is the same as the pixelsperunit of the texture, otherwise debug error
        if (textureObject.GetComponent<SpriteRenderer>().sprite.pixelsPerUnit != texturePixelsPerUnityUnit)
        {
            UnityEngine.Debug.LogError("Texture " + linesStencil.GetLineSprite(lineCode).name + " has a different pixels per unit than the texture, this will cause scaling issues!");
        }

        //Check if the height of the texture is the same as the height of the node, otherwise debug error
        if (textureObject.GetComponent<SpriteRenderer>().sprite.textureRect.height != textureHeightPixel)
        {
            UnityEngine.Debug.LogError("Texture " + linesStencil.GetLineSprite(lineCode).name + " has a different height than the texture, this will cause scaling issues!");
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

    private void SetLineState(int lineIndex, LineState lineState)
    {
        SpriteRenderer line = lineSprites[lineIndex];
        int lineCode = int.Parse(shapeCode[lineIndex].ToString());

        switch (lineState)
        {
            case LineState.REGULAR:
                //Duplicate the line and set the regular one over it
                line.sprite = linesRegular.GetLineSprite(lineCode);
                break;
            case LineState.HIGHLIGHT:
                line.sprite = linesHighlighted.GetLineSprite(lineCode);
                break;
            case LineState.STENCIL:
                line.sprite = linesStencil.GetLineSprite(lineCode);
                break;
            case LineState.SHADOW:
                line.sprite = linesShadow.GetLineSprite(lineCode);
                break;
        }
    }

    private void ModifyLineZDepth(bool bringToForeground, int lineIndex)
    {
        float zOffset = bringToForeground ? -lineZDepthModifier : lineZDepthModifier;
        SpriteRenderer line = lineSprites[lineIndex];
        line.transform.localPosition = new Vector3(
            line.transform.localPosition.x,
            line.transform.localPosition.y,
            line.transform.localPosition.z + zOffset * lineIndex);
    }

    public void StartLineHighlight(int playerNum, int lineIndex)
    {
        //print("registered highlight begin for player: " + playerNum + " at index: " + lineIndex);
        highlightedLineIndex = lineIndex;
        selectState = selectState == SelectState.LOCKED ? SelectState.LOCKEDSELECTED : SelectState.SELECTED;
        this.playerNum = playerNum;
        SetLineState(highlightedLineIndex, LineState.HIGHLIGHT);
        ModifyLineZDepth(true, highlightedLineIndex);

        StartCoroutine(sap.PlayShapeCode(shapeCode));
        flashingRoutine = StartCoroutine(FlashHighlight());
    }

    public void EndLineHighlight(bool isFullReset = false)
    {
        //print("Dehighlighting line " + highlightedLineIndex + " with code: " + shapeCode[highlightedLineIndex].ToString() + " for player: " + selectState.ToString() + " at index: " + highlightedLineIndex);
        if (!flashingRoutine.IsUnityNull()) StopCoroutine(flashingRoutine);

        if (selectState == SelectState.LOCKEDSELECTED)
        {
            selectState = SelectState.LOCKED;
        }
        else if (selectState == SelectState.LOCKED || selectState == SelectState.SELECTED)
        {
            selectState = SelectState.UNSELECTED;
        }

        playerNum = 0;

        if (isFullReset)
        {
            InitializeShape(true, numSides, GetShapecode());
        }
        else
        {
            SetLineState(highlightedLineIndex, LineState.STENCIL);
        }

        //Reset Z-Layers
        int iZ = 0;
        foreach (SpriteRenderer line in lineSprites)
        {
            // Offset each line slightly downwards so they their Z-Layer corresponds to the order of creating the lines
            line.transform.localPosition = new Vector3(
                line.transform.localPosition.x,
                line.transform.localPosition.y,
                (-corners.Length + iZ) * lineZDepthModifier);

            iZ++;
        }
    }

    //Flash the highlighted line on and off
    private IEnumerator FlashHighlight()
    {
        while (true)
        {
            SetLineState(highlightedLineIndex, LineState.STENCIL);
            yield return new WaitForSeconds(0.25f);
            SetLineState(highlightedLineIndex, LineState.HIGHLIGHT);
            yield return new WaitForSeconds(0.25f);
        }
    }

    public bool IsLocked()
    {
        return selectState == SelectState.LOCKED || selectState == SelectState.LOCKEDSELECTED;
    }

    public bool IsSelected()
    {
        return selectState == SelectState.SELECTED || selectState == SelectState.LOCKEDSELECTED;
    }

    public void HighlightNextLine()
    {
        if (selectState == SelectState.SELECTED)
        {
            //print("Highlighting next line for player: " + iData.playerNum + " at index: " + highlightedLineIndex + " with sides: " + numSides + " and code: " + shapeCode);
            SetLineState(highlightedLineIndex, LineState.REGULAR);
            ModifyLineZDepth(false, highlightedLineIndex);
            highlightedLineIndex = highlightedLineIndex == numSides - 1 ? 0 : highlightedLineIndex + 1;
            SetLineState(highlightedLineIndex, LineState.HIGHLIGHT);
            ModifyLineZDepth(true, highlightedLineIndex);
        }
    }

    public void HighlightLastLine(){
        if (selectState == SelectState.SELECTED && highlightedLineIndex > 0)
        {
            //print("Highlighting next line for player: " + iData.playerNum + " at index: " + highlightedLineIndex + " with sides: " + numSides + " and code: " + shapeCode);
            SetLineState(highlightedLineIndex, LineState.REGULAR);
            ModifyLineZDepth(false, highlightedLineIndex);
            highlightedLineIndex = highlightedLineIndex == 0 ? numSides - 1 : highlightedLineIndex - 1;
            SetLineState(highlightedLineIndex, LineState.HIGHLIGHT);
            ModifyLineZDepth(true, highlightedLineIndex);
        }
    }
}

#endregion