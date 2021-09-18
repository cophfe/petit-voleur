using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

//Used to guarantee playerprefs has a value before options uses it
[DefaultExecutionOrder(-1)]
public class SetDefaultOptions : MonoBehaviour
{
	static bool openedBefore = false;

	public CameraController cameraControllerPrefab = null;
	public AudioMixer mixer;
	public string masterParameterName = "Volume Master";
	public string musicParameterName = "Volume Music";
	public OptionsUI oUI = null;

	private void Start()
	{
		if (openedBefore)
			return;

#if UNITY_EDITOR
		PlayerPrefs.DeleteAll();
#endif

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
		if (!PlayerPrefs.HasKey("IsFullscreen"))
		{
			//int val = Screen.fullScreen ? 1 : 0;
			PlayerPrefs.SetInt("IsFullscreen", 1);
			PlayerPrefs.SetInt("DefaultIsFullscreen", 1);
		}
		
		//INVERT
		if (!PlayerPrefs.HasKey("IsInverted"))
		{
			int val = cameraControllerPrefab.inverted ? 1 : 2;
			PlayerPrefs.SetInt("IsInverted", val);
			PlayerPrefs.SetInt("DefaultIsInverted", val);
		}

		//QUALITY
		if (!PlayerPrefs.HasKey("Quality"))
		{
			int val = QualitySettings.GetQualityLevel();
			PlayerPrefs.SetInt("Quality", val);
			PlayerPrefs.SetInt("DefaultQuality", val);
		}

		//RESOLUTION
		PlayerPrefs.DeleteKey("Resolution");
		

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

		openedBefore = true;
	}
}
