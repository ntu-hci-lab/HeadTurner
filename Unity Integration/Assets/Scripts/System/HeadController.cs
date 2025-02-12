using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadController : MonoBehaviour
{
    [Header("Sensing")]
    // Head and Pillow orientation sensing    
    public OrientationUtility headOT;
    public DistanceSensor distanceSensor;
    // two control mode, default angle+distance, if there's no pillowOT, use the angle only
    [Header("Mode")]
    public Mode mode = Mode.AngleDistance;
    public enum Mode { AngleDistance, AngleOnly };

    [Header("Controller")]
    public float distThreshold = 0.02f;
    public float angleThreshold = 0.1f;
    public float timeDelay = 0.5f;
    float time = 0;
    public enum Status { Stay, PitchUp, PitchDown };
    public Status status = Status.Stay;

    [Header("Actuation")]
    public DOFCommunication dOFCommunication;
    public ArduinoCommunication arduinoCommunication;
    public int step = 11;
    public bool enableActuation = true;
    float yawHeadRelativeToTrunk, pitchAngle, prevPitchAngle = 0;
    float yawAngle, prevYawAngle = 0, returnFactor = 1;
    public bool canReturn = true;
    float targetPlatformMotor = 0.5f;
    int targetLinearMotor;
    const float stroke = 330f; // 1250 for slower moteor
    private void Start()
    {
        if (distanceSensor == null)
        {
            mode = Mode.AngleOnly;
            Debug.LogWarning("PillowTransform is not set, switch to AngleOnly mode");
        }
        if (headOT == null)
        {
            headOT = FindObjectOfType<OrientationUtility>();
        }
        //Find the Communication script in the scene
        dOFCommunication = FindObjectOfType<DOFCommunication>();
        arduinoCommunication = FindObjectOfType<ArduinoCommunication>();
        if (dOFCommunication == null || arduinoCommunication == null || headOT == null)
        {
            Debug.LogError("Sensing or Communication is not set");
            // Quit
            Application.Quit();
        }
    }
    private void Update()
    {
        if (!enableActuation)
        {
            return;
        }
        if (headOT.IsCalibrated)
        {
            // Body Yawing
            // Get the Yaw angle from the OrientationUtility script
            yawAngle = headOT.YawAngle;
            yawHeadRelativeToTrunk = yawAngle - (targetPlatformMotor - 0.5f) * 23;
            Debug.Log("|yawAngle: " + yawAngle.ToString());
            Debug.Log("|yawHeadRelativeToTrunk: " + yawHeadRelativeToTrunk.ToString());
            // The yawing range is (0,0) and (1,1), the neutral position is (0.5,0.5)
            // The error is normalized to the range (-0.5,0.5), then shifted to the range (0,1)
            // targetAngle is mapped from the error of (-90,90) to (0,1)
            // Debug.Log(); // -180 ~ +180
            targetPlatformMotor = yawHeadRelativeToTrunk / 140 + 0.5f;
            targetPlatformMotor = Mathf.Clamp(targetPlatformMotor, 0, 1);
            if (canReturn)
            {
                if (yawAngle >23 && Mathf.Abs(prevYawAngle - yawAngle) > 1f)
                {
                    // the head yaw is returning to the neutral position
                    returnFactor *= Mathf.Abs(yawAngle) / Mathf.Abs(prevYawAngle);
                    targetPlatformMotor *= returnFactor;
                }
                else
                {
                    returnFactor = 1;
                }
            }
            dOFCommunication.SetMotorPos(targetPlatformMotor, targetPlatformMotor);
            prevYawAngle = yawAngle;

            // Head Pitching
            pitchAngle = headOT.PitchAngle;
            if (mode == Mode.AngleOnly)
            {
                targetLinearMotor = (int)(stroke * (1 - Mathf.Tan((Mathf.Clamp(pitchAngle, -45, 20) + 45) * Mathf.Deg2Rad)));
                arduinoCommunication.TargetLinearMotor = targetLinearMotor;
            }
            else if (mode == Mode.AngleDistance && distanceSensor.IsCalibrated)
            {
                switch (status)
                {
                    case Status.Stay:
                        if (distanceSensor.Dist > distanceSensor.DistAvg + distThreshold)
                        {
                            status = Status.PitchDown;
                        }
                        if (pitchAngle - prevPitchAngle > angleThreshold)
                        {
                            status = Status.PitchUp;
                        }
                        break;
                    case Status.PitchUp:
                        targetLinearMotor = (int)(stroke * (1 - Mathf.Tan((Mathf.Clamp(pitchAngle, -45, 20) + 45) * Mathf.Deg2Rad)));
                        arduinoCommunication.TargetLinearMotor = targetLinearMotor;
                        // Delay for actuator to move
                        time += Time.deltaTime;
                        if (time > timeDelay)
                        {
                            if (distanceSensor.Dist <= distanceSensor.DistAvg + distThreshold)
                            {
                                status = Status.Stay;
                            }
                            time = 0;
                        }
                        break;
                    case Status.PitchDown:
                        targetLinearMotor += step;
                        arduinoCommunication.TargetLinearMotor = targetLinearMotor;
                        if (distanceSensor.Dist <= distanceSensor.DistAvg)
                        {
                            status = Status.Stay;
                        }
                        break;
                    default:
                        break;
                }
                prevPitchAngle = pitchAngle;
            }
            // Debug.Log("yawRel: " + yawHeadRelativeToTrunk.ToString());
            // Debug.Log("|pitchAngle: " + pitchAngle.ToString());
            // Debug.Log("|targetLinearMotor: " + targetLinearMotor.ToString());
        }
    }
}
