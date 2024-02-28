using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChallengeFactory : ShapeFactory
{
    //Challenge Spawn Intervall
    public float spawnInterval = 5f;
    private float spawnCountdown = 0;
    public float challengeMoveSpeed = 1f;
    public string factoryName;

    public GameObject pathParent;
    private Vector3[] pathPoints;

    private int currentChallengeIndex = 0;

    //The game supports up to 2 players for now
    public List<(ScoreManager, int)> playerData = new List<(ScoreManager, int)>();

    private void Start() {
        print("Challenge Factory Start");

        //Create path points from the children of pathParent
        pathPoints = new Vector3[pathParent.transform.childCount];
        for(int i = 0; i < pathPoints.Length; i++){
            pathPoints[i] = pathParent.transform.GetChild(i).position;
        }
    }

    private void Update() {
        spawnCountdown -= Time.deltaTime;

        //Spawn new challenge shape when countdown is over
        if(spawnCountdown <= 0){
            spawnCountdown = spawnInterval;
            CreateChallenge(maxAllowedFaces);
        }

        List<CustomShapeBuilder> buildersToBeDeleted = new List<CustomShapeBuilder>();
        //Move all challenge shapes by interpolating between their current Path Point and next Path point
        foreach(CustomShapeBuilder shapeBuilder in shapeBuilders){
            int pathIndex = shapeBuilder.currNodePathID;
            
            //check if shapebuilder position has reached last position of last path point
            if(pathIndex >= pathPoints.Length - 1){
                buildersToBeDeleted.Add(shapeBuilder);
                continue;
            }
            
            Vector3 direction = pathIndex == 0 ? pathPoints[pathIndex] - this.transform.position: pathPoints[pathIndex + 1] - pathPoints[pathIndex];
            shapeBuilder.transform.position += direction.normalized * challengeMoveSpeed * Time.deltaTime;

            //check if shapebuilder has reached the current point
            if(Vector3.Distance(shapeBuilder.transform.position, pathPoints[pathIndex]) <= 0.1f){
                shapeBuilder.currNodePathID++;
            }
        }

        for (int i = 0; i < buildersToBeDeleted.Count; i++)
        {
                shapeBuilders.Remove(buildersToBeDeleted[i]);
                Destroy(buildersToBeDeleted[i].gameObject);
                currentChallengeIndex -= 1;


                print("Destroyed Shape");
        }
    }

    //Returns code of shape created by challenge
    public string CreateChallenge(int maxNumberOfFaces){
        //Instantiate a new shapebuilder and initialize it
        CustomShapeBuilder shapeBuilder = Instantiate(shapePrefab, this.transform).GetComponent<CustomShapeBuilder>();
        shapeBuilders.Add(shapeBuilder);

        for (int i = 0; i < playerData.Count; i++)
        {
            //print("Sending Data to ScoreManager " + playerData[i].Item1.gameObject.name);
            //Tell the ScoreManager that a new challenge has been added and pass the priority index of the factory for that respective scoremanager
            playerData[i].Item1.onFactoryAddedChallenge.Invoke(playerData[i].Item2);
        }

        //TODO: Do I really want the amount of faces to be random? should it not be decided by combomanager and not the shapefactory?
        //Yes-Robin I'll get to that later. For now, let's just make it random
        int randFaceNum = Random.Range(2, maxNumberOfFaces + 1);
        return shapeBuilder.InitializeShape(true, randFaceNum);
    }

    //Gets the next challenge from the builders list, if none available returns false so the scoremanager can move to another factory
    public bool GetNextChallenge(out CustomShapeBuilder shapeBuilder){
        currentChallengeIndex++;
        if(currentChallengeIndex < shapeBuilders.Count){
            print("Returning next challenge at index: " + currentChallengeIndex + " for factory: " + factoryName + " which has " + shapeBuilders.Count + " challenges left");
            shapeBuilder = shapeBuilders[currentChallengeIndex];
            return true;
        }

        //HACK, otherwise an empty factory has created a "first" shape (or their shapes were depleted) the index for the list would be wrong
        currentChallengeIndex = Mathf.Clamp(currentChallengeIndex - 1, 0, shapeBuilders.Count - 1);
        print("Next challenge Index has been determined to be: " + currentChallengeIndex + " for factory: " + factoryName);

        shapeBuilder = null;
        return false;
    }

    public void GetCurrentChallenge(out CustomShapeBuilder shapeBuilder){
        shapeBuilder = shapeBuilders[currentChallengeIndex];
    }
}
