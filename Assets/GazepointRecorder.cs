using System.Collections.Generic;
using UnityEngine;
using System.IO;
using GazepointUnity;


public class GazeDataRecorder : MonoBehaviour
{
    [SerializeField] private GazepointClient _eyeTracker;

    public GameObject cameraObject;
    public RectTransform crosshair;
    private List<CalibrationData> gazeDataList = new List<CalibrationData>(); 
    private bool csvInitialized = false; 
    private struct CalibrationData 
    {
        public int calibrationNumber;
        public int frameNumber;
        public Vector2 crosshairPosition;
        public GazepointClient.GazeData gazePosition;

        public CalibrationData(int trialNum, int frameNum, Vector2 ballPosition, GazepointClient.GazeData gazePos)
        {
            calibrationNumber = trialNum;
            frameNumber = frameNum;
            crosshairPosition = ballPosition;
            gazePosition = gazePos;
        }
    }

    public void ResetGazePath()
    {
        gazeDataList.Clear(); 
    }

    public void TrackGaze(int calibrationNumber)
    {    
        int frameNumber = Time.frameCount;

        Vector2 crosshairPosition = crosshair.position;
        Debug.Log($"Ball position: {crosshairPosition}");

        foreach (var gazePos in _eyeTracker.GazeValues) {
            CalibrationData currCalibrationData = new CalibrationData(calibrationNumber, frameNumber, crosshairPosition, gazePos);
            gazeDataList.Add(currCalibrationData);
        }
    }

    public void SaveGazeDataToSingleCSV(int calibrationNumber)
    {
        // Define the output folder path for the CSV
        var csvOutputFolder = Path.Combine(Application.dataPath, "GazeData");
        if (!Directory.Exists(csvOutputFolder))
        {
            Directory.CreateDirectory(csvOutputFolder);
        }

        // Define the CSV file path
        var csvFilePath = Path.Combine(csvOutputFolder, "gaze_data_segements.csv");

        // Use StreamWriter to write or append data to the CSV file
        using (StreamWriter writer = new StreamWriter(csvFilePath, append: true))
        {
            // Write the CSV header if this is the first trial being recorded
            if (!csvInitialized)
            {
                writer.WriteLine("CalibrationNumber,FrameNumber,CrosshairPosition,"
                    + "Counter,CursorX,CursorY,CursorState,"
                    + "LeftEyeX,LeftEyeY,LeftEyeZ,LeftEyePupilDiameter,LeftEyePupilValid,"
                    + "RightEyeX,RightEyeY,RightEyeZ,RightEyePupilDiameter,RightEyePupilValid,"
                    + "FixedPogX,FixedPogY,FixedPogStart,FixedPogDuration,FixedPogId,FixedPogValid,"
                    + "LeftPogX,LeftPogY,LeftPogValid,"
                    + "RightPogX,RightPogY,RightPogValid,"
                    + "BestPogX,BestPogY,BestPogValid,"
                    + "LeftPupilX,LeftPupilY,LeftPupilDiameter,LeftPupilScale,LeftPupilValid,"
                    + "RightPupilX,RightPupilY,RightPupilDiameter,RightPupilScale,RightPupilValid,"
                    + "Time,TimeTick");
                csvInitialized = true;
            }

            Debug.Log($"Len of data to write {gazeDataList.Count}");

            // Write each frame's gaze data from the gazeDataList
            foreach (var data in gazeDataList)
            {
                var gazePosition = data.gazePosition; // Assuming gazePosition is of type GazeData
                string positionString = "(" + data.crosshairPosition.x + " " + data.crosshairPosition.y + ")";
                writer.WriteLine($"{data.calibrationNumber},{data.frameNumber},{positionString},"
                    + $"{gazePosition.Counter},{gazePosition.CursorX},{gazePosition.CursorY},{gazePosition.CursorState},"
                    + $"{gazePosition.LeftEyeX},{gazePosition.LeftEyeY},{gazePosition.LeftEyeZ},{gazePosition.LeftEyePupilDiameter},{gazePosition.LeftEyePupilValid},"
                    + $"{gazePosition.RightEyeX},{gazePosition.RightEyeY},{gazePosition.RightEyeZ},{gazePosition.RightEyePupilDiameter},{gazePosition.RightEyePupilValid},"
                    + $"{gazePosition.FixedPogX},{gazePosition.FixedPogY},{gazePosition.FixedPogStart},{gazePosition.FixedPogDuration},{gazePosition.FixedPogId},{gazePosition.FixedPogValid},"
                    + $"{gazePosition.LeftPogX},{gazePosition.LeftPogY},{gazePosition.LeftPogValid},"
                    + $"{gazePosition.RightPogX},{gazePosition.RightPogY},{gazePosition.RightPogValid},"
                    + $"{gazePosition.BestPogX},{gazePosition.BestPogY},{gazePosition.BestPogValid},"
                    + $"{gazePosition.LeftPupilX},{gazePosition.LeftPupilY},{gazePosition.LeftPupilDiameter},{gazePosition.LeftPupilScale},{gazePosition.LeftPupilValid},"
                    + $"{gazePosition.RightPupilX},{gazePosition.RightPupilY},{gazePosition.RightPupilDiameter},{gazePosition.RightPupilScale},{gazePosition.RightPupilValid},"
                    + $"{gazePosition.Time},{gazePosition.TimeTick}");
            }
        }

        // Log a message indicating that the data has been saved
        Debug.Log($"Gaze data for trial {calibrationNumber} saved to {csvFilePath}");
    }
}
