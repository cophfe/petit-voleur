using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
	public bool isValuable = false;
	public bool isBreakable = false;
	public float itemValue = 1;
	[Header("Item Breaking")]
	public GameObject shards;
	public float impactThreshold = 10;

	void OnCollisionEnter(Collision collision)
	{
		if (collision.relativeVelocity.sqrMagnitude > impactThreshold * impactThreshold)
		{
			Break();
		}
	}

	void Break()
	{
		//Add points here
		GameObject newShards = Instantiate(shards, transform.position, transform.rotation);
		Destroy(gameObject);
	}
}
