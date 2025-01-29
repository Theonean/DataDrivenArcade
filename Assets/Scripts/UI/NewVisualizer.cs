using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class NewVisualizer : MonoBehaviour
{
    public GameObject lines;
    private PlayerManager Player;
    [SerializeField] private LineTextureVariant linesRegular;
    [SerializeField] private LineTextureVariant linesPressed;
    [SerializeField] private LineTextureVariant linesGreyedOut;

    private bool active = false;

    GameManager gm;

    private void Awake()
    {
        gm = GameManager.instance;
        Player = GetComponentInParent<PlayerManager>();
        Player.OnChangeReadyState.AddListener(ToggleActive);

        PlayerInput playerInput = Player.gameObject.GetComponent<PlayerInput>();
        playerInput.actions["CreateLine1"].performed += ctx => OnButtonInput(0, ctx);
        playerInput.actions["CreateLine2"].performed += ctx => OnButtonInput(1, ctx);
        playerInput.actions["CreateLine3"].performed += ctx => OnButtonInput(2, ctx);
        playerInput.actions["CreateLine4"].performed += ctx => OnButtonInput(3, ctx);
        playerInput.actions["CreateLine5"].performed += ctx => OnButtonInput(4, ctx);
        playerInput.actions["CreateLine6"].performed += ctx => OnButtonInput(5, ctx);

        SetLinesActive(false);
    }

    public void ToggleActive(bool isActive)
    {
        //Debug.Log("Inputvisualizer toggled active " + isActive);
        gm = GameManager.instance;
        active = isActive;
        SetLinesActive(isActive);
    }

    public void OnButtonInput(int lineCode, InputAction.CallbackContext ctx)
    {
        if (!active)
            return;

        GameObject lineObject = lines.transform.GetChild(lineCode).gameObject;
        SpriteRenderer lineRenderer = lineObject.GetComponent<SpriteRenderer>();

        StartCoroutine(linePressed(lineCode, lineRenderer));

    }

    private void SetLinesActive(bool active)
    {
        int lineIndex = 0;
        foreach (SpriteRenderer line in lines.GetComponentsInChildren<SpriteRenderer>())
        {
            if (active)
            {
                line.sprite = linesRegular.GetLineSprite(lineIndex);
            }
            else
            {
                line.sprite = linesGreyedOut.GetLineSprite(lineIndex);
            }
            lineIndex++;
        }
    }

    private IEnumerator linePressed(int lineCode, SpriteRenderer lineRenderer)
    {
        lineRenderer.sprite = linesPressed.GetLineSprite(lineCode);
        yield return new WaitForSeconds(0.1f);
        lineRenderer.sprite = linesRegular.GetLineSprite(lineCode);
    }
}
