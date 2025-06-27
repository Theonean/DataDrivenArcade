using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class FadeInOnButtonPress : MonoBehaviour
{
    [SerializeField] private KeyCode fadeInButton;
    [SerializeField] private float fadeTime;
    [SerializeField] private AnimationCurve fadeInCurve;
    public float cameraTargetDistance;
    public Camera cam;

    private void Update()
    {
        if(Input.GetKeyDown(fadeInButton))
        {
            StartCoroutine(FadeInCanvasGroup());
        }
    }

    private IEnumerator FadeInCanvasGroup() 
    {
        float t = 0;
        CanvasGroup cG = GetComponent<CanvasGroup>();

        while(t < fadeTime)
        {
            t += Time.deltaTime;
            cG.alpha = Mathf.Lerp(0, 1, fadeInCurve.Evaluate(t/fadeTime));
            cam.orthographicSize = Mathf.Lerp(4.32f, cameraTargetDistance, fadeInCurve.Evaluate(t/fadeTime));
            yield return null;
        }
    }
}
