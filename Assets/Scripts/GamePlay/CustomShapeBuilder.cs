using UnityEngine;

public class CustomShapeBuilder : MonoBehaviour
{
    //Variables used for Randomization purposes 
    [Header("Randomization")]
    public bool randomSidesNum;
    private bool randomCode; //made private because code randomization is determined by if shape is created by player or code

    //Variables used for shape generation
    [Header("Shape")]
    public float radius;
    private int numSides; //Determined upon initialization
    private string shapeCode = ""; //Determined during runtime (by player) or during initialization (random, by code)

    //Switch between outline and filled textures
    public Sprite[] lineTextures;
    public Sprite[] highlightLineTextures;

    [HideInInspector]
    public int currNodePathID = 0;

    //Variables needed for texture scaling
    private float textureHeightPixel = 32;
    private float texturePixelsPerUnityUnit = 100;
    private float textureHeightUnityUnit;

    //Variables needed for outline generation
    private Vector2[] corners;
    private int linesPlaced = 0;

    //builds a shape based on the shapeCode, each shape has a different number of sides
    public string InitializeShape(bool shapeIsChallenge, int numberOfSides)
    {
        randomCode = shapeIsChallenge;

        //calculate the height of the texture in unity units, needed for proper scaling
        textureHeightUnityUnit = textureHeightPixel / texturePixelsPerUnityUnit;

        //if randomSides is true, then randomize the number of sides
        numSides = randomSidesNum ? Random.Range(2, 8) : numberOfSides;
        print("Created Shape with number of sides: " + numSides);

        //initialize array, otherwise fucky wucky
        corners = new Vector2[numSides];
        //sprint("Shape generated has " + numSides + " sides");

        CreateCorners();

        //If shape is Hole (Challenge piece), create a random code and then add lines
        if (shapeIsChallenge)
        {
            string tempShapeCode = GenerateShapeCode(numSides, false);

            //Add all lines by iterating over shapecode
            for (int i = 0; i < tempShapeCode.Length; i++)
            {
                //print("This should be one char " + tempShapeCode[i]);
                int lineCode = (int)char.GetNumericValue(tempShapeCode[i]);
                AddLine(lineCode);
            }
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
        textureObject.GetComponent<SpriteRenderer>().sprite = lineTextures[lineCode];

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

    public void ResetShape()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        linesPlaced = 0;
        shapeCode = "";
    }

    public void SetLineHighlight(int lineIndex, bool isHighlight)
    {
        if (isHighlight)
        {
            //print full debug information for highlighted line
            print("Highlighting line " + lineIndex + " with code: " + shapeCode[lineIndex].ToString());
            transform.GetChild(lineIndex).GetComponent<SpriteRenderer>().sprite = highlightLineTextures[int.Parse(shapeCode[lineIndex].ToString())];
        }
        else
        {
            print("DeHighlighting line " + lineIndex + " with code: " + shapeCode[lineIndex].ToString());
            transform.GetChild(lineIndex).GetComponent<SpriteRenderer>().sprite = lineTextures[int.Parse(shapeCode[lineIndex].ToString())];
        }
    }

    //function which generates a random shape code dependent on amount of sides
    public static string GenerateShapeCode(int numSides, bool debug = false)
    {
        string code = "";
        for (int i = 0; i < numSides; i++)
        {
            code += Random.Range(0, 6).ToString();
        }

        if (debug) { print("Generated shapecode: " + code); }

        return code;
    }

    public string GetShapecode() { return shapeCode; }
}