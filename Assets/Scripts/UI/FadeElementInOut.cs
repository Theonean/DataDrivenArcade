using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class FadeElementInOut : MonoBehaviour
{
    public bool isVisible => GetComponent<CanvasGroup>().alpha > 0;
    public bool isFadingOut = false;
    public AnimationCurve fadeInCurve;
    public AnimationCurve fadeOutCurve;
    public float duration = 0.5f;

    private Selectable[] childInteractables;


    private void Awake()
    {
        GetComponent<CanvasGroup>().blocksRaycasts = isVisible;

        childInteractables = GetComponentsInChildren<Selectable>();
        foreach (Selectable selectable in childInteractables)
        {
            selectable.gameObject.SetActive(isVisible);
        }
    }

    public void ToggleFadeElement()
    {
        if (isVisible)
        {
            FadeElementOut();
        }
        else
        {
            FadeElementIn();
        }
    }

    public void FadeElementOut()
    {
        StopAllCoroutines();
        StartCoroutine(FadeElementInOutCoroutine(false));
        isFadingOut = true;
    }
    public void FadeElementIn()
    {
        StopAllCoroutines();
        StartCoroutine(FadeElementInOutCoroutine(true));
        isFadingOut = false;
    }

    private IEnumerator FadeElementInOutCoroutine(bool isFadeIn)
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        AnimationCurve curve = isFadeIn ? fadeInCurve : fadeOutCurve;
        float startAlpha = canvasGroup.alpha;
        float endAlpha = isFadeIn ? 1 : 0;
        float time = 0;

        while (time < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, curve.Evaluate(time / duration));
            time += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
        canvasGroup.interactable = isFadeIn;
        canvasGroup.blocksRaycasts = isFadeIn;

        //Workaround to make buttons have navigation mode "automatic" and it works
        foreach (Selectable selectable in childInteractables)
        {
            selectable.gameObject.SetActive(isFadeIn);
        }
    }
}
