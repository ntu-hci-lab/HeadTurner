using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RotationTest : MonoBehaviour
{
    // The object that will be rotated, and we are going to monitor its rotation using the Roll, Yaw, Pitch expressions
    public OrientationUtility orientationUtility;
    [Header("Data Logging")]
    public string dirName = "Measurement";
    private StreamWriter dataWriter;
    public float logPeriod = 1.0f;
    private float logTimer = 0.0f;
    void Start()
    {
        // Set the object that will be rotated
        orientationUtility = FindObjectOfType<OrientationUtility>();
        if (orientationUtility == null)
        {
            Debug.LogError("OrientationUtility not found in the scene");
            Application.Quit();
        }
        Directory.CreateDirectory(dirName);
        string currentTime = System.DateTime.Now.ToString("HHmmss");
        string data_path = Path.Combine(dirName, $"data_{currentTime}.csv");
        dataWriter = new StreamWriter(data_path);
        string header = "TimeStamp, Roll, Yaw, Pitch,";
        dataWriter.WriteLine(header);
    }

    // Update is called once per frame
    void Update()
    {
        if (orientationUtility.IsCalibrated)
        {
            // Log the Roll, Yaw, Pitch angles to console and csv file every 1 second
            logTimer += Time.deltaTime;
            if (logTimer >= logPeriod)
            {
                logTimer = 0.0f;
                string timeStamp = System.DateTime.Now.ToString("HH:mm:ss");
                string data = $"{timeStamp}, {orientationUtility.RollAngle}, {orientationUtility.YawAngle}, {orientationUtility.PitchAngle},";
                dataWriter.WriteLine(data);
                Debug.Log("Roll: " + orientationUtility.RollAngle + " Yaw: " + orientationUtility.YawAngle + " Pitch: " + orientationUtility.PitchAngle);
            }
        }
    }
    private void OnApplicationQuit()
    {
        dataWriter.Close();
    }
}
