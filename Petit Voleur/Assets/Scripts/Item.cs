using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Item : MonoBehaviour
{
	public bool pickupable = true;
	[HideInInspector]
	public Rigidbody rbody;
	public GameObject col;
	private Transform originalParent;
	private RigidbodyInterpolation interpolateMode = RigidbodyInterpolation.None;

	void Awake()
	{
		rbody = GetComponent<Rigidbody>();
	}

	public void Grab(Transform newParent, Vector3 offset)
	{
		originalParent = transform.parent;
		interpolateMode = rbody.interpolation;
		rbody.isKinematic = true;
		rbody.interpolation = RigidbodyInterpolation.None;
		col.SetActive(false);
		transform.SetParent(newParent);
		transform.localPosition = offset;
	}

	public void Grab(Transform newParent)
	{
		Grab(newParent, Vector3.zero);
	}

	public void Release(Vector3 velocity)
	{
		transform.SetParent(originalParent);

		rbody.interpolation = interpolateMode;
		rbody.isKinematic = false;
		col.SetActive(true);
		rbody.velocity = velocity;
	}
}
