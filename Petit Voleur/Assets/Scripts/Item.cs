using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Item : MonoBehaviour
{
	//Statics
	public static int itemLayer = 10;
	public static int heldItemLayer = 13;

	public bool pickupable = true;
	[HideInInspector]
	public Rigidbody rbody;
	public GameObject col;

	void Awake()
	{
		rbody = GetComponent<Rigidbody>();
	}

	/// <summary>
	/// Set the item to a grabbed state
	/// </summary>
	public void Grab()
	{
		SetLayerRecursively(gameObject, heldItemLayer);
		rbody.useGravity = false;
	}

	/// <summary>
	/// Drop the item
	/// </summary>
	/// <param name="velocity">What velocity to assign to the object after dropping</param>
	public void Release(Vector3 velocity)
	{
		SetLayerRecursively(gameObject, itemLayer);
		rbody.velocity = velocity;
		rbody.useGravity = true;
	}

	public static void SetLayerRecursively(GameObject obj, int newLayer)
	{
		obj.layer = newLayer;

		foreach (Transform child in obj.transform)
		{
			SetLayerRecursively(child.gameObject, newLayer);
		}
	}
}
