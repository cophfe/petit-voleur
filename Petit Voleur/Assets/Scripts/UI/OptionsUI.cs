/*==================================================
	Programmer: Connor Fettes
==================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour
{
	[Tooltip("")]
	public Slider volumeSlider = null;
	[Tooltip("")]
	public Toggle fullscreenToggle = null;

	private void Awake()
	{
		//set values to default value	
	}

	public void OnVolumeChanged()
	{
		if (!volumeSlider)
			return;

		Debug.Log("Volume Slider Value: " + volumeSlider.value);
	}

	public void OnFullscreenValueChanged()
	{
		if (!fullscreenToggle)
			return;

		Debug.Log("Fullscreen Value: " + fullscreenToggle.isOn);
	}
}
