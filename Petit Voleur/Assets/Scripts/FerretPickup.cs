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
	public float maxTravelSpeed = 400.0f;
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
			//Travel towards the target point
			Vector3 targetPoint = Vector3.MoveTowards(heldItem.rbody.position, grabTransform.TransformPoint(grabPoint), maxTravelSpeed * Time.deltaTime);
			heldItem.rbody.MovePosition(targetPoint);
			//Set the rotation
			heldItem.rbody.MoveRotation(grabTransform.transform.rotation * grabRotation);
			//Reset velocities
			heldItem.rbody.velocity = Vector3.zero;
			heldItem.rbody.angularVelocity = Vector3.zero;
		}
    }

	/// <summary>
	/// Try and grab an item
	/// </summary>
	/// <param name="point">Where to try and grab</param>
	/// <param name="range">How far from point we are checking</param>
	/// <returns></returns>
	bool GrabItem(Vector3 point, float range)
	{
		//Get list of colliders in range of the point
		Collider[] results = Physics.OverlapSphere(point, range, grabLayers);

		//Only loop through if a result was found
		if (results.Length > 0)
		{
			//Loop through and find the closest collider
			Collider closestCollider = results[0];
			float closestSqrDist = (closestCollider.transform.position - point).sqrMagnitude;
			float newSqrDist;
			for (int i = 0; i < results.Length; ++i)
			{
				//Compare square distance to the current closest square distance
				newSqrDist = (results[i].transform.position - point).sqrMagnitude;
				if (closestSqrDist > newSqrDist)
				{
					closestSqrDist = newSqrDist;
					closestCollider = results[i];
				}
			}
			//Since the object is in the item layer, it will have an Item component
			heldItem = closestCollider.attachedRigidbody.GetComponent<Item>();

			if (heldItem.pickupable)
			{
				//Grab point generation
				//Get the closest point on the item's bounds, move it so it is on the grab point
				grabPoint = (closestCollider.transform.position - point);
				grabPoint -= (closestCollider.attachedRigidbody.ClosestPointOnBounds(point) - point);
				//Convert from world to local space
				grabPoint = grabTransform.InverseTransformVector(grabPoint);
				//Convert world to local rotation
				grabRotation = Quaternion.Inverse(grabTransform.transform.rotation) * heldItem.transform.rotation;
				heldItem.Grab();
			}
			else
			{
				heldItem = null;
			}

			return true;
		}

		return false;
	}

	/// <summary>
	/// Drop the item
	/// </summary>
	void ReleaseItem()
	{
		heldItem.Release(controller.velocity);
		heldItem = null;
	}

	//Input
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
