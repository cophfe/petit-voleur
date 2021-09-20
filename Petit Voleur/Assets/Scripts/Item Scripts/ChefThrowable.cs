/*----------------------------------------------------------------------------------
			** Created by Rahul J **
			------------------------
			2021
================================================================================*/

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
		//Change the object's layer so throwables collide with eachother
		if (gameObject.layer != 0)
		{
			gameObject.layer = 0;

			foreach (Transform t in colliders)
				t.gameObject.layer = 0;
		}
		//only hit the player onc and if the throwable is going fast
		if (!hitPlayer && (rb.velocity.sqrMagnitude > velocityThreshold * velocityThreshold))
		{
			//Ensure the collision is in the ferret layer
			if ((ferretLayer.value & (1 << collision.gameObject.layer)) > 0)
			{
				if (collision.rigidbody)
				{
					//Deal damage to the ferret and start ragdolling it
					FerretController ferret = collision.rigidbody.GetComponent<FerretController>();
					ferret.health.Damage(damage);
					ferret.StartRagdoll(ragdollDuration);
					//Yeet the ferret
					ferret.rigidbody.velocity = rb.velocity.normalized * impulse;
					hitPlayer = true;

					//Screen shake
					Vector3 shakeDirection = ferret.cameraController.transform.InverseTransformDirection(rb.velocity.normalized * 3);
					ferret.cameraController.AddCameraShake(shakeDirection);

					//CLANG sfx
					if (audioSource)
					{
						audioSource.Play();
					}
				}
			}
		}
	}
}
