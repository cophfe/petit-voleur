using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FerretPickup))]
public class FerretPickup : MonoBehaviour
{
	public float grabRange = 1;
	public Transform grabTransform;
	public LayerMask grabLayers;
	public Item heldItem;
	public float appliedForce = 30.0f;
	private FerretController controller;
	private Vector3 grabPoint;
	private Quaternion grabRotation;
	
    // Start is called before the first frame update
    void Awake()
    {
        controller = GetComponent<FerretController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (heldItem)
		{
			Vector3 targetPoint = grabTransform.TransformPoint(grabPoint);
			
			heldItem.rbody.MovePosition(targetPoint);
			heldItem.rbody.MoveRotation(grabTransform.transform.rotation * grabRotation);
			heldItem.rbody.velocity = Vector3.zero;
			heldItem.rbody.angularVelocity = Vector3.zero;
		}
    }

	void GrabItem(Vector3 point, float range)
	{
		Collider[] results = Physics.OverlapSphere(point, range, grabLayers);

		if (results.Length > 0)
		{
			Collider closestCollider = results[0];
			float closestSqrDist = (closestCollider.transform.position - point).sqrMagnitude;
			float newSqrDist;
			for (int i = 0; i < results.Length; ++i)
			{
				newSqrDist = (results[i].transform.position - point).sqrMagnitude;
				if (closestSqrDist > newSqrDist)
				{
					closestSqrDist = newSqrDist;
					closestCollider = results[i];
				}
			}
			heldItem = closestCollider.attachedRigidbody.GetComponent<Item>();

			if (heldItem.pickupable)
			{
				grabPoint = (closestCollider.transform.position - point);
				grabPoint -= (closestCollider.ClosestPoint(point) - point);
				grabPoint = grabTransform.InverseTransformVector(grabPoint);
				grabRotation = Quaternion.Inverse(grabTransform.transform.rotation) * heldItem.transform.rotation;
				heldItem.Grab();
			}
		}
	}

	void ReleaseItem()
	{
		heldItem.Release(controller.velocity);
		heldItem = null;
	}

	void OnGrab()
	{
		if (heldItem)
		{
			ReleaseItem();
		}
		else
		{
			GrabItem(grabTransform.position, grabRange);
		}
	}
}
