using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "VoicePack", menuName = "ScriptableObjects/Voice Pack")]
public class VoicePack : ScriptableObject
{
	[Tooltip("Order is: Wander, Inspect, Kick, Throw, Last Seen Pos")]
	public VoiceSet[] voiceSets = new VoiceSet[5];
}

[System.Serializable]
public class VoiceSet
{
	public VoiceClip[] clips;
	public float delayPerPlay = 5.0f;

	//Used for weighted random selection
	[System.Serializable]
	public class VoiceClip
	{
		public AudioClip clip;
		public int weight = 1;
	}
}