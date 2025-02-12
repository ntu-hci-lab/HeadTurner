using System;
using UnityEngine;
using UnityEngine.UI;
public class DOFController : MonoBehaviour
{
	public Slider leftMotor, rightMotor;
	public DOFCommunication dofCommunication;
	private void Start()
	{
		//Find the DOFCommunication script in the scene
		dofCommunication = FindObjectOfType<DOFCommunication>();
		leftMotor.onValueChanged.AddListener(delegate { OnSliderChange(); });
		rightMotor.onValueChanged.AddListener(delegate { OnSliderChange(); });
	}
	public void OnSliderChange()
	{
		Debug.Log("Left/Right Slider changed");
		dofCommunication.SetMotorPos(leftMotor.normalizedValue, rightMotor.normalizedValue);
	}
}
