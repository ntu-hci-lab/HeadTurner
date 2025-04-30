using System.Collections;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.UI;
public enum Condition { NormalBed, ActuatedBed }
public enum Apps { Ecosphere, FPS }
public class SummativeRecorder : MonoBehaviour
{
    [Header("Data Pipeline Settings")]
    public int participantID = 0;

    public Apps app = Apps.Ecosphere;
    public bool usingEMG = true;
    public bool isAngled = true;
    static EMGLogger_O emgLogger;
    static AngledEMGLogger_O angledEMGLogger;
    public Condition condition = Condition.NormalBed;
    private FileStream fs;
    private StreamWriter sw;
    public OrientationUtility headOT, bodyOT;
    bool isRecording = false;
    public string dirname = "Result S";
    string folder;
    void Start()
    {
        folder = Path.Combine(dirname, app.ToString());
        Directory.CreateDirectory(folder);
        string path = Path.Combine(folder, $"P_{participantID}_{condition}.csv");
        fs = new FileStream(path, FileMode.OpenOrCreate);
        sw = new StreamWriter(fs);
        sw.WriteLine("time, HeadPitch, HeadYaw, HeadRoll, BodyPitch, BodyYaw, BodyRoll");
        if (headOT == null || bodyOT == null)
        {
            Debug.LogError("Head or Body OrientationUtility is not set");
            Application.Quit();

        }
        if (condition == Condition.ActuatedBed)
        {
            FindObjectOfType<HeadController>().enableActuation = true;
        }
        else
        {
            FindObjectOfType<HeadController>().enableActuation = false;
        }
        if (usingEMG)
        {
            string emg_folder = Path.Combine(folder, "emg_data", $"P_{participantID}_{condition}");
            if (isAngled)
            {
                angledEMGLogger = new AngledEMGLogger_O(dirname: emg_folder);
            }
            else
            {
                emgLogger = new EMGLogger_O(dirname: emg_folder);
            }
        }
    }
    public void Update()
    {
        string timeStamp = Time.time.ToString();

        if (headOT.IsCalibrated == false)
        {
            return;
        }
        if ((Input.GetKeyDown(KeyCode.Space) || headOT.IsCalibrated == true) && !isRecording)
        {
            //Debug.Log(app.ToString() + "Recording started");
            isRecording = !isRecording;
            if (usingEMG)
            {
                // if isAngled, the AngledEMGLogger will start logging upon initialization
                if (!isAngled)
                {
                    emgLogger.start_logging(app.ToString(), condition.ToString());
                }
                //Debug.Log("EMG start logging");
            }
        }
        if (isRecording)
        {
            string line = $"{timeStamp}, {headOT.PitchAngle}, {headOT.YawAngle}, {headOT.RollAngle}, {bodyOT.PitchAngle}, {bodyOT.YawAngle}, {bodyOT.RollAngle}";
            sw.WriteLine(line);
        }
    }
    void OnApplicationQuit()
    {
        if (usingEMG)
        {
            if (isAngled)
            {
                angledEMGLogger.close();
            }
            else
            {
                emgLogger.end_logging();
                emgLogger.close();
            }
        }
        if (sw != null)
        {
            sw.Close();
        }
        if (fs != null)
        {
            fs.Close();
        }
    }
}