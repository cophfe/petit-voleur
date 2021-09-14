/*==================================================
	Programmer: Connor Fettes
==================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class OptionsUI : MonoBehaviour
{
	[Tooltip("The slider representing the music volume value.")]
	public Slider musicVolumeSlider = null;
	[Tooltip("The slider representing the sfx volume value.")]
	public Slider masterVolumeSlider = null;
	[Tooltip("The toggle that changes fullscreen value.")]
	public Toggle fullscreenToggle = null;
	
	[Tooltip("The audio mixer, obviously.")]
	public AudioMixer mixer;
	public string masterParameterName = "Volume Master";
	public string musicParameterName = "Volume Music";

	private void Awake()
	{
		fullscreenToggle.isOn = Screen.fullScreen;

		float volume;
		
		mixer.GetFloat(musicParameterName, out volume);
		musicVolumeSlider.value = Mathf.Pow(10.0f, volume / 20.0f); ;

		mixer.GetFloat(masterParameterName, out volume);
		masterVolumeSlider.value = Mathf.Pow(10.0f, volume / 20.0f); ;

	}

	public void OnMasterVolumeChanged()
	{
		if (!masterVolumeSlider)
			return;

		mixer.SetFloat(masterParameterName, 20.0f * Mathf.Log10(masterVolumeSlider.value));
	}

	public void OnMusicVolumeChanged()
	{
		if (!musicVolumeSlider)
			return;

		mixer.SetFloat(musicParameterName, 20.0f * Mathf.Log10(musicVolumeSlider.value));
	}

	public void OnFullscreenValueChanged()
	{
		if (!fullscreenToggle)
			return;

		Screen.fullScreen = fullscreenToggle.isOn;
	}
}
