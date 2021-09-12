using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ChefAI))]
public class ChefVoices : MonoBehaviour
{
	public AudioSource source;
	[Tooltip("Wander, Inspect, Kick, Throw, Last Seen")]
	public VoicePack[] voicePacks = new VoicePack[5];
	private ChefAI chefAI;
	private float playTimer;
	// Start is called before the first frame update
	void Start()
	{
		chefAI = GetComponent<ChefAI>();

		//Update lengths!
		for (int i = 0; i < voicePacks.Length; ++i)
		{
			voicePacks[i].UpdateSumWeights();
		}
	}

	// Update is called once per frame
	void Update()
	{
		playTimer += Time.deltaTime;

		//Play a random clip if the play delay threshold is reached
		if (playTimer > voicePacks[(int)chefAI.currentState].delayPerPlay)
		{
			source.clip = voicePacks[(int)chefAI.currentState].GetRandomClip();
			source.Play();
			playTimer = 0;
		}
	}

	[System.Serializable]
	public class VoicePack
	{
		public VoiceClip[] clips;
		public float delayPerPlay = 5.0f;
		private float sumWeights = 0;

		//Calculate sum of all weights
		public void UpdateSumWeights()
		{
			sumWeights = 0;
			for(int i = 0; i < clips.Length; ++i)
			{
				sumWeights += clips[i].weight;
			}
		}

		//Get a weighted random clip
		public AudioClip GetRandomClip()
		{
			float randomNum = Random.Range(0, sumWeights);

			float cum = 0;
			int index = 0;

			//Iterate through all clips, adding their weight to the cumulative sum, then checking if the sum is larger than the random number
			for (index = 0; index < clips.Length; ++index)
			{
				cum += clips[index].weight;

				if (cum > randomNum)
					break;
			}

			if (index >= clips.Length)
			{
				index--;
			}
			
			return clips[index].clip;
		}

		//Used for weighted random selection
		[System.Serializable]
		public class VoiceClip
		{
			public AudioClip clip;
			public int weight = 1;
		}
	}
}
