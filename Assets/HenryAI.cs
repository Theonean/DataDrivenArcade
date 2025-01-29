using System.Collections;
using UnityEngine;

public class HenryAI : MonoBehaviour
{
    [SerializeField] private PlayerManager p2; // Reference to Henry AI's PlayerManager it controls
    [SerializeField] private PlayerManager p1; // Reference to Player 1's PlayerManager, the player enemy of Henry AI
    private bool isRunning = false;

    public float baseInputInterval = 1f; // Base interval between sending lines
    public float intervalRandomDeviationPercent = 20f; // Percentage deviation for randomness in interval

    private void OnEnable()
    {
        if (GameManager.instance.singlePlayer) StartAI();
    }

    private void OnDisable()
    {
        if (GameManager.instance.singlePlayer) StopAI();
    }

    public void StartAI()
    {
        if (!isRunning)
        {
            isRunning = true;
            StartCoroutine(SendLineInput());
        }
    }

    public void StopAI()
    {
        isRunning = false;
        StopCoroutine(SendLineInput());
    }

    private IEnumerator SendLineInput()
    {
        yield return new WaitForSeconds(1f); // Wait for 1 second before starting so everything is initialized

        while (isRunning)
        {
            // Always send the correct line based on Player 1's shape code
            int correctLineToSend = p2.selectedFactory.shapeBuilder.GetCurrentLineCode();
            float accuracy = p1.LineAccuracy - 0.1f; // HenryAI is slightly less accurate than the player
            float random = Random.Range(0f, 1f);

            //Give HenryAI a chance to still get it right even if it "should" miss, might make it more exciting?
            int lineToSend = random < accuracy ? correctLineToSend : Random.Range(0, 6);

            // Simulate the line input
            switch (lineToSend)
            {
                case 0: p2.OnCreateLine1(); break;
                case 1: p2.OnCreateLine2(); break;
                case 2: p2.OnCreateLine3(); break;
                case 3: p2.OnCreateLine4(); break;
                case 4: p2.OnCreateLine5(); break;
                case 5: p2.OnCreateLine6(); break;
                default:
                    Debug.LogError("Invalid line code: " + lineToSend); 
                    break;
            }


            // Calculate the interval with randomness
            float randomDeviation = baseInputInterval * (intervalRandomDeviationPercent / 100f);
            float interval = Random.Range(p1.InputSpeed - randomDeviation, p1.InputSpeed + randomDeviation);
            interval = Mathf.Max(0.1f, interval); // Ensure interval is at least 0.1f

            yield return new WaitForSeconds(interval);
        }
    }
}