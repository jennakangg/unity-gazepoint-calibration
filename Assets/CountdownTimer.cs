using UnityEngine;
using TMPro;  

public class CountdownTimer : MonoBehaviour
{
    public TextMeshProUGUI countdownText;  // TextMesh Pro reference
    private float currentTime;   // Internal timer
    private bool isCountingDown; // Track if countdown is active
    public RectTransform countdownRectTransform;  // Reference to the RectTransform of the countdownText

    void Start()
    {
        // Initialize the countdown to be inactive initially
        isCountingDown = false;
        
        // Hide the countdown text at the start
        countdownText.gameObject.SetActive(false);

        // Get the RectTransform component if not set
        if (countdownRectTransform == null)
        {
            countdownRectTransform = countdownText.GetComponent<RectTransform>();
        }
    }

    void Update()
    {
        if (isCountingDown && currentTime > 0)
        {
            // Reduce the current time
            currentTime -= Time.deltaTime;

            // Clamp currentTime to zero to avoid negative numbers
            if (currentTime < 0)
            {
                currentTime = 0;
                isCountingDown = false; // Stop countdown when it reaches 0

                // Hide the countdown text when the countdown ends
                countdownText.gameObject.SetActive(false);
            }

            // Update the countdown text
            UpdateCountdownText();
        }
    }

    // Public method to start the countdown with a specified duration
    public void StartCountdown(float countdownLength)
    {
        currentTime = countdownLength;  // Set the countdown duration
        isCountingDown = true;          // Activate the countdown
        
        // Make the countdown text visible when the countdown starts
        countdownText.gameObject.SetActive(true);
        
        UpdateCountdownText();          // Immediately update the text to show the starting time
    }

    // Helper method to update the UI text
    private void UpdateCountdownText()
    {
        // Update the UI text component to display the remaining time (formatted to 1 decimal place)
        countdownText.text = currentTime.ToString("0.0");
    }

    // Function to move the countdown to different positions on the screen
    public void PlaceCountdownAtPosition(Vector2 position)
    {
        countdownRectTransform.anchorMin = position;
        countdownRectTransform.anchorMax = position;
        countdownRectTransform.anchoredPosition = Vector2.zero;  // Align to left-middle
    }
}
