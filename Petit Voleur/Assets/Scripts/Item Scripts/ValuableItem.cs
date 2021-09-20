/*==================================================
	Programmer: Connor Fettes
==================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Item))]
public class ValuableItem : MonoBehaviour
{
	public int pointValue = 1;

	bool stashed = false;
	bool stashable = false;
	PointTracker pointTracker = null;

	private void Start()
	{
		pointTracker = GameObject.FindObjectOfType<PointTracker>();
	}

	private void OnTriggerEnter(Collider other)
	{
		stashable = true;
		
	}

	private void OnTriggerExit(Collider other)
	{
		stashable = false;
	}

	/// <summary>
	/// Attempt to stash this item. This will fail if the item isn't inside the stash.
	/// </summary>
	public void TryStash()
	{
		if (!stashed && stashable)
		{
			pointTracker.AddPoints(pointValue);
			GetComponent<Item>().pickupable = false;
			stashed = true;
		}

	}

}
