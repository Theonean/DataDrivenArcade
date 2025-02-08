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

        SetLinesActive(false);
    }

    private void OnEnable() {
        PlayerInput playerInput = Player.gameObject.GetComponent<PlayerInput>();

        playerInput.actions["CreateLine1"].performed += OnLine1Input;
        playerInput.actions["CreateLine2"].performed += OnLine2Input;
        playerInput.actions["CreateLine3"].performed += OnLine3Input;
        playerInput.actions["CreateLine4"].performed += OnLine4Input;
        playerInput.actions["CreateLine5"].performed += OnLine5Input;
        playerInput.actions["CreateLine6"].performed += OnLine6Input;
    }

    private void OnDisable() {
        PlayerInput playerInput = Player.gameObject.GetComponent<PlayerInput>();

        playerInput.actions["CreateLine1"].performed -= OnLine1Input;
        playerInput.actions["CreateLine2"].performed -= OnLine2Input;
        playerInput.actions["CreateLine3"].performed -= OnLine3Input;
        playerInput.actions["CreateLine4"].performed -= OnLine4Input;
        playerInput.actions["CreateLine5"].performed -= OnLine5Input;
        playerInput.actions["CreateLine6"].performed -= OnLine6Input;
    }

    private void OnLine1Input(InputAction.CallbackContext ctx)
    {
        OnButtonInput(0, ctx);
    }

    private void OnLine2Input(InputAction.CallbackContext ctx)
    {
        OnButtonInput(1, ctx);
    }

    private void OnLine3Input(InputAction.CallbackContext ctx)
    {
        OnButtonInput(2, ctx);
    }

    private void OnLine4Input(InputAction.CallbackContext ctx)
    {
        OnButtonInput(3, ctx);
    }

    private void OnLine5Input(InputAction.CallbackContext ctx)
    {
        OnButtonInput(4, ctx);
    }

    private void OnLine6Input(InputAction.CallbackContext ctx)
    {
        OnButtonInput(5, ctx);
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
