/*==================================================
	Programmer: Connor Fettes
==================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValuableItem : MonoBehaviour
{
	public int pointValue = 1;
	public string stashTag = "Stash";
	//public GameManager gameManager;

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == stashTag)
		{
			//gameManager.AddPoints(pointValue);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.tag == stashTag)
		{
			//gameManager.AddPoints(-pointValue);
		}
	}

}
