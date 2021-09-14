using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FerretLook : MonoBehaviour
{
    public Transform neck;
	public Transform lookTarget;
	public float maxAngle = 30.0f;
	public float lookSpeed = 300.0f;
	private FerretController controller;
	private Quaternion rotationOffset;

	void Awake()
	{
		controller = GetComponent<FerretController>();
		rotationOffset = neck.rotation;
	}

	void Update()
	{
		if (!controller.isRagdolled)
		{
			//Create a vector by projecting the camera's forward on the upDirection plane to get the horizontal component
			Vector3 projectedForward = Vector3.ProjectOnPlane(Camera.main.transform.forward, controller.upDirection);
			//Do the same thing but using normal projection
			Vector3 projectedUp = Vector3.Project(Camera.main.transform.forward, controller.upDirection);
			//Combine to make a direction vector based on the camera that is aligned with the ground
			Vector3 projectedVec = (projectedForward + projectedUp).normalized;

			//Desired rotation, limited by max angle
			//Quaternion a is the character controller's forward
			//Quaternion b is the projected vector from earlier (where the camera is kinda looking)
			Quaternion targetRotation = Quaternion.RotateTowards(Quaternion.LookRotation(transform.forward, controller.upDirection), Quaternion.LookRotation(projectedVec, controller.upDirection), maxAngle);
			//Limit rotation based on lookspeed
			neck.rotation = Quaternion.RotateTowards(neck.rotation, targetRotation * rotationOffset, lookSpeed * Time.deltaTime);
		}
	}
}
