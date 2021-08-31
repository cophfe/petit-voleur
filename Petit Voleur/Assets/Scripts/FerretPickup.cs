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
	private FerretController controller;
	
    // Start is called before the first frame update
    void Awake()
    {
        controller = GetComponent<FerretController>();
    }

    // Update is called once per frame
    void Update()
    {
        
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
			Vector3 grabPoint = (closestCollider.transform.position - point);
			grabPoint -= (closestCollider.ClosestPoint(point) - point);
			grabPoint = grabTransform.InverseTransformVector(grabPoint);
			heldItem = closestCollider.attachedRigidbody.GetComponent<Item>();
			heldItem.Grab(grabTransform, grabPoint);
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
