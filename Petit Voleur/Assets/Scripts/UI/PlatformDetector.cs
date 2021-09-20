/*==================================================
	Programmer: Connor Fettes
==================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DefaultExecutionOrder(-1)]
public class PlatformDetector : MonoBehaviour
{
	/// <summary>
	/// What platform the game should consider itself to be on. NULL automatically detects platform.
	/// </summary>
	public enum PlatformOverride
	{
		NULL,
		NONE,
		WINDOWS,
		ANDROID,
		WEB,
		ALL
	}

	[Tooltip("Overrides the detected platform with this value")] 
	public PlatformOverride platformOverride = PlatformOverride.NULL;

	[Tooltip("A list of gameobjects enabled only on android")] 
	public GameObject[] androidExclusive = new GameObject[0];
	[Tooltip("A list of gameobjects enabled only on windows")] 
	public GameObject[] windowsExclusive = new GameObject[0];
	[Tooltip("A list of gameobjects enabled only on windows")] 
	public GameObject[] webExclusive = new GameObject[0];

	[Tooltip("A unity event invoked on start if the game is running on android")] 
	public UnityEvent isAndroid = null;
	[Tooltip("A unity event invoked on start if the game is running on windows")] 
	public UnityEvent isWindows = null;
	[Tooltip("A unity event invoked on start if the game is running on webgl")] 
	public UnityEvent isWeb = null;

	void Update()
    {
		RuntimePlatform platform = Application.platform;
		
		EnableWindows(platformOverride != PlatformOverride.NONE
			&& (platformOverride == PlatformOverride.ALL || platformOverride == PlatformOverride.WINDOWS || (platformOverride == PlatformOverride.NULL && (platform == RuntimePlatform.WindowsPlayer || platform == RuntimePlatform.WindowsEditor))));
		EnableAndroid(platformOverride != PlatformOverride.NONE
			&& (platformOverride == PlatformOverride.ALL || platformOverride == PlatformOverride.ANDROID || (platformOverride == PlatformOverride.NULL && (platform == RuntimePlatform.Android))));
		EnableWeb(platformOverride != PlatformOverride.NONE
			&& (platformOverride == PlatformOverride.ALL || platformOverride == PlatformOverride.WEB || (platformOverride == PlatformOverride.NULL && (platform == RuntimePlatform.WebGLPlayer))));
	}

    /// <summary>
	/// Invokes windows event and enables windows exclusives
	/// </summary>
	/// <param name="enable">whether it is windows or not</param>
    void EnableWindows(bool enable)
    {
		if (enable && isWindows != null)
		{
			isWindows.Invoke();
		}

		for (int i = 0; i < windowsExclusive.Length; i++)
		{
			windowsExclusive[i].SetActive(enable);
		}
    }

	/// <summary>
	/// Invokes android event and enables android exclusives
	/// </summary>
	/// <param name="enable">whether it is android or not</param>
	void EnableAndroid(bool enable)
	{
		if (enable && isAndroid != null)
		{
			isAndroid.Invoke();
		}

		for (int i = 0; i < androidExclusive.Length; i++)
		{
			androidExclusive[i].SetActive(enable);
		}
	}

	/// <summary>
	/// Invokes web event and enables web exclusives
	/// </summary>
	/// <param name="enable">whether it is web or not</param>
	void EnableWeb(bool enable)
	{
		if (enable && isWeb != null)
		{
			isWeb.Invoke();
		}

		for (int i = 0; i < webExclusive.Length; i++)
		{
			webExclusive[i].SetActive(enable);
		}
	}
}
