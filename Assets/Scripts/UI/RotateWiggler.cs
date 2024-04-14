using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RotateWiggler : MonoBehaviour
{
    [Header("Rotation")]
    public bool rotate = true;
    public float rotationSpeed;
    public Vector2 rotationRange;
    public AnimationCurve rotationEasingCurve;

    [Header("Scaling")]
    public bool scale = true;
    public float scaleSpeed;
    public Vector2 scaleRange;
    public AnimationCurve scaleEasingCurve;

    // Internal variables to track time
    private float rotationTime;
    private float scaleTime;

    [Header("FontGradient")]
    public bool colorChange = true;
    public TextMeshProUGUI textComponent;
    public float colorChangeInterval;
    private float colorChangeTimer;

    private Color targetTopLeft;
    private Color targetTopRight;
    private Color targetBottomLeft;
    private Color targetBottomRight;

    void Start()
    {
        // Initialize middle values
        if (rotate) transform.localEulerAngles = new Vector3(0, 0, (rotationRange.x + rotationRange.y) / 2);
        if (scale) transform.localScale = Vector3.one * (scaleRange.x + scaleRange.y) / 2;

        // Initialize colors
        if (textComponent != null && colorChange)
        {
            targetTopLeft = Random.ColorHSV(0, 1, 0.9f, 1, 0.9f, 1);
            targetTopRight = Random.ColorHSV(0, 1, 0.9f, 1, 0.9f, 1);
            targetBottomLeft = Random.ColorHSV(0, 1, 0.9f, 1, 0.9f, 1);
            targetBottomRight = Random.ColorHSV(0, 1, 0.9f, 1, 0.9f, 1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Handle rotation
        if (rotate)
        {
            rotationTime += Time.deltaTime * rotationSpeed;
            float rotationPingPong = Mathf.PingPong(rotationTime, 1); // Oscillates between 0 and 1
            float rotationValue = Mathf.Lerp(rotationRange.x, rotationRange.y, rotationEasingCurve.Evaluate(rotationPingPong));
            transform.localEulerAngles = new Vector3(0, 0, rotationValue);
        }

        // Handle scaling
        if (scale)
        {
            scaleTime += Time.deltaTime * scaleSpeed;
            float scalePingPong = Mathf.PingPong(scaleTime, 1); // Oscillates between 0 and 1
            float scaleValue = Mathf.Lerp(scaleRange.x, scaleRange.y, scaleEasingCurve.Evaluate(scalePingPong));
            transform.localScale = Vector3.one * scaleValue;
        }

        //Slowly change the gradient colors on the text over time, pick a random gradient corner each update and slowly change that
        if (textComponent != null && colorChange)
        {
            colorChangeTimer += Time.deltaTime;

            // Check if it's time to pick new target colors
            if (colorChangeTimer >= colorChangeInterval)
            {
                targetTopLeft = Random.ColorHSV(0, 1, 0.9f, 1, 0.9f, 1);
                targetTopRight = Random.ColorHSV(0, 1, 0.9f, 1, 0.9f, 1);
                targetBottomLeft = Random.ColorHSV(0, 1, 0.9f, 1, 0.9f, 1);
                targetBottomRight = Random.ColorHSV(0, 1, 0.9f, 1, 0.9f, 1);
                colorChangeTimer = 0;
            }

            // Smoothly transition to the target colors
            textComponent.colorGradient = new VertexGradient(
                Color.Lerp(textComponent.colorGradient.topLeft, targetTopLeft, Time.deltaTime / colorChangeInterval),
                Color.Lerp(textComponent.colorGradient.topRight, targetTopRight, Time.deltaTime / colorChangeInterval),
                Color.Lerp(textComponent.colorGradient.bottomLeft, targetBottomLeft, Time.deltaTime / colorChangeInterval),
                Color.Lerp(textComponent.colorGradient.bottomRight, targetBottomRight, Time.deltaTime / colorChangeInterval)
            );
        }
    }
}
