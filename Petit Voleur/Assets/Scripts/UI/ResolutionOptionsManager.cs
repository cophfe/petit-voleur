/*==================================================
	Programmer: Connor Fettes
==================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DefaultExecutionOrder(-1)]
public class ResolutionOptionsManager : MonoBehaviour
{
	public TMP_Dropdown resolutionOptions;
	Resolution[] resolutions;
	List<Vector2Int> validResolutions;
	int initialIndex = 0;
	int currentIndex = 0;

	void Start()
    {
#if !UNITY_ANDROID
		resolutions = Screen.resolutions;
		validResolutions = new List<Vector2Int>();
		Resolution currentRes = Screen.currentResolution;

		//GET VALID RESOLUTIONS
		validResolutions.Add(new Vector2Int(resolutions[0].width, resolutions[0].height));

		for (int i = 1; i < resolutions.Length; i++)
		{
			if (resolutions[i].width == resolutions[i - 1].width && resolutions[i].height == resolutions[i - 1].height)
				continue;

			validResolutions.Add(new Vector2Int(resolutions[i].width, resolutions[i].height));

			if (currentRes.width == resolutions[i].width && currentRes.height == resolutions[i].height)
			{
				initialIndex = validResolutions.Count - 1;
			}
		}

		//PARSE AS STRINGS
		List<string> resolutionStrings = new List<string>(validResolutions.Count);

		for (int i = validResolutions.Count - 1; i >= 0 ; i--)
		{
			resolutionStrings.Add($"{validResolutions[i].x}x{validResolutions[i].y}");
		}

		//ADD AS OPTIONS
		resolutionOptions.AddOptions(resolutionStrings);
		resolutionOptions.value = resolutionStrings.Count - 1 - initialIndex;

		currentIndex = resolutionStrings.Count - 1 - initialIndex;

		PlayerPrefs.SetInt("Resolution", currentIndex);
#endif
	}

	/// <summary>
	/// Sets the screen resolution to a value, by index
	/// </summary>
	/// <param name="index">An index of the resolution dropdown</param>
	/// <param name="fullscreen">whether or not the game is fullscreen</param>
	public void SetResolutionFromIndex(int index, bool fullscreen) 
	{
#if !UNITY_ANDROID
		Vector2Int r = validResolutions[validResolutions.Count - 1 - index];
		currentIndex = index;
		Screen.SetResolution(r.x, r.y, fullscreen);
#endif
	}

	/// <summary>
	/// gets current index
	/// </summary>
	/// <returns>returns current index</returns>
	public int GetCurrentIndex() { return currentIndex; }
}
