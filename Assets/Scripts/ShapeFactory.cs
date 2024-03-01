using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class ShapeFactory : MonoBehaviour
{
    //first Input of player gives the number of sides (has to be a number between 2 and 8)
    //each input after that (until the shape has all sides) adds a line with the corresponding code to the shape
    public CustomShapeBuilder shapeBuilder;

    //Prefab for Shape
    public GameObject shapePrefab;
    
    [HideInInspector]
    protected int maxAllowedFaces = 2;

    private GameManager gm;

    protected AudioSource linePlaceSource;

    void Start() {
        gm = GameManager.instance;
        linePlaceSource = GetComponent<AudioSource>();
    }

    public void SetMaxAllowedFaces(int maxAllowedFaces){
        this.maxAllowedFaces = maxAllowedFaces;
    }

    public int GetMaxAllowedFaces(){
        return maxAllowedFaces;
    }
}
