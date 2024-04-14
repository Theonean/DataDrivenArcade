using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audiomanager : MonoBehaviour
{
    public PlayerManager p1Manager;
    public PlayerManager p2Manager;
    public AudioSource backgroundSource;
    public AudioSource comboSource;
    public AudioClip[] comboClips;
    private int currentCombo = 0;
    private bool changeComboSource = false;

    // Update is called once per frame
    void Update()
    {
        int p1Combo = p1Manager.GetCombo();
        int p2Combo = p2Manager.GetCombo();

        //the higher the combo, the more intense the sound becomes
        int CombinedCombo = Mathf.Clamp(p1Combo + p2Combo, 0, comboClips.Length);
        if(CombinedCombo != currentCombo){
            changeComboSource = true;
        }

        //When the background source loops, we need to change the combo source
        //no goddamn idea if this code works
        if(changeComboSource){
            if(backgroundSource.isPlaying && backgroundSource.time >= backgroundSource.clip.length - 0.1f){
                changeComboSource = false;
                comboSource.clip = comboClips[CombinedCombo];
                comboSource.volume = CombinedCombo / 100f;

                //Activate if not already playing
                if(!comboSource.isPlaying){
                    comboSource.Play();
                }
            }
        }
    }
}
