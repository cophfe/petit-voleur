using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EaseIt;

public class ItemShard : MonoBehaviour
{
	public static float fadePoint = 0.5f;
	
	public float destroyTime;
	public AudioSource audioSource;
	public AudioClip[] breakSounds;
	public Rigidbody[] pieces;
	public float minExplodeVelocity;
	public float maxExplodeVelocity;
	public bool scaleOut = false;
	private float timer;
	
	// Start is called before the first frame update
	void Start()
	{
		if (audioSource)
		{
			audioSource.PlayOneShot(breakSounds[Random.Range(0, breakSounds.Length)]);
		}
		
		timer = destroyTime;
		Destroy(gameObject, destroyTime);
		foreach(Rigidbody piece in pieces)
		{
			//Launch each shard outwards
			Vector3 direction = piece.transform.localPosition.normalized;
			piece.velocity = direction * Random.Range(minExplodeVelocity, maxExplodeVelocity);
		}
	}

	void Update()
	{
		timer -= Time.deltaTime;

		if (scaleOut)
		{
			if (timer < fadePoint)
			{
				transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, Easing.SmoothStart4(timer / fadePoint));
			}
		}
	}
}
