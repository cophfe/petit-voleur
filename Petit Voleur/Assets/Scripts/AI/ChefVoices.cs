using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ChefAI))]
public class ChefVoices : MonoBehaviour
{
	public AudioSource source;
	[Tooltip("Wander, Inspect, Kick, Throw, Last Seen")]
	public VoicePack voicePack;
	private ChefAI chefAI;
	private float playTimer;
	private int[] sumWeights;

	// Start is called before the first frame update
	void Start()
	{
		chefAI = GetComponent<ChefAI>();

		sumWeights = new int[voicePack.voiceSets.Length];
		//Update lengths!
		for (int i = 0; i < sumWeights.Length; ++i)
		{
			sumWeights[i] = SumWeights(voicePack.voiceSets[i]);
		}
	}

	// Update is called once per frame
	void Update()
	{
		playTimer += Time.deltaTime;

		//Play a random clip if the play delay threshold is reached
		if (playTimer > voicePack.voiceSets[(int)chefAI.currentState].delayPerPlay)
		{
			source.clip = GetRandomClip((int)chefAI.currentState);
			source.Stop();
			source.Play();
			playTimer = 0;
		}
	}

	/// <summary>
	/// Calculates the sum of all weights in a voice set
	/// </summary>
	/// <param name="set"></param>
	/// <returns></returns>
	private int SumWeights(VoiceSet set)
	{
		int sum = 0;
		for(int i = 0; i < set.clips.Length; ++i)
		{
			sum += set.clips[i].weight;
		}

		return sum;
	}

	/// <summary>
	/// Get a random weighted clip from the voice set at given index
	/// </summary>
	/// <param name="index">Index of voice set from the list. Usually based on AI state.</param>
	/// <returns></returns>
	private AudioClip GetRandomClip(int index)
	{
		VoiceSet set = voicePack.voiceSets[index];
		float randomNum = Random.Range(0, sumWeights[index]);

		float cum = 0;
		int i = 0;

		//Iterate through all clips, adding their weight to the cumulative sum, then checking if the sum is larger than the random number
		for (i = 0; i < set.clips.Length; ++i)
		{
			cum += set.clips[i].weight;

			if (cum > randomNum)
				break;
		}

		//Since the index is added one final time if it reaches the end of the loop, we restore the index to its proper range
		if (i >= set.clips.Length)
		{
			i--;
		}
		
		return set.clips[i].clip;
	}

	
}
