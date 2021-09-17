using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChefThrowable : MonoBehaviour
{
	public Rigidbody rb;
	public Transform colliders;
	public AudioSource audioSource;
    public LayerMask ferretLayer;
	public float velocityThreshold;
	public float ragdollDuration;
	public float impulse;
	public int damage = 1;
	private bool hitPlayer = false;

	void OnCollisionEnter(Collision collision)
	{
		if (gameObject.layer != 0)
		{
			gameObject.layer = 0;

			foreach (Transform t in colliders)
				t.gameObject.layer = 0;
		}
		if (!hitPlayer && (rb.velocity.sqrMagnitude > velocityThreshold * velocityThreshold))
		{

			if ((ferretLayer.value & (1 << collision.gameObject.layer)) > 0)
			{
				if (collision.rigidbody)
				{
					FerretController ferret = collision.rigidbody.GetComponent<FerretController>();
					ferret.health.Damage(damage);
					ferret.StartRagdoll(ragdollDuration);
					ferret.rigidbody.velocity = rb.velocity.normalized * impulse;
					hitPlayer = true;

					Vector3 shakeDirection = ferret.cameraController.transform.InverseTransformDirection(rb.velocity.normalized * 3);
					ferret.cameraController.AddCameraShake(shakeDirection);

					if (audioSource)
					{
						audioSource.Play();
					}
				}
			}
		}
	}
}
