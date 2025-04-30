using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.IO.Ports;
using System.Threading;

public class EMGLogger_O
{
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

    public EMGLogger_O(string portName = "COM5", int baudRate = 115200, string dirname = "Result")
    {
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
        string header = "range,posture,start,end,";
        _timestampWriter.WriteLine(header);

        _thread = new Thread(ReadSerialPort);
        _thread.Start();
    }

    private void ReadSerialPort()
    {
        while (_running)
        {
            string data = _serialPort.ReadLine();
            _dataWriter.WriteLine(data);
            if (_endLogging)
            {
                string end_time = data.Split(',')[0];
                data = _cur_range + ","
                    + _cur_posture + ","
                    + _cur_start_time + ","
                    + end_time + ",";
                _timestampWriter.WriteLine(data);
                _endLogging = false;
            }
            else if (_startLogging)
            {
                string timestamp = data.Split(',')[0];
                _cur_start_time = timestamp;
                _startLogging = false;
            }

        }
    }

    public void start_logging(string range, string posture)
    {
        _startLogging = true;
        _cur_range = range;
        _cur_posture = posture;
    }

    public void end_logging()
    {
        _endLogging = true;
    }

    public void close()
    {
        _running = false;
        if (_endLogging)
        {
            string end_time = _serialPort.ReadLine().Split(',')[0];
            string data = "EOL" + ","
                + _cur_posture + ","
                + _cur_start_time + ","
                + end_time + ",";
            _timestampWriter.WriteLine(data);
        }
        _thread.Abort();
        _dataWriter.Close();
        _timestampWriter.Close();
        if (_serialPort.IsOpen)
            _serialPort.Close();
    }
}
