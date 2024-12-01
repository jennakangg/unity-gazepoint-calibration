using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using UnityEngine.UI;
using TMPro;  

public class CalibrationDriver : MonoBehaviour
{
    public GameObject cameraObject;
    public RectTransform canvasRectTransform; 
    public GameObject blackScreenPanel; 
    public GameObject crosshair; 
    public CountdownTimer countdownTimer; 
    public TextMeshProUGUI directionsText;  // TextMesh Pro reference
    public Button startButton; 
    public bool isTrialRunning = false;
    public int trialNum = 0;
    public float sceneDuration = 5.0f; 
    private float elapsedTime = 0f; 
    public bool waitingForStartInput = true;

    public class TrialStructure
    {
        public int trialID;
        public float duration;
        public Vector2 initialCrosshairPlacement;

        public TrialStructure(int trialID, float duration, Vector2 initialCrosshairPlacement)
        {
            this.trialID = trialID;
            this.duration = duration;
            this.initialCrosshairPlacement = initialCrosshairPlacement;
        }
    }

    private List<TrialStructure> testCases;
    public GameObject gazePathRecorderObject; 
    private GazeDataRecorder gazePathRecorder;
    private int startIndex = 0;          
    private string csvFilePath = "Assets/TrialsToLoad/calibration.csv";

    void Start()
    {
        startButton.onClick.AddListener(StartTrials);
        gazePathRecorder = gazePathRecorderObject.GetComponent<GazeDataRecorder>();
        testCases = LoadTrialsFromCSV(csvFilePath, startIndex);

    }

    private List<TrialStructure> LoadTrialsFromCSV(string filePath, int startIndex)
    {
        List<TrialStructure> trials = new List<TrialStructure>();
        int currentIndex = 0;
        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            bool isFirstLine = true;

            while ((line = reader.ReadLine()) != null)
            {
                if (isFirstLine) 
                {
                    isFirstLine = false; // Skip header
                    continue;
                }

                if (currentIndex++ < startIndex) 
                    continue;

                string[] values = line.Split(',');

                int trialID = int.Parse(values[0]);
                float duration = float.Parse(values[1], CultureInfo.InvariantCulture);
                Vector2 initialCrosshairPlacement = ParseVector2(values[2]); 

                TrialStructure trial = new TrialStructure(
                    trialID, duration, initialCrosshairPlacement);

                trials.Add(trial);
            }
        }
        return trials;
    }

    // Helper method to parse a "(x y)" formatted string into a Vector2
    private Vector2 ParseVector2(string vectorString)
    {
        vectorString = vectorString.Trim('(', ')'); // Remove parentheses
        string[] coordinates = vectorString.Split(' '); // Split by space
        float x = float.Parse(coordinates[0], CultureInfo.InvariantCulture);
        float y = float.Parse(coordinates[1], CultureInfo.InvariantCulture);
        return new Vector2(x, y);
    }

    public void StartTrials()
    {
        startButton.gameObject.SetActive(false);
        StartCoroutine(RunTestCases());
    }

    IEnumerator RunTestCases()
    {        
        for (int i = 0; i < testCases.Count; i++)
        {            
            yield return null;

            Debug.Log($"Starting trial {startIndex + i}");

            trialNum = startIndex + i;

            gazePathRecorder.ResetGazePath();
            waitingForStartInput = true;
            directionsText.gameObject.SetActive(true);
            Debug.Log("Waiting for space bar input to start trial...");
            PlaceCrosshairAtPosition(testCases[i].initialCrosshairPlacement);
            Vector2 countdownPlacement = new (testCases[i].initialCrosshairPlacement.x, testCases[i].initialCrosshairPlacement.y - 0.05f);
            countdownTimer.PlaceCountdownAtPosition(countdownPlacement);

            yield return new WaitUntil(() => !waitingForStartInput);
            isTrialRunning = true;
            directionsText.gameObject.SetActive(false);

            waitingForStartInput = false;
            
            
            sceneDuration = testCases[i].duration;
            isTrialRunning = true;

            elapsedTime = 0f;
            // countdownTimer.StartCountdown(sceneDuration);
            
            yield return StartCoroutine(WaitForSceneDuration());
            gazePathRecorder.SaveGazeDataToSingleCSV(trialNum); 
            gazePathRecorder.ResetGazePath();

            Debug.Log($"Finished calibration number {trialNum}");
            waitingForStartInput = true;
            isTrialRunning = false;
        }

        startButton.gameObject.SetActive(true);
        isTrialRunning = false;
    }

    IEnumerator WaitForSceneDuration()
    {
        yield return new WaitForSeconds(sceneDuration);
    }

    public void PlaceCrosshairAtPosition(Vector2 position)
    {
        RectTransform crosshairRect = crosshair.GetComponent<RectTransform>();

        crosshairRect.anchorMin = position;
        crosshairRect.anchorMax = position;
        crosshairRect.anchoredPosition = Vector2.zero;  // Align crosshair to the bottom
    }

    void Update()
    {
        if (isTrialRunning)
        {
            if (elapsedTime < sceneDuration)
            {
                elapsedTime += Time.deltaTime;
                Debug.Log("Calibration started, tracking gaze");
                gazePathRecorder.TrackGaze(trialNum);
            }
        }

        // Start trial when space is pressed
        if (waitingForStartInput && Input.GetKeyDown(KeyCode.Space) && !isTrialRunning)
        {
            waitingForStartInput = false;
            isTrialRunning = true;
            elapsedTime = 0f;
        }
    }
}