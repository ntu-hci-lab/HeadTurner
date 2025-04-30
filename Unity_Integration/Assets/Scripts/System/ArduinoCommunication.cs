using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System;
using UnityEngine;
using System.Threading;

public class ArduinoCommunication : MonoBehaviour
{
    public enum Mode { Speed, Position }
    public Mode mode = Mode.Position;
    public string portName = "COM8";
    public int baudRate = 115200;
    const int motorRange = 660;

    private SerialPort port;
    int targetLinearMotor = 0;
    public int TargetLinearMotor
    {
        get
        {
            return targetLinearMotor;
        }
        set
        {
            targetLinearMotor = value;
        }
    }
    int targetMotorSpeed = 0;
    public int TargetMotorSpeed
    {
        get
        {
            return targetMotorSpeed;
        }
        set
        {
            targetMotorSpeed = value;
        }
    }
    int currentMotor = 0;
    float threshold = 15f;
    public float motorSpeed = 0.15f;
    Thread arduinoCommander;
    Thread arduinoReader;
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
    private void Start()
    {
        /* StartCoroutine(ActuatorCommand());
        StartCoroutine(EncoderRead()); */
        arduinoCommander = new Thread(ArduinoCommand);
        arduinoReader = new Thread(ArduinoRead);
        arduinoCommander.Start();
        arduinoReader.Start();
    }
    protected void Command(char mode, int pos)
    {
        if (!port.IsOpen) return;
        port.WriteLine(mode + pos.ToString());
        //Debug.Log(mode + pos.ToString());
    }
    IEnumerator ActuatorCommand()
    {
        while (true)
        {
            targetLinearMotor = Mathf.Clamp(targetLinearMotor, -motorRange / 2, motorRange / 2);
            currentMotor = (int)Mathf.Lerp(currentMotor, targetLinearMotor, motorSpeed);
            try
            {
                Command('p', currentMotor);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed sending command. Log: \n{e}");
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
    IEnumerator EncoderRead()
    {
        while (true)
        {
            try
            {
                string message = port.ReadLine();
                //Debug.Log(message);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed reading message. Log: \n{e}");
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    // Update is called once per frame
    void ArduinoCommand()
    {
        while (true)
        {
            switch (mode)
            {
                case Mode.Speed:
                    try
                    {
                        Command('s', targetMotorSpeed);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed sending command. Log: \n{e}");
                    }
                    break;
                case Mode.Position:

                    if (Math.Abs(currentMotor - targetLinearMotor) > threshold)
                    {
                        targetLinearMotor = Mathf.Clamp(targetLinearMotor, -motorRange / 2, motorRange / 2);
                        currentMotor = (int)Mathf.Lerp(currentMotor, targetLinearMotor, motorSpeed);
                        try
                        {
                            Command('p', currentMotor);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Failed sending command. Log: \n{e}");
                        }
                    }
                    else
                    {
                        Command('s', 0);
                    }
                    break;
            }
            Thread.Sleep(100); // 100 ms
        }
    }
    void ArduinoRead()
    {
        while (true)
        {
            try
            {
                string message = port.ReadLine();
                //Debug.Log(message);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed reading message. Log: \n{e}");
            }
        }
    }
    void OnDestroy()
    {
        arduinoCommander.Abort();
        arduinoReader.Abort();
        Command('p', 0);
        if (port.IsOpen)
        {
            port.Close();
        }
    }
}
