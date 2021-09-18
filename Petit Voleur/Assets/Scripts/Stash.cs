/*==================================================
	Programmer: Connor Fettes
==================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stash : MonoBehaviour
{
	GameManager gameManager = null;
	public LayerMask ferretLayer;

	private void Start()
	{
		gameManager = FindObjectOfType<GameManager>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (((1 << other.gameObject.layer) & ferretLayer.value) > 0 && gameManager != null)
		{
			gameManager.OnEnterStash();
		}
	}
	private void OnTriggerExit(Collider other)
	{
		if (((1 << other.gameObject.layer) & ferretLayer.value) > 0 && gameManager != null)
		{
			gameManager.OnExitStash();
		}
	}

}
