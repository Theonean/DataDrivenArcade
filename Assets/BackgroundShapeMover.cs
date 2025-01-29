using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class BackgroundShapeMover : MonoBehaviour
{
    public GameObject[] pathPoints;
    public GameObject shapePrefab;

    public float shapeScaleFactory = 1.0f;
    public float moveSpeed = 1.0f;
    private float trueMoveSpeed;
    public bool FastForward = false;
    public float fastForwardFactor = 2.0f;
    public int shapeNumSides;
    public bool randomShapeNumSides;
    public Vector2Int randomShapeNumSidesRange;
    public float spawnInterval = 1.0f;
    private float trueSpawnInterval;
    private float spawnTimer = 0.0f;

    //Array of shapes with their current path index
    private List<(GameObject, int)> shapes = new List<(GameObject, int)>();

    private void Update()
    {
        if (FastForward)
        {
            trueMoveSpeed = moveSpeed * fastForwardFactor;
            trueSpawnInterval = spawnInterval / fastForwardFactor;
        }
        else
        {
            trueMoveSpeed = moveSpeed;
            trueSpawnInterval = spawnInterval;
        }

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= trueSpawnInterval)
        {
            spawnTimer = 0.0f;
            SpawnShape();
        }

        MoveShapes();
    }

    private void SpawnShape()
    {
        GameObject shape = Instantiate(shapePrefab, pathPoints[0].transform.position, Quaternion.identity);
        CustomShapeBuilder builder = shape.GetComponent<CustomShapeBuilder>();

        int numSides = randomShapeNumSides ? Random.Range(randomShapeNumSidesRange.x, randomShapeNumSidesRange.y) : shapeNumSides;
        string shapeCode = builder.GenerateShapeCode(numSides);

        builder.InitializeShape(true, numSides, shapeCode, LineState.REGULAR);

        shape.transform.SetParent(transform);
        shape.transform.localScale = new Vector3(shapeScaleFactory, shapeScaleFactory, shapeScaleFactory);

        //Add shape to array
        shapes.Add((shape, 0));
    }

    private void MoveShapes()
    {
        List<(GameObject, int)> shapesToDelete = new List<(GameObject, int)>();
        foreach (var shapeData in shapes.ToArray())
        {
            GameObject shape = shapeData.Item1;
            int pathIndex = shapeData.Item2;

            if (pathIndex < pathPoints.Length - 1)
            {
                Vector3 targetPos = pathPoints[pathIndex + 1].transform.position;
                Vector3 direction = (targetPos - shape.transform.position).normalized;
                shape.transform.position += Time.deltaTime * trueMoveSpeed * direction;

                if (Vector3.Distance(shape.transform.position, targetPos) < 0.1f)
                {
                    int newIndex = shapes.IndexOf(shapeData);
                    shapes[newIndex] = (shape, pathIndex + 1);
                }
            }
            else
            {
                shapesToDelete.Add(shapeData);
            }
        }

        foreach (var shapeData in shapesToDelete)
        {
            shapes.Remove(shapeData);
            Destroy(shapeData.Item1);
        }
    }
}
