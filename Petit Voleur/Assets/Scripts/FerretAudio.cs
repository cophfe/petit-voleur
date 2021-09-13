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
	
	void PlaySound(AudioClip clip)
	{
		source.clip = clip;
		source.Play();
	}

	void PlayQuickSound(AudioClip clip)
	{
		source.PlayOneShot(clip);
	}

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
	
	public void PlayFerretLanded()
	{
		PlayQuickSound(ferretLanded);
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
