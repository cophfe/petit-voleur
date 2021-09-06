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
		
		Vector3 projectedForward = Vector3.ProjectOnPlane(Camera.main.transform.forward, controller.upDirection);
		Vector3 projectedUp = Vector3.Project(Camera.main.transform.forward, controller.upDirection);
		Vector3 projectedVec = (projectedForward + projectedUp).normalized;
		Quaternion targetRotation = Quaternion.RotateTowards(Quaternion.LookRotation(transform.forward, controller.upDirection), Quaternion.LookRotation(projectedVec, controller.upDirection), maxAngle);
		neck.rotation = Quaternion.RotateTowards(neck.rotation, targetRotation * rotationOffset, lookSpeed * Time.deltaTime);
	}
}
