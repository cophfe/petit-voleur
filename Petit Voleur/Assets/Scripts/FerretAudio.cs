using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FerretAudio : MonoBehaviour
{
    public AudioSource source;
	public AudioClip wallImpact;
	public AudioClip itemImpact;
	public AudioClip ferretDash;
	public AudioClip ferretJump;
	public AudioClip ferretLanded;
	public AudioClip ferretKicked;
	public AudioClip ferretDead;
	
	/// <summary>
	/// Sets the clip to the audio source and plays it. For longer sfx
	/// </summary>
	/// <param name="clip"></param>
	void PlaySound(AudioClip clip)
	{
		source.clip = clip;
		source.Play();
	}

	/// <summary>
	/// Plays a clip as a one shot, not being interrupted
	/// </summary>
	/// <param name="clip"></param>
	void PlayQuickSound(AudioClip clip, float volume = 1.0f)
	{
		source.PlayOneShot(clip, volume);
	}

	// ====================================================== //
	// =========== These are all self explanatory =========== //
	// ============VVVVVVVVVVVVVVVVVVVVVVVVVVVVVV============ //

	public void PlayWallImpact()
	{
		PlayQuickSound(wallImpact);
	}

	public void PlayItemImpact()
	{
		PlayQuickSound(itemImpact);
	}

	public void PlayFerretDash()
	{
		PlayQuickSound(ferretDash);
	}

	public void PlayFerretJump()
	{
		PlayQuickSound(ferretJump);
	}
	
	public void PlayFerretLanded(float volume = 1.0f)
	{
		PlayQuickSound(ferretLanded, volume);
	}

	public void PlayFerretKicked()
	{
		PlayQuickSound(ferretKicked);
	}

	public void PlayFerretDead()
	{
		PlayQuickSound(ferretDead);
	}

}
