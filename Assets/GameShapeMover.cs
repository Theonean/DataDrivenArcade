using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameShapeMover : MonoBehaviour
{
    [SerializeField] private PlayerManager player1;
    [SerializeField] private PlayerManager player2;
    [SerializeField] private GameObject shapePrefab;

    [SerializeField] private List<Transform> player1Waypoints; // Path for Player 1
    [SerializeField] private List<Transform> player2Waypoints; // Path for Player 2

    private void OnEnable()
    {
        player1.OnFinishedShape.AddListener((shapeData) => StartCoroutine(HandleFinishedShape(0, shapeData, player1Waypoints)));
        if (player2 != null) player2.OnFinishedShape.AddListener((shapeData) => StartCoroutine(HandleFinishedShape(1, shapeData, player2Waypoints)));
    }

    private void OnDisable()
    {
        player1.OnFinishedShape.RemoveAllListeners();
        if (player2 != null) player2.OnFinishedShape.RemoveAllListeners();
    }

    private IEnumerator HandleFinishedShape(int playernum, string shapeData, List<Transform> waypoints)
    {
        yield return new WaitForSeconds(0.5f);
        GameObject shape = ConstructShape(shapeData, playernum);
        StartCoroutine(MoveShapeAlongPath(shape, waypoints));
    }

    private GameObject ConstructShape(string shapeData, int playerNum)
    {
        // Choose a starting position based on the player
        Vector3 startPosition = playerNum == 0 ?
            player1.selectedFactory.transform.position :
            player2.selectedFactory.transform.position;

        GameObject shape = Instantiate(shapePrefab, startPosition, Quaternion.identity);
        CustomShapeBuilder shapeBuilder = shape.GetComponent<CustomShapeBuilder>();
        shapeBuilder.InitializeShape(true, shapeData.Length, shapeData, LineState.REGULAR);

        float targetScaleMultiplier = Mathf.Lerp(1, 0.25f, shapeBuilder.radius / shapeBuilder.maxRadius);
        shape.transform.localScale = new Vector3(targetScaleMultiplier, targetScaleMultiplier, targetScaleMultiplier);

        return shape;
    }

    private IEnumerator MoveShapeAlongPath(GameObject shape, List<Transform> waypoints)
    {
        foreach (Transform waypoint in waypoints)
        {
            while (Vector3.Distance(shape.transform.position, waypoint.position) > 0.1f)
            {
                shape.transform.position = Vector3.MoveTowards(shape.transform.position, waypoint.position, Time.deltaTime * 5f);
                yield return null;
            }
        }

        // Destroy the shape when it reaches the final waypoint
        Destroy(shape);
    }
}
