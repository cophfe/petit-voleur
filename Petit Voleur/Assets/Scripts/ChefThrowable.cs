using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChefThrowable : MonoBehaviour
{
	public Rigidbody rb;
	public Transform colliders;
    public LayerMask ferretLayer;
	public float ragdollDuration;
	public float impulse;

	void OnCollisionEnter(Collision collision)
	{
		if (gameObject.layer != 0)
		{
			gameObject.layer = 0;

			foreach(Transform t in colliders)
				t.gameObject.layer = 0;

			if ((ferretLayer.value & (1 << collision.gameObject.layer)) > 0)
			{
				if (collision.rigidbody)
				{
					FerretController ferret = collision.rigidbody.GetComponent<FerretController>();
					ferret.StartRagdoll(ragdollDuration);
					ferret.rigidbody.velocity = rb.velocity.normalized * impulse;
				}
			}
		}
	}
}
