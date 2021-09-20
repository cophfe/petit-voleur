﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DefaultExecutionOrder(-1)]
public class PlatformDetector : MonoBehaviour
{
	public enum PlatformOverride
	{
		NULL,
		NONE,
		WINDOWS,
		ANDROID,
		BOTH
	}

	public PlatformOverride platformOverride = PlatformOverride.NULL;

	public GameObject[] androidExclusive = new GameObject[0];
	public GameObject[] windowsExclusive = new GameObject[0];
	public UnityEvent isAndroid = null;
	public UnityEvent isWindows = null;

	void Awake()
    {
		RuntimePlatform platform = Application.platform;
		
		EnableWindows(platformOverride != PlatformOverride.NONE
			&& (platformOverride == PlatformOverride.BOTH || platformOverride == PlatformOverride.WINDOWS || (platformOverride == PlatformOverride.NULL && (platform == RuntimePlatform.WindowsPlayer || platform == RuntimePlatform.WindowsEditor))));
		EnableAndroid(platformOverride != PlatformOverride.NONE
			&& (platformOverride == PlatformOverride.BOTH || platformOverride == PlatformOverride.ANDROID || (platformOverride == PlatformOverride.NULL && (platform == RuntimePlatform.Android))));
	}

    // Update is called once per frame
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