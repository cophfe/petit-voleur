/*----------------------------------------------------------------------------------
			** Created by Rahul J **
			------------------------
			2021
================================================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Item))]
public class BreakableItem : MonoBehaviour
{
	private PointTracker pointTracker;
	public GameObject shards;
	public int breakValue = 0;
	public float impactThreshold = 10;
	private ChefAI chef;
	private bool broken = false;

	void Start()
	{
		pointTracker = FindObjectOfType<PointTracker>();
		chef = FindObjectOfType<ChefAI>();
	}

	void OnCollisionEnter(Collision collision)
	{
		if (!broken)
		{
			//Get the first contact point normal
			Vector3 contactNormal = collision.GetContact(0).normal;
			//Calculate the amount of relative velocity going into the contact wall. This avoids slides from smashing the object
			float impactVector = Vector3.Dot(collision.relativeVelocity, contactNormal);
			//Break if the impactVector is too high
			if (impactVector > impactThreshold)
			{
				Break();
				broken = true;
			}
		}
	}

	/// <summary>
	/// Break object and add points
	/// </summary>
	void Break()
	{
		pointTracker.AddPoints(breakValue);
		Destroy(gameObject);
		GameObject newShards = Instantiate(shards, transform.position, transform.rotation);
		newShards.transform.localScale = transform.localScale;

		if (chef)
		{
			chef.SetSoundPoint(transform.position);
		}
	}
}
