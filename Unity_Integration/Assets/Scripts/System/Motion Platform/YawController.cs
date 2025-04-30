using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class YawController : MonoBehaviour
{
    public Slider yawAngle;
    public DOFCommunication dOFCommunication;
    private void Start()
    {
        //Find the DOFCommunication script in the scene
        dOFCommunication = FindObjectOfType<DOFCommunication>();
        yawAngle.onValueChanged.AddListener(delegate { OnSliderChange(); });
    }
    public void OnSliderChange()
    {
        Debug.Log("Yawing Slider changed");
        // Yawing between (0,0) and (motorRange, motorRange)
        var angle = yawAngle.normalizedValue;
        dOFCommunication.SetMotorPos(angle, angle);
    }
}
