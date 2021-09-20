/*----------------------------------------------------------------------------------
			** Created by Rahul J **
			------------------------
			2021
			`````````````````````````
			Makes a noise on collision
================================================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
public class PhysAudio : MonoBehaviour
{
    private Rigidbody rb;
    private AudioSource source;
    public AudioClip impactSound;
    public float minImpactSpeed = 0.4f;
    public float maxImpactSpeed = 5.0f;
    public float minVolume = 0.1f;
    public float maxVolume = 2.5f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        source = GetComponent<AudioSource>();
    }

    void OnCollisionEnter(Collision collision)
    {
            ContactPoint contact = collision.GetContact(0);
        //Calculate the component of velocity that is going INTO the wall
		Vector3 impactVector = -contact.normal * Mathf.Max(-Vector3.Dot(contact.normal, rb.velocity), 0);

        float impactSpeed = impactVector.magnitude;

        if (impactSpeed > minImpactSpeed)
        {
            float volume = Mathf.Lerp(minVolume, maxVolume, impactSpeed / maxImpactSpeed);
			source.clip = impactSound;
			source.volume = volume;
			source.Play();
        }
    }
}
