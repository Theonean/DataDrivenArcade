using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    //This class works as an addon to the ScoreManager to handle player input and selection of Factories
    //Without this class, Scoremanager will default its selected Factory to the first index
    public PlayerFactory playerFactory;
    //Gets all the layer Information from the factoriesParent and builds the "locked layers" from it
    public GameObject factoriesParent;
    public GameObject selectionSprite;
    private Vector2 inputDir;
    private Vector2 factoryIndex;
    private int playerNum;
    private ScoreManager scoreManager;

    // Start is called before the first frame update
    void Start()
    {
        playerNum = playerFactory.playerNum;
        scoreManager = playerFactory.scoreManager;

        factoryIndex = scoreManager.selectedFactoryIndex;
        
        //DIRRRTTTYYYY but its fine for this kind of "DLC" Script I guess?
        selectionSprite.transform.position = scoreManager.challengeFactories[(int)factoryIndex.y].list[(int)factoryIndex.x].transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //Save Input Direction
        inputDir = Vector2.zero;
        inputDir.x = Input.GetAxis("P" + playerNum + "Horizontal");
        inputDir.y = Input.GetAxis("P" + playerNum + "Vertical");

        if (inputDir != Vector2.zero)
        {
            factoryIndex += inputDir;

            scoreManager.UpdateSelectedFactory(factoryIndex);
            factoryIndex = scoreManager.selectedFactoryIndex;
            
            //DIRRRTTTYYYY but its fine for this kind of "DLC" Script I guess?
            selectionSprite.transform.position = scoreManager.challengeFactories[(int)factoryIndex.y].list[(int)factoryIndex.x].transform.position;
    
        }   
    }
}
