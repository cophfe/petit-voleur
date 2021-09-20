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
		BOTH
	}

	[Tooltip("Overrides the detected platform with this value")] 
	public PlatformOverride platformOverride = PlatformOverride.NULL;

	[Tooltip("A list of gameobjects enabled only on android")] 
	public GameObject[] androidExclusive = new GameObject[0];
	[Tooltip("A list of gameobjects enabled only on windows")] 
	public GameObject[] windowsExclusive = new GameObject[0];
	[Tooltip("A unity event invoked on awake if the game is running on android")] 
	public UnityEvent isAndroid = null;
	[Tooltip("A unity event invoked on awake if the game is running on windows")] 
	public UnityEvent isWindows = null;

	void Awake()
    {
		RuntimePlatform platform = Application.platform;
		
		EnableWindows(platformOverride != PlatformOverride.NONE
			&& (platformOverride == PlatformOverride.BOTH || platformOverride == PlatformOverride.WINDOWS || (platformOverride == PlatformOverride.NULL && (platform == RuntimePlatform.WindowsPlayer || platform == RuntimePlatform.WindowsEditor))));
		EnableAndroid(platformOverride != PlatformOverride.NONE
			&& (platformOverride == PlatformOverride.BOTH || platformOverride == PlatformOverride.ANDROID || (platformOverride == PlatformOverride.NULL && (platform == RuntimePlatform.Android))));
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
	/// Invokes windows event and enables windows exclusives
	/// </summary>
	/// <param name="enable">whether it is windows or not</param>
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
}
