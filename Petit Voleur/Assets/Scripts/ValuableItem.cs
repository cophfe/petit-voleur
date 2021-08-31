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
	public string stashTag = "Stash";

	PointTracker pointTracker = null;

	private void Start()
	{
		GameObject eS = GameObject.Find("EventSystem");
		if (eS != null)
			pointTracker = eS.GetComponent<PointTracker>();
	}
	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == stashTag && pointTracker != null)
		{
			pointTracker.AddPoints(pointValue);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.tag == stashTag && pointTracker != null)
		{
			pointTracker.SubtractPoints(pointValue);
		}
	}

}
