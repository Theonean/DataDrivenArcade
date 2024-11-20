using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameWelcome : MonoBehaviour
{
    private GameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameManager.instance;
        gm.InsertCoinPressed.AddListener(InsertCoinPressed);
    }

    private void InsertCoinPressed(bool isArcadeMode)
    {
        GameManager.SwitchScene(SceneType.LOGIN);
    }
}
