/*==================================================
	Programmer: Connor Fettes
==================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

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
	[Tooltip("The toggle that changes inverted value.")]
	public Toggle invertToggle = null;
	[Tooltip("The dropdown that changes resolution.")]
	public TMP_Dropdown resolutionDropdown = null;
	[Tooltip("The dropdown that changes quality.")]
	public TMP_Dropdown qualityDropdown = null;
	[Tooltip("The apply button.")]
	public Button applyButton = null;
	[Tooltip("The Resolution Manager.")]
	public ResolutionOptionsManager rOM = null;

	[Tooltip("The audio mixer, obviously.")]
	public AudioMixer mixer;
	[Tooltip("The exposed paramater on the mixer that controls master volume.")]
	public string masterParameterName = "Volume Master";
	[Tooltip("The exposed paramater on the mixer that controls music volume.")]
	public string musicParameterName = "Volume Music";
	[Tooltip("The camera controller.")]
	public CameraController cameraController = null;

	bool isChanged = false;
	/// <summary>
	/// If values have been changed since opening the options menu
	/// </summary>
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

	public void OnResolutionChange()
	{
		IsChanged = true;
	}

	public void OnValueChange()
	{
		IsChanged = true;
	}

	public void OnApply()
	{
		IsChanged = false;
		PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
		PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
		PlayerPrefs.SetFloat("Sensitivity", sensitivitySlider.value);
		PlayerPrefs.SetInt("IsFullscreen", fullscreenToggle.isOn ? 1 : 2);
		PlayerPrefs.SetInt("IsInverted", invertToggle.isOn ? 1 : 2);
		PlayerPrefs.SetInt("Quality", qualityDropdown.value);
		PlayerPrefs.SetInt("Resolution", resolutionDropdown.value);

		PlayerPrefs.Save();

		SetGameValuesToUIValues();
	}

	public void SetUIValuesToPlayerPrefs()
	{
		sensitivitySlider.value = PlayerPrefs.GetFloat("Sensitivity");
		fullscreenToggle.isOn = PlayerPrefs.GetInt("IsFullscreen") == 1;
		invertToggle.isOn = PlayerPrefs.GetInt("IsInverted") == 1;
		masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume");
		musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume");

		if (PlayerPrefs.HasKey("Resolution"))
		{
			Debug.Log(PlayerPrefs.GetInt("Resolution"));
			resolutionDropdown.value = PlayerPrefs.GetInt("Resolution");
		}

		qualityDropdown.value = PlayerPrefs.GetInt("Quality");

		IsChanged = false;
	}

	public void SetGameValuesToUIValues()
	{
		mixer.SetFloat(masterParameterName, LinearToDecibels(masterVolumeSlider.value));
		mixer.SetFloat(musicParameterName, LinearToDecibels(musicVolumeSlider.value));

		if (cameraController)
		{
			cameraController.sensitivity = sensitivitySlider.value;
			cameraController.inverted = invertToggle.isOn;
		}

		if (qualityDropdown.value != QualitySettings.GetQualityLevel())
			QualitySettings.SetQualityLevel(qualityDropdown.value, true);

		if (PlayerPrefs.HasKey("Resolution") && rOM.GetCurrentIndex() != resolutionDropdown.value)
			rOM.SetResolutionFromIndex(resolutionDropdown.value, fullscreenToggle.isOn);

		Screen.fullScreenMode = fullscreenToggle.isOn ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
	}

	public void OnRestoreDefaults()
	{
		masterVolumeSlider.value = PlayerPrefs.GetFloat("DefaultMasterVolume");
		musicVolumeSlider.value = PlayerPrefs.GetFloat("DefaultMusicVolume");
		sensitivitySlider.value = PlayerPrefs.GetFloat("DefaultSensitivity");
		invertToggle.isOn = PlayerPrefs.GetInt("DefaultIsInverted") == 1;
		qualityDropdown.value = PlayerPrefs.GetInt("DefaultQuality");
		//do not reset screen
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
