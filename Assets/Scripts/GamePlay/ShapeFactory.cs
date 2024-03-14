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

    protected GameManager gm;

    protected AudioSource linePlaceSource;

    void Awake() {
        gm = GameManager.instance;
        linePlaceSource = GetComponent<AudioSource>();
    }
}
