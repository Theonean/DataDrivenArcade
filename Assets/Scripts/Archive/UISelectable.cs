using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UISelectable : MonoBehaviour
{
    public Image objectToColour;
    public GameObject objectToScale;

    private Vector3 scaleObjectOriginalScale;
    private Coroutine scaleCoroutine;
    public bool IsSelected { get; private set; }
    public UnityEvent FinishedSelectionEvent;

    private void Start()
    {
        scaleObjectOriginalScale = objectToScale.transform.localScale;
    }

    public void Selected()
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ConfirmSelected(true));
        IsSelected = true;
    }

    public void Deselected()
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ConfirmSelected(false));
        IsSelected = false;
    }

    private IEnumerator ConfirmSelected(bool scalingDown)
    {
        objectToColour.color = scalingDown ? Color.green : Color.white;
        Vector3 targetScale = scalingDown ? Vector3.zero : scaleObjectOriginalScale;
        float scaleSpeed = 0.1f;

        while (Vector3.Distance(objectToScale.transform.localScale, targetScale) > 0.01f)
        {
            objectToScale.transform.localScale = Vector3.Lerp(objectToScale.transform.localScale, targetScale, scaleSpeed);
            yield return null;
        }

        if (scalingDown)
        {
            FinishedSelectionEvent?.Invoke();
        }
    }
}
