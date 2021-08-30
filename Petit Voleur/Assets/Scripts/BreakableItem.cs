using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableItem : MonoBehaviour
{
	public GameObject shards;
	public float breakValue = 0;
	public float impactThreshold = 10;
	private bool broken = false;

	void OnCollisionEnter(Collision collision)
	{
		if (!broken)
		{
			if (collision.relativeVelocity.sqrMagnitude > impactThreshold * impactThreshold)
			{
				Break();
				broken = true;
			}
		}
	}

	void Break()
	{
		//Add points here
		GameObject newShards = Instantiate(shards, transform.position, transform.rotation);
		newShards.transform.localScale = transform.localScale;
		Destroy(gameObject);
	}
}
