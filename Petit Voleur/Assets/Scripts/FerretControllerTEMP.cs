using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FerretController : MonoBehaviour
{
	private CharacterController controller;
    public float speed;
	public float rotationSpeed;
	public Vector2 input;
	public Vector3 floorNormal = Vector3.up;
	public float groundCheck = 0.01f;

	void Start()
	{
		controller = GetComponent<CharacterController>();
	}

	void FixedUpdate()
	{

		RaycastHit rayHit;
		Ray ray = new Ray(transform.position, -floorNormal);
		if (Physics.Raycast(ray, out rayHit, groundCheck + controller.radius))
		{
			if (rayHit.normal != floorNormal)
			{
				Quaternion rot = Quaternion.FromToRotation(floorNormal, rayHit.normal);
				floorNormal = rayHit.normal;
				transform.rotation = rot * transform.rotation;
			}
		}
		
		transform.Rotate(Vector3.up * rotationSpeed * Time.fixedDeltaTime * input.x, Space.Self);
		Vector3 moveVec = Physics.gravity * Time.fixedDeltaTime;
		moveVec += transform.forward * input.y * speed * Time.fixedDeltaTime;
		CollisionFlags flags = controller.Move(moveVec);
		
	}

	void OnDrawGizmos()
	{
		Gizmos.DrawLine(transform.position, transform.position + floorNormal * 3);
	}

	public void OnMoveAxis(InputValue value)
	{
		input = value.Get<Vector2>();
	}
}
