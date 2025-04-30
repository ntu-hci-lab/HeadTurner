using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
// This script is used to convert the head roll, yaw, pitch angles to the quaternion representation, and calculate the relative angles between it and the identity quaternion
public class DataConverter : MonoBehaviour
{
    // data is stored in csv files, headers are [time, HeadPitch,HeadYaw,HeadRoll,BodyPitch,BodyYaw,BodyRoll]
    // file name is like "P_1_ActuatedBed.csv", "P_2_NormalBed.csv", every P_i is a different participant, i is the participant number, i = 1~16
    [Header("Data Logging")]
    public string dirName = "Result S";
    private StreamReader dataReader;
    private StreamWriter dataWriter;
    // Start is called before the first frame update
    void Start()
    {
        Directory.CreateDirectory(dirName + "_converted");
        // convert the data of each participant
        for (int i = 1; i <= 16; i++)
        {
            /* if (i == 2)
            {
                continue;
            } */
            Convert($"P_{i}_ActuatedBed");
            Convert($"P_{i}_NormalBed");
        }
        // quit the application after converting the data
        Application.Quit();
    }
    void Convert(string fileName)
    {
        // read the data from the csv file
        string data_path = Path.Combine(dirName, fileName + ".csv");
        dataReader = new StreamReader(data_path);
        // write the data to the csv file
        string header = "time,TurnedAngle,HeadPitch,HeadYaw,HeadRoll";
        data_path = Path.Combine(dirName + "_converted", fileName + "_converted.csv");
        dataWriter = new StreamWriter(data_path);
        dataWriter.WriteLine(header);
        // read the time, HeadPitch,HeadYaw,HeadRoll
        string line = dataReader.ReadLine();// skip the header
        // try reading lines, if a indexOutOfRangeException is thrown, skip that line
        while ((line = dataReader.ReadLine()) != null)
        {
            string[] data = line.Split(',');
            try
            {
                float HeadPitch = float.Parse(data[1]);
                float HeadYaw = float.Parse(data[2]);
                float HeadRoll = float.Parse(data[3]);
                // convert the head roll, yaw, pitch angles to the quaternion representation
                Quaternion headQuaternion = Quaternion.Euler(HeadPitch, HeadYaw, HeadRoll);
                Vector3 relativeVector = headQuaternion * Vector3.forward;
                float turnedAngle = Vector3.Angle(Vector3.forward, relativeVector);
                // write the time, turned angle, HeadPitch,HeadYaw,HeadRoll to the csv file
                string convertedData = $"{data[0]},{turnedAngle},{data[1]},{data[2]},{data[3]}";
                dataWriter.WriteLine(convertedData);
            }
            catch (System.Exception)
            {
                continue;
            }
        }
        dataReader.Close();
        dataWriter.Close();
    }
    private void OnApplicationQuit()
    {
        if (dataReader != null)
        {
            dataReader.Close();
        }
        if (dataWriter != null)
        {
            dataWriter.Close();
        }
    }
}
