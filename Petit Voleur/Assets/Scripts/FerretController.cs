/*===================================================================
		  Created by Radongo Du Congo
		||----------------------------||
				  2021
===================================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FerretController : MonoBehaviour
{
	[Header("Status")]
	public Vector3 velocity;
	public bool grounded = false;
	public bool isRagdolled = false;
	public Vector3 upDirection = Vector3.up;
	public Vector3 floorNormal = Vector3.up;
	public Vector3 lookDirection = Vector3.forward;
	public Vector2 input;

	[Header("Ground Checking")]
	public LayerMask groundCheckLayerMask;
	public float groundCheckRadiusFactor = 0.85f;
	public float groundCheckDistance = 0.3f;

	[Header("Ferret forces")]
	public float acceleration = 50;
	public float targetSpeed = 30;
	public float floorFriction = 80;
	public float frictionMultiplier = 2;
	public float airControl = 0.4f;
	public float lookSpeed = 600f;
	//Measured in dot product
	public float maxVerticalAngle = 0.9f;
	public float gravity = 10;

	[Header("Jumping")]
	public bool isJumping = false;
	public JumpArc fallingArc;
	public JumpArc jumpArc;

	[Header("Wall Climbing")]
	public LayerMask climbableLayers;
	public bool isClimbing = false;
	public float climbFriction = 140;
	public float wallCheckDistance = 0.12f;
	public float wallCheckFactor = 0.9f;
	public int wallCheckAngles = 4;

	private CharacterController characterController;
	new private Rigidbody rigidbody;
	private float friction = 80;
	private Vector3 forward;
	private Vector3 targetVelocity;
	private Vector3 frameAcceleration;
	private float accelerationScaled;

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
		rigidbody = GetComponent<Rigidbody>();
		StopClimbing();
    }

    // FixedUpdate is called once per physics step
    void Update()
    {
		//No need to run these if the player is ragdolled
		if (isRagdolled)
			return;
		
		//Reset these
		targetVelocity = Vector3.zero;
		frameAcceleration = Vector3.zero;
		accelerationScaled = acceleration;

		//Calculate floor normal and ground check
			//Don't consider surfaces that are too steep
		RaycastHit rayhit;
		if (Physics.SphereCast(transform.position, characterController.radius * groundCheckRadiusFactor, -floorNormal, out rayhit, groundCheckDistance, groundCheckLayerMask)
			 && Vector3.Angle(upDirection, rayhit.normal) <= characterController.slopeLimit)
		{
			floorNormal = rayhit.normal;
			grounded = true;
		}
		else
		{
			//Reset floor normal
			StopClimbing();
			floorNormal = upDirection;
			grounded = false;
		}

		//Component of velocity that is parallel with the ground
		Vector3 planarVelocity = Vector3.ProjectOnPlane(velocity, floorNormal);
		//Project camera's forward vector on virtual plane that is the "floor", use global up vector if wall climbing
		Vector3 desiredCamVector;
		desiredCamVector = isClimbing? Vector3.up : Camera.main.transform.forward;
		forward = Vector3.ProjectOnPlane(desiredCamVector, floorNormal).normalized;

		if (input.x != 0 || input.y != 0)
		{
			// ~~~~~~~ Generate target velocity ~~~~~~ //
			//Forward component
			targetVelocity = forward * input.y;
			//Cross product the forward and floorNormal to get the right component
			targetVelocity -= Vector3.Cross(forward, floorNormal) * input.x;
			targetVelocity *= targetSpeed;
			
			//Desired change in velocity
			frameAcceleration = targetVelocity - planarVelocity;
			
			//Give a boost if trying to go away from current velocity
			if (Vector3.Dot(frameAcceleration, planarVelocity) < 0)
			{
				accelerationScaled = friction;
			}
		}
		else
		{
			//Add friction
			if (grounded)
			{
				frameAcceleration = -planarVelocity;
				accelerationScaled = friction;
			}
		}

		//Reduce control in the air
		if (!grounded)
			accelerationScaled *= airControl;
		
		//Amount of acceleration in this frame
		accelerationScaled *= Time.deltaTime;

		//Limit change in position to the player's acceleration in this frame
		if (frameAcceleration.sqrMagnitude > accelerationScaled * accelerationScaled)
		{
			frameAcceleration = frameAcceleration.normalized * accelerationScaled;
		}

		//Jump reset check
		if (isJumping)
		{
			//If the player is falling downwards
			if (Vector3.Dot(velocity, upDirection) <= 0)
			{
				CancelJump();
			}
		}

		//GRAVITY
		frameAcceleration -= upDirection * gravity * Time.deltaTime;
		//Add the acceleration calculated this frame to velocity
		velocity += frameAcceleration;
		
		//lets goooooooooooooooooooooo
		characterController.Move(velocity * Time.deltaTime);

		//Rotation
		Vector3 newLookDirection = lookDirection;
		//Follow the target direction when the player is grounded and trying to move
		if (grounded && targetVelocity.sqrMagnitude > 0.01f)
		{
			newLookDirection = targetVelocity.normalized;
		}
		else
		{
			//Set direction to direction of velocity unless velocity isn't high enough
			if (velocity.sqrMagnitude > 0.01f)
			{
				//If velocity is less than the maxverticalangle then point in the direction of velocity
				//Otherwise create a new look direction by adding the current velocity to the current look direction which
				//	allows for vertical influence on the look direction.
				//Without this the player will either look straight when jumping or look directly up and get confused
				Vector3 velDirection = velocity.normalized;
				if (Mathf.Abs(Vector3.Dot(velDirection, -floorNormal)) < maxVerticalAngle)
					newLookDirection = velDirection;
				else
					newLookDirection = (velDirection + lookDirection).normalized;
			}
			else
			{
				newLookDirection = Vector3.ProjectOnPlane(lookDirection, floorNormal);
			}
		}

		if (Mathf.Abs(Vector3.Dot(newLookDirection, -floorNormal)) < maxVerticalAngle)
			lookDirection = newLookDirection;

		transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lookDirection, floorNormal), lookSpeed * Time.deltaTime);
    }

	void OnControllerColliderHit(ControllerColliderHit hit)
	{
		//Remove component of velocity that is pointing into the collision
		/*----------------------------------------------------------------
						 \
						  \
						   \
							\--------------->
		-----------------------------------------------------------------*/
		
		velocity +=  hit.normal * Mathf.Max(Vector3.Dot(-hit.normal, velocity), 0);
	}

	public void CancelJump()
	{
		gravity = fallingArc.GetGravity();
		isJumping = false;
	}

	void StartClimbing(Vector3 newUp)
	{
		isClimbing = true;
		upDirection = newUp;
		floorNormal = newUp;
		friction = climbFriction;
		CancelJump();
	}

	void StopClimbing()
	{
		isClimbing = false;
		upDirection = Vector3.up;
		floorNormal = Vector3.up;
		friction = floorFriction;
	}

	public void SetRagdollState(bool state)
	{
		isRagdolled = state;

		characterController.enabled = !isRagdolled;
		
		if (!isRagdolled)
		{
			velocity = rigidbody.velocity;
			CancelJump();
		}

		rigidbody.isKinematic = !isRagdolled;

		if (isRagdolled)
		{
			rigidbody.velocity = velocity;
		}
	}

	public void OnMoveAxis(InputValue value)
	{
		input = value.Get<Vector2>();
	}

	public void OnJump()
	{
		if (!isClimbing)
		{
			RaycastHit rayhit;

			Vector3 rayDirection = Vector3.forward;

			for (int i = 0; i < wallCheckAngles; ++i)
			{
				Debug.DrawRay(transform.position, rayDirection * 3, Color.red, 2);
				if (Physics.SphereCast(transform.position, characterController.radius * wallCheckFactor, rayDirection, out rayhit, wallCheckDistance, climbableLayers))
				{
					StartClimbing(rayhit.normal);
					return;
				}
				rayDirection = Quaternion.Euler(0, 360.0f / (float)wallCheckAngles, 0) * rayDirection;
			}
			
		}
		if (grounded)
		{
			gravity = jumpArc.GetGravity();
			//Reset "vertical" velocity
			velocity -= upDirection * Vector3.Dot(velocity, upDirection);
			velocity += upDirection * jumpArc.GetJumpForce();
			isJumping = true;
		}
	}

	public void OnJumpRelease()
	{
		CancelJump();
	}

	public void OnInteract()
	{
		SetRagdollState(!isRagdolled);
	}

	//---------STRUCTS-------------------//

	[System.Serializable]
	public struct JumpArc
	{
		public float height;
		public float durationToPeak;

		public float GetJumpForce()
		{
			return (2 * height) / durationToPeak;
		}

		public float GetGravity()
		{
			return (2 * height) / (durationToPeak * durationToPeak);
		}
	}
}