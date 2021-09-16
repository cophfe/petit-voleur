/*==================================================
	Programmer: Connor Fettes
==================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

//there was a much better way to do this but too late now
public class OptionsUI : MonoBehaviour
{
	[Tooltip("The slider representing the music volume value.")]
	public Slider musicVolumeSlider = null;
	[Tooltip("The slider representing the sfx volume value.")]
	public Slider masterVolumeSlider = null;
	[Tooltip("The slider representing the camera sensitivity.")]
	public Slider sensitivitySlider = null;
	[Tooltip("The toggle that changes fullscreen value.")]
	public Toggle fullscreenToggle = null;
	public Button applyButton = null;

	[Tooltip("The audio mixer, obviously.")]
	public AudioMixer mixer;
	public string masterParameterName = "Volume Master";
	public string musicParameterName = "Volume Music";

	public CameraController cameraController = null;
	bool isChanged = false;
	bool IsChanged
	{
		get
		{
			return isChanged;
		}
		set
		{
			isChanged = value;
			applyButton.interactable = value;
		}
	}

	private void OnEnable()
	{
		SetUIValuesToPlayerPrefs();
	}

	public void OnMasterVolumeChanged()
	{
		IsChanged = true;
	}

	public void OnMusicVolumeChanged()
	{
		IsChanged = true;
	}

	public void OnSensitivityChanged()
	{
		IsChanged = true;
	}

	public void OnFullscreenValueChanged()
	{
		IsChanged = true;
	}

	public void OnApply()
	{
		IsChanged = false;
		PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
		PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
		PlayerPrefs.SetFloat("Sensitivity", sensitivitySlider.value);
		PlayerPrefs.SetInt("IsFullscreen", fullscreenToggle.isOn ? 1 : 0);

		PlayerPrefs.Save();

		SetGameValuesToUIValues();
	}

	public void SetUIValuesToPlayerPrefs()
	{
		sensitivitySlider.value = PlayerPrefs.GetFloat("Sensitivity");
		fullscreenToggle.isOn = PlayerPrefs.GetInt("isFullscreen") == 1;
		masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume");
		musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume");
		IsChanged = false;
	}

	public void SetGameValuesToUIValues()
	{
		mixer.SetFloat(masterParameterName, LinearToDecibels(masterVolumeSlider.value));
		mixer.SetFloat(musicParameterName, LinearToDecibels(musicVolumeSlider.value));
		Screen.fullScreen = fullscreenToggle.isOn;
		if (cameraController)
			cameraController.sensitivity = sensitivitySlider.value;
	}

	public void OnRestoreDefaults()
	{
		masterVolumeSlider.value = PlayerPrefs.GetFloat("DefaultMasterVolume");
		musicVolumeSlider.value = PlayerPrefs.GetFloat("DefaultMusicVolume");
		sensitivitySlider.value = PlayerPrefs.GetFloat("DefaultSensitivity");
		fullscreenToggle.isOn = PlayerPrefs.GetInt("DefaultIsFullscreen") == 1;
	}

	public static float LinearToDecibels(float linear)
	{
		return 20.0f * Mathf.Log10(linear);
	}

	public static float DecibelsToLinear(float db)
	{
		return Mathf.Pow(10.0f, db / 20.0f);
	}
}
