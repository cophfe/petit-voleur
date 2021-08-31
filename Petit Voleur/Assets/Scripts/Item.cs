using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
	public Rigidbody rbody;
	private Collider col;
	private Transform originalParent;
	private RigidbodyInterpolation interpolateMode = RigidbodyInterpolation.None;

	void Awake()
	{
		rbody = GetComponent<Rigidbody>();
		col = GetComponent<Collider>();
	}

	public void Grab(Transform newParent, Vector3 offset)
	{
		originalParent = transform.parent;
		interpolateMode = rbody.interpolation;
		rbody.isKinematic = true;
		rbody.interpolation = RigidbodyInterpolation.None;
		col.enabled = false;
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
		col.enabled = true;
		rbody.velocity = velocity;
	}
}
