using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformEnabler : MonoBehaviour
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

	//private void OnValidate()
	//{
	//	RuntimePlatform platform = Application.platform;
	//	var win = (platformOverride != PlatformOverride.NONE
	//		&& (platformOverride == PlatformOverride.BOTH || platformOverride == PlatformOverride.WINDOWS || (platformOverride == PlatformOverride.NULL && (platform == RuntimePlatform.WindowsPlayer || platform == RuntimePlatform.WindowsEditor))));

	//	var and = (platformOverride != PlatformOverride.NONE
	//		&& (platformOverride == PlatformOverride.BOTH || platformOverride == PlatformOverride.ANDROID || (platformOverride == PlatformOverride.NULL && (platform == RuntimePlatform.Android))));
	
	//	Debug.Log("Platform: " + platform + ", Override: " + platformOverride + ", Windows: " + win + ", Android: " + and);

	//}

	void Start()
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
		for (int i = 0; i < windowsExclusive.Length; i++)
		{
			windowsExclusive[i].SetActive(enable);
		}
    }

	void EnableAndroid(bool enable)
	{
		for (int i = 0; i < androidExclusive.Length; i++)
		{
			androidExclusive[i].SetActive(enable);
		}
	}
}
