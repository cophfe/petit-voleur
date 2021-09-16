using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

//Used to guarantee playerprefs has a value before options uses it
public class SetDefaultOptions : MonoBehaviour
{
	public CameraController cameraControllerPrefab = null;
	public AudioMixer mixer;
	public string masterParameterName = "Volume Master";
	public string musicParameterName = "Volume Music";
	public OptionsUI oUI = null;

	private void Start()
	{
		//MASTER VOLUME
		if (!PlayerPrefs.HasKey("MasterVolume")) 
		{ 
			float volumeDb;
			mixer.GetFloat(masterParameterName, out volumeDb);
			float volumeLin = OptionsUI.DecibelsToLinear(volumeDb);
			PlayerPrefs.SetFloat("MasterVolume", volumeLin);
			PlayerPrefs.SetFloat("DefaultMasterVolume", volumeLin);
		}

		//MUSIC VOLUME
		if (!PlayerPrefs.HasKey("MusicVolume"))
		{
			float volumeDb;
			mixer.GetFloat(musicParameterName, out volumeDb);
			float volumeLin = OptionsUI.DecibelsToLinear(volumeDb);
			PlayerPrefs.SetFloat("MusicVolume", volumeLin);
			PlayerPrefs.SetFloat("DefaultMusicVolume", volumeLin);
		}

		//FULLSCREEN
		if (!PlayerPrefs.HasKey("isFullscreen"))
		{
			int val = Screen.fullScreen ? 1 : 0;
			PlayerPrefs.SetInt("isFullscreen", val);
			PlayerPrefs.SetInt("DefaultIsFullscreen", val);
		}

		//CAMERA SENSITIVITY
		if (!PlayerPrefs.HasKey("Sensitivity"))
		{
			float sensitivity = cameraControllerPrefab.sensitivity;
			PlayerPrefs.SetFloat("Sensitivity", sensitivity);
			PlayerPrefs.SetFloat("DefaultSensitivity", sensitivity);
		}

		PlayerPrefs.Save();
		if (oUI)
		{
			oUI.cameraController = FindObjectOfType<CameraController>();
			oUI.SetUIValuesToPlayerPrefs();
			oUI.SetGameValuesToUIValues();
		}
	}
}
