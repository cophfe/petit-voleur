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
	PointTracker pointTracker = null;

	private void Start()
	{
		GameObject eS = GameObject.Find("EventSystem");
		if (eS != null)
			pointTracker = eS.GetComponent<PointTracker>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!stashed && other.gameObject.tag == "Stash")
		{
			pointTracker.AddPoints(pointValue);
			GetComponent<Item>().pickupable = false;
			stashed = true;
		}
	}

}
