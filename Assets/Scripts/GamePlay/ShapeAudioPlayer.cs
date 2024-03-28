using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeAudioPlayer : MonoBehaviour
{
    public AudioClip[] lineClips = new AudioClip[6];
    public AudioClip shapeRight;
    public AudioClip shapeWrong;

    private bool playingShapeFinished = false;

    public void PlayLinePlaced(int lineCode)
    {
        AudioSource.PlayClipAtPoint(lineClips[lineCode], Vector3.zero);
    }

    public IEnumerator PlayShapeCode(string shapeCode)
    {
        while (playingShapeFinished)
        {
            //print("Waiting for audio to finish");
            yield return new WaitForEndOfFrame();
        }

        //Wait for a fraction of a second so sounds aren't played too fast
        yield return new WaitForSeconds(0.3f);

        //Iterate through shapeCode and play the corresponding lineClip when the previous line clip has ended
        //This is done to prevent the audio from playing too fast
        for (int i = 0; i < shapeCode.Length; i++)
        {
            StartCoroutine(PlayLineAfterDelay(i, shapeCode));
        }
    }

    /// <summary>
    /// Plays the line clip for the given index after a delay with pitch corresponding to the combo
    /// </summary>
    /// <param name="isCorrect"></param>
    /// <param name="combo"></param>
    public void playShapeFinished(bool isCorrect, int combo)
    {
        playingShapeFinished = true;
        //print("Locked audio");
        if (isCorrect)
        {
            AudioSource.PlayClipAtPoint(shapeRight, Vector3.zero);
            StartCoroutine(UnlockAudio(shapeRight.length));
        }
        else
        {
            AudioSource.PlayClipAtPoint(shapeWrong, Vector3.zero);
            StartCoroutine(UnlockAudio(shapeWrong.length));
        }
    }

    public void StopAllSounds()
    {
        Debug.LogWarning("Tried to stop sounds but StopAllSounds is not implemented");
    }

    private IEnumerator PlayLineAfterDelay(int lineIndex, string shapeCode)
    {
        yield return new WaitForSeconds(0.25f * lineIndex);
        AudioSource.PlayClipAtPoint(lineClips[int.Parse(shapeCode[lineIndex].ToString())], Vector3.zero);
    }

    private IEnumerator UnlockAudio(float waitTime)
    {
        //print("Waiting for amount: " + waitTime);
        yield return new WaitForSeconds(waitTime);
        playingShapeFinished = false;
        //print("Unlocked audio");
    }
}
