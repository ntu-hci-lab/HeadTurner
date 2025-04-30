using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System;
using System.Threading;
using Unity.VisualScripting;

public class PressureSensor : MonoBehaviour
{
    [Header("Sensor")]
    public string portName = "COM11";
    public int baudRate = 115200;
    public int averagingFrames = 100;
    private SerialPort port;
    Thread arduinoReader;
    int[] pressureChannels = { 0, 0, 0, 0 };
    // UDLR
    public OrientationUtility orientationUtility;
    const float stroke = 330f; // 1250 for slower motor
    int currentFrame = 0;
    int pressureValueUp, pressureValueDown;
    float pressureValueAverageUp, pressureValueAverageDown;
    float pitchAngle = 0, prevPitchAngle = 0;

    bool isCalibrated = false;
    public bool IsCalibrated
    {
        get
        {
            return isCalibrated;
        }
    }
    float alpha = 1f;
    public int[] PressureChannels
    {
        get
        {
            return pressureChannels;
        }
    }
    [Header("Controller")]
    public Status status = Status.Stay;
    public enum Status { Stay, PitchUp, PitchDown };
    public float upthreshold = 0.2f;
    public float downthreshold = 0.2f;
    public int dowmSpeed = -200;
    int step = 7;
    int angleThreshold = 5;
    [Header("Actuator")]
    public ArduinoCommunication arduinoCommunication;
    int targetLinearMotor;

    // Methods
    void Awake()
    {
        port = new SerialPort(portName, baudRate);
        try
        {
            port.Open();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed opening port. Log: \n{e}");
        }

        if (!port.IsOpen) return;
    }
    // Start is called before the first frame update
    void Start()
    {
        arduinoReader = new Thread(ArduinoRead);
        arduinoReader.Start();
        arduinoCommunication = FindObjectOfType<ArduinoCommunication>();

        if (orientationUtility == null)
        {
            orientationUtility = FindObjectOfType<OrientationUtility>();
        }
        if (arduinoCommunication == null)
        {
            Debug.LogError("Sensing or Communication is not set");
            // Quit
            Application.Quit();
        }
        currentFrame = 0;
    }
    void Update()
    {
        if (currentFrame < averagingFrames)
        {
            if (pressureChannels[0] < 10)
            {
                Debug.LogWarning("Pressure Too Low!");
                return;
            }

            pressureValueUp = pressureValueUp + pressureChannels[0];
            pressureValueDown += pressureChannels[1];
            currentFrame++;
        }
        else if (currentFrame == averagingFrames)
        {
            pressureValueAverageUp = (float)pressureValueUp / averagingFrames;
            pressureValueAverageDown = (float)pressureValueDown / averagingFrames;

            Debug.Log($"Pressure Sensor Average : {pressureValueAverageUp + pressureValueAverageDown}");
            currentFrame++;
            isCalibrated = true;
        }
        // Control
        else
        {
            pitchAngle = Mathf.Clamp(orientationUtility.PitchAngle, -45, 20);
            Debug.Log("Status: " + status + " Pitch: " + pitchAngle + " PressureSum " + (pressureChannels[0] + pressureChannels[1]));
            switch (status)
            {
                case Status.Stay:
                    if ((pressureChannels[0] + pressureChannels[1]) < (pressureValueAverageUp + pressureValueAverageDown) * (1 - downthreshold))
                    {
                        status = Status.PitchDown;
                    }
                    if (pitchAngle - prevPitchAngle > angleThreshold)
                    {
                        status = Status.PitchUp;
                    }
                    break;
                case Status.PitchUp:
                    targetLinearMotor = (int)(stroke * (1 - Mathf.Tan((pitchAngle + 45) * Mathf.Deg2Rad)));
                    arduinoCommunication.TargetLinearMotor = targetLinearMotor;
                    if (pressureChannels[0] + pressureChannels[1] >= (pressureValueAverageUp + pressureValueAverageDown) * (1 - downthreshold))
                    {
                        status = Status.Stay;
                    }
                    break;
                case Status.PitchDown:
                    arduinoCommunication.TargetLinearMotor += 7;
                    if (pressureChannels[0] + pressureChannels[1] >= (pressureValueAverageUp + pressureValueAverageDown) * (1 - downthreshold))
                    {
                        status = Status.Stay;
                    }
                    break;
                default:
                    break;
            }
        }
    }

    void ArduinoRead()
    {
        while (true)
        {
            try
            {
                string message = port.ReadLine();
                // string[] values = message.Split(new string[] {" UP:"," DOWN:"," LEFT:"," RIGHT:"}, StringSplitOptions.RemoveEmptyEntries);
                string[] values = message.Split(',');
                int sumValue = 0;
                for (int i = 0; i < values.Length; i++)
                {
                    pressureChannels[i] = (int)Mathf.Lerp(pressureChannels[i], int.Parse(values[i]), alpha);
                    sumValue += pressureChannels[i];
                    // Debug.Log($"Pressure Sensor up+down : {pressureChannels[0] + pressureChannels[1]}");
                }
                //Debug.Log($"Pressure Sensor sum : {sumValue}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed reading message. Log: \n{e}");
            }
        }
    }
    void OnDestroy()
    {
        arduinoReader.Abort();
        if (port.IsOpen)
        {
            port.Close();
        }
    }
}
