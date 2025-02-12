using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.IO.Ports;
using System.Threading;

public class AngledEMGLogger_O
{
    [Header("Orientation Sensing")]
    public OrientationUtility headOT;
    [Header("Serial Port")]
    private SerialPort _serialPort;
    private StreamWriter _dataWriter;
    private StreamWriter _timestampWriter;
    private Thread _thread;
    private bool _startLogging;
    private bool _endLogging;
    public bool IsEndLogging
    {
        get
        {
            return _endLogging;
        }
    }
    private string _cur_range;
    private string _cur_posture;

    private string _cur_start_time;
    private bool _running = true;

    public enum Status { Start, Waiting, Back, End }

    public AngledEMGLogger_O(string portName = "COM5", int baudRate = 115200, string dirname = "Result")
    {
        headOT = GameObject.FindObjectOfType<HeadController>().headOT;
        if (headOT == null)
        {
            Debug.LogError("Head OrientationUtility is not set");
            Application.Quit();
        }
        _serialPort = new SerialPort(portName, baudRate);
        try
        {
            _serialPort.Open();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed opening port. Log: \n{e}");
        }

        Directory.CreateDirectory(dirname);
        string currentTime = System.DateTime.Now.ToString("HHmmss");
        string data_path = Path.Combine(dirname, $"data_{currentTime}");
        string timestamp_path = Path.Combine(dirname, $"timestamp_{currentTime}.csv");

        _dataWriter = new StreamWriter(data_path);
        _timestampWriter = new StreamWriter(timestamp_path);
        string header = "time,turnedAngle,pitchAngle,yawAngle";
        _timestampWriter.WriteLine(header);

        _thread = new Thread(ReadSerialPort);
        _thread.Start();
    }

    private float getCurrentAngle()
    {
        // TODO: Implement this function
        return headOT.TurnedAngle;
    }

    private void ReadSerialPort()
    {
        while (_running)
        {
            string data = _serialPort.ReadLine();
            _dataWriter.WriteLine(data);
            string cur_time = "0";
            try
            {
                cur_time = data.Split(',')[0];
            }
            catch (Exception e)
            {
                Debug.LogError($"EMG Timestamp cannot be parsed Log: \n{e}");
                cur_time = "Error";
            }
            float angle = getCurrentAngle();
            data = cur_time + "," + headOT.TurnedAngle + "," + headOT.PitchAngle + "," + headOT.YawAngle;
            _timestampWriter.WriteLine(data);
        }
    }

    public void close()
    {
        _running = false;
        _thread.Abort();
        _dataWriter.Close();
        _timestampWriter.Close();
        if (_serialPort.IsOpen)
            _serialPort.Close();
    }
}