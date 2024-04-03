using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ShapeAudioPlayer : MonoBehaviour
{
    public AudioClip[] lineClips = new AudioClip[6];
    public AudioClip shapeRight;
    public AudioClip shapeWrong;
    private AudioSource audioSource;

    private bool playingShapeFinished = false;

    private void Start()
    {
        audioSource = this.AddComponent<AudioSource>();
    }

    public void PlayLinePlaced(int lineCode)
    {
        audioSource.PlayOneShot(lineClips[lineCode]);
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
            audioSource.pitch = 0.5f + (combo * 0.05f);
            audioSource.PlayOneShot(shapeRight);
            StartCoroutine(UnlockAudio(shapeRight.length));
        }
        else
        {
            audioSource.PlayOneShot(shapeWrong);
            StartCoroutine(UnlockAudio(shapeWrong.length));
        }
    }

    public void StopCurrentAudio()
    {
        StartCoroutine(QuickSilentFade());
    }

    private IEnumerator PlayLineAfterDelay(int lineIndex, string shapeCode)
    {
        yield return new WaitForSeconds(0.25f * lineIndex);
        audioSource.PlayOneShot(lineClips[int.Parse(shapeCode[lineIndex].ToString())]);
    }

    private IEnumerator UnlockAudio(float waitTime)
    {
        //print("Waiting for amount: " + waitTime);
        yield return new WaitForSeconds(waitTime);
        audioSource.pitch = 1f;
        playingShapeFinished = false;
        //print("Unlocked audio");
    }

    //Quickly fade out audio to avoid clicks
    private IEnumerator QuickSilentFade()
    {
        Debug.LogWarning("Quickly fading out audio still unreliable, sometimes not stopping playback?");
        //Check if audio is playing (for efficiency I guess? ~ tired code writing yeehaw)
        if (audioSource.isPlaying && !playingShapeFinished)
        {
            float originalVolume = audioSource.volume;
            float decreaseVolumeSpeed = 5f;
            while (audioSource.volume > 0)
            {
                audioSource.volume -= originalVolume * Time.deltaTime * decreaseVolumeSpeed;
                print("volume: " + audioSource.volume + " originalVolume: " + originalVolume + " Time.deltaTime: " + Time.deltaTime + " decreaseVolumeSpeed: " + decreaseVolumeSpeed);
                yield return null;
            }
            audioSource.Stop();
            audioSource.volume = originalVolume;
        }
    }
}
