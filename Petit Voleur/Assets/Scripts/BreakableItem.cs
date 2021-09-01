using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Item))]
public class BreakableItem : MonoBehaviour
{
	public PointTracker pointTracker;
	public GameObject shards;
	public int breakValue = 0;
	public float impactThreshold = 10;
	private bool broken = false;

	void OnCollisionEnter(Collision collision)
	{
		if (!broken)
		{
			Vector3 contactNormal = collision.GetContact(0).normal;
			float impactVector = Vector3.Dot(collision.relativeVelocity, contactNormal);
			if (impactVector > impactThreshold)
			{
				Break();
				broken = true;
			}
		}
	}

	void Break()
	{
		pointTracker.AddPoints(breakValue);
		Destroy(gameObject);
		GameObject newShards = Instantiate(shards, transform.position, transform.rotation);
		newShards.transform.localScale = transform.localScale;
	}
}
