using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System;
using System.Threading;
using Unity.VisualScripting;
/* Caution: this script is mixed with sensing and control logics, codes are coupled */
public class DistanceSensor : MonoBehaviour
{
    [Header("Sensor")]
    public int averagingFrames = 100;
    const float stroke = 330f; // 1250 for slower motor
    int currentFrame = 0;

    bool isCalibrated = false;
    public bool IsCalibrated
    {
        get
        {
            return isCalibrated;
        }
    }
    [Header("OptiTrack Sensor")]
    public Transform head;
    public Transform pillow;
    float dist, distAvg = 0;
    public float DistAvg
    {
        get
        {
            if (isCalibrated)
            {
                return distAvg;
            }
            else
            {
                Debug.LogError("DistanceSensor is not calibrated, return 0 anyway");
                return 0;
            }
        }
    }
    public float Dist
    {
        get
        {
            return dist;
        }
    }

    // Methods
    // Start is called before the first frame update
    void Start()
    {
        if (head == null || pillow == null)
        {
            Debug.LogError("Head or Pillow is not set");
            // Quit
            Application.Quit();
        }
        currentFrame = 0;
    }
    void Update()
    {
        dist = Vector3.Distance(head.position, pillow.position);
        //Debug.Log("dist:" + dist);
        if (currentFrame < averagingFrames)
        {
            distAvg += dist;
            currentFrame++;
        }
        else if (currentFrame == averagingFrames)
        {
            distAvg /= averagingFrames;
            Debug.Log("distAvg:" + distAvg);
            currentFrame++;
            isCalibrated = true;
        }
    }
}
