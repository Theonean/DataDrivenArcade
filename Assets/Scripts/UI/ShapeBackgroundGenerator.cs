using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeBackgroundGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    public Vector2 gridSize;
    public float gridSpacing;
    public float scale;
    public bool liveUpdateSettings = false;

    [Header("Object Settings")]
    public bool rotateObjects = false;
    public float objectOpacity = 1f;
    private List<List<GameObject>> objects = new List<List<GameObject>>();

    [Header("Line Settings")]
    public Sprite[] lineSprites;
    public bool lineMode = false;
    [Header("Shape Settings")]
    public GameObject shapePrefab;
    public float shapeMinSides;
    public float shapeMaxSides;

    private void Start()
    {
        //Draw lines on background
        if (lineMode)
        {
            for (int x = -(int)gridSize.x / 2; x < gridSize.x / 2; x++)
            {
                List<GameObject> lineList = new List<GameObject>();

                for (int y = -(int)gridSize.y / 2; y < gridSize.y / 2; y++)
                {
                    Vector2 position = new Vector2(x * gridSpacing, y * gridSpacing);

                    GameObject line = new GameObject();
                    line.transform.parent = transform;
                    line.transform.localScale = new Vector3(scale, scale, scale);
                    line.AddComponent<SpriteRenderer>().sprite = lineSprites[Random.Range(0, lineSprites.Length)];
                    line.transform.localPosition = position;

                    //Rotate line when needed
                    if (rotateObjects)
                    {
                        line.transform.Rotate(0, 0, Random.Range(0, 360));
                    }

                    lineList.Add(line);
                }
                objects.Add(lineList);
            }
        } //Draw Shapes on background
        else
        {
            for (int x = -(int)gridSize.x / 2; x < gridSize.x / 2; x++)
            {
                List<GameObject> shapeList = new List<GameObject>();

                for (int y = -(int)gridSize.y / 2; y < gridSize.y / 2; y++)
                {
                    Vector2 position = new Vector2(x * gridSpacing, y * gridSpacing);

                    //Instantiate a new shape shapePrefab under this gameobject and initialize it with random number of sides
                    int sides = Random.Range((int)shapeMinSides, (int)shapeMaxSides);
                    GameObject shape = Instantiate(shapePrefab, new Vector3(x, y, 0), Quaternion.identity);
                    shape.transform.parent = transform;
                    shape.transform.localPosition = position;
                    shape.transform.localScale = new Vector3(scale, scale, scale);
                    shape.transform.localRotation = Quaternion.identity;
                    shape.GetComponent<CustomShapeBuilder>().InitializeShape(true, sides);

                    shapeList.Add(shape);
                }
                objects.Add(shapeList);
            }
        }

        //Go through all added objects and set the opacity on each spriterender that can be found in the object
        foreach (List<GameObject> objectList in objects)
        {
            foreach (GameObject obj in objectList)
            {
                foreach (SpriteRenderer sr in obj.GetComponentsInChildren<SpriteRenderer>())
                {
                    if (sr != null)
                    {
                        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, objectOpacity);
                    }
                }
            }
        }
    }


    // when liveupdate is set, update lines or shapes position based on gridsize and gridspacing and scale
    void Update()
    {
        if (liveUpdateSettings)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    float adaptedX = x - gridSize.x / 2;
                    float adaptedY = y - gridSize.y / 2;
                    Vector2 position = new Vector2(adaptedX * gridSpacing, adaptedY * gridSpacing);

                    objects[x][y].transform.localPosition = position;
                    objects[x][y].transform.localScale = new Vector3(scale, scale, scale);
                }
            }
        }
    }

}
