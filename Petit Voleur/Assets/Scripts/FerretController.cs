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
[RequireComponent(typeof(FerretPickup))]
[RequireComponent(typeof(Rigidbody))]
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
	public Collider floorObject;
	public float groundCheckRadiusFactor = 0.85f;
	public float groundCheckDistance = 0.3f;

	[Header("Ferret forces")]
	public float acceleration = 50;
	public float targetSpeed = 30;
	public float floorFriction = 80;
	public float airControl = 0.4f;
	public float lookSpeed = 600f;
	//Measured in dot product
	public float maxVerticalAngle = 0.9f;
	public float gravity = 10;
	public float maxVelocity = 300.0f;

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

	[Header("Dashing and Impact")]
	public float defaultImpactMultiplier = 0.5f;
	public float dashVelocity = 40.0f;
	public float dashCooldown = 1.4f;
	private float dashCDTimer = 0f;

	private CharacterController characterController;
	new private Rigidbody rigidbody;
	private FerretPickup ferretPickup;
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
		ferretPickup = GetComponent<FerretPickup>();
		StopClimbing();
    }

    // FixedUpdate is called once per physics step
    void Update()
    {
		//No need to run these if the player is ragdolled
		if (isRagdolled)
			return;

		//Decrement dash timer
		if (dashCDTimer > 0)
			dashCDTimer -= Time.deltaTime;

		//GRAVITY
		velocity -= upDirection * gravity * Time.deltaTime;

		
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
			floorObject = rayhit.collider;
			grounded = true;
		}
		else
		{
			//Reset floor normal
			StopClimbing();
			floorNormal = upDirection;
			floorObject = null;
			grounded = false;
		}
		
		//Component of velocity that is parallel with the ground
		Vector3 planarVelocity = Vector3.ProjectOnPlane(velocity, floorNormal);
		//Project camera's forward vector on virtual plane that is the "floor", use global up vector if wall climbing
		Vector3 desiredForwardVector;
		desiredForwardVector = isClimbing? Vector3.up : Camera.main.transform.forward;
		forward = Vector3.ProjectOnPlane(desiredForwardVector, floorNormal).normalized;

		if (input.sqrMagnitude > 0)
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

		//Add the acceleration calculated this frame to velocity
		velocity += frameAcceleration;
		
		//Limit velocity so nothing breaks
		if (velocity.sqrMagnitude > maxVelocity * maxVelocity)
		{
			velocity = velocity.normalized * maxVelocity;
		}
		
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
		Vector3 impactVector = hit.normal * Mathf.Max(Vector3.Dot(-hit.normal, velocity), 0);
		
		if (hit.rigidbody)
		{
			if (hit.collider == floorObject)
			{
				hit.rigidbody.AddForceAtPosition(-impactVector * defaultImpactMultiplier, hit.point, ForceMode.Impulse);
				velocity += impactVector;
			}
			else
			{
				hit.rigidbody.AddForceAtPosition(-impactVector * defaultImpactMultiplier, hit.point, ForceMode.Force);
				velocity += impactVector * Time.fixedDeltaTime;
			}
		}
		else
		{
			velocity +=  impactVector;
		}
	}

	void Dash()
	{
		if (dashCDTimer <= 0)
		{
			Vector3 dashDirection = forward;
			if (input.sqrMagnitude > 0)
				dashDirection = targetVelocity.normalized;

			velocity = dashDirection * dashVelocity;
			dashCDTimer = dashCooldown;
		}
	}

	void ResetDash()
	{
		dashCDTimer = 0;
	}

	void StartJump()
	{
		gravity = jumpArc.GetGravity();
		//Reset "vertical" velocity
		velocity -= upDirection * Vector3.Dot(velocity, upDirection);
		velocity += upDirection * jumpArc.GetJumpForce();
		isJumping = true;
	}

	void CancelJump()
	{
		gravity = fallingArc.GetGravity();
		isJumping = false;
	}

	bool TryClimb()
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
					return true;
				}
				rayDirection = Quaternion.Euler(0, 360.0f / (float)wallCheckAngles, 0) * rayDirection;
			}
		}

		return false;
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
			StopClimbing();
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
		//Try climb first, otherwise jump
		if (!TryClimb() && grounded)
		{
			StartJump();
		}
	}

	public void OnJumpRelease()
	{
		CancelJump();
	}

	public void OnDash()
	{
		Dash();
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