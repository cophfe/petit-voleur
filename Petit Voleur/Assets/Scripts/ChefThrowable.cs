using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChefThrowable : MonoBehaviour
{
	public Rigidbody rb;
	public Transform colliders;
	public AudioSource audioSource;
    public LayerMask ferretLayer;
	public float ragdollDuration;
	public float impulse;
	public int damage = 1;
	private bool hitPlayer = false;

	void OnCollisionEnter(Collision collision)
	{
		if (!hitPlayer)
		{
			if (gameObject.layer != 0)
			{
				//gameObject.layer = 0;

				foreach(Transform t in colliders)
					t.gameObject.layer = 0;

				if ((ferretLayer.value & (1 << collision.gameObject.layer)) > 0)
				{
					if (collision.rigidbody)
					{
						FerretController ferret = collision.rigidbody.GetComponent<FerretController>();
						ferret.health.Damage(damage);
						ferret.StartRagdoll(ragdollDuration);
						ferret.rigidbody.velocity = rb.velocity.normalized * impulse;
						hitPlayer = true;

						if (audioSource)
						{
							audioSource.Play();
						}
					}
				}
			}
		}
	}
}
