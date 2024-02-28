using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChallengeFactory : ShapeFactory
{
    public string factoryName;

    private void Start() {
        print("Challenge Factory Start");
    }

    //Returns code of shape created by challenge
    public string CreateChallenge(){
        shapeBuilder.ResetShape();
        return shapeBuilder.InitializeShape(true, maxAllowedFaces);
    }
}
