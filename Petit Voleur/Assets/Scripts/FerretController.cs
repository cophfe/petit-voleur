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
	public float maxVelocityFollowAngle = 0.9f;
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
	private Vector3 projectedInput;
	private Vector3 targetVelocity;
	private Vector3 deltaVelocity;
	private float accelerationThisFrame;

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

		Move();

		DoRotation();

		//Decrement dash timer when on the ground
		if (grounded && dashCDTimer > 0)
			dashCDTimer -= Time.deltaTime;
    }

	// ========================================================|
	//		--- Collision Response ---
	//--------------------------------------------------------/
	void OnControllerColliderHit(ControllerColliderHit hit)
	{
		//-----------------------------------------------------------------
		//Remove component of velocity that is pointing into the collision
		/*-----------------------------------------------------------------
						 \
						  \
						   \
							\--------------->
		-----------------------------------------------------------------*/
		
		//Calculate the component of velocity that is going INTO the wall
		Vector3 impactVector = -hit.normal * Mathf.Max(-Vector3.Dot(hit.normal, velocity), 0);
		
		//Apply forces to other rigidbody if the other object has a rigidbody
		if (hit.rigidbody)
		{
			//If the player is standing on a physics object, then impart all velocity into the object. Remove that same velocity from the player
			if (hit.collider == floorObject)
			{
				hit.rigidbody.AddForceAtPosition(impactVector * defaultImpactMultiplier, hit.point, ForceMode.Impulse);
				velocity -= impactVector;
			}
			//Otherwise add the force over time
			else
			{
				hit.rigidbody.AddForceAtPosition(impactVector * defaultImpactMultiplier, hit.point, ForceMode.Force);
				velocity -= impactVector * Time.fixedDeltaTime;
			}
		}
		else
		{
			velocity -=  impactVector;
		}
	}

	// ========================================================|
	//		--- Main Movement Loop ---
	//--------------------------------------------------------/
	void Move()
	{
		//GRAVITY
		//Happens before everything else so that slope calculations can include gravity and resolve it BEFORE rotation happens
		velocity -= upDirection * gravity * Time.deltaTime;

		//Reset these
		projectedInput = Vector3.zero;
		targetVelocity = Vector3.zero;
		deltaVelocity = Vector3.zero;
		accelerationThisFrame = acceleration;

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
			floorObject = null;
			grounded = false;
		}
				
		//Project camera's forward vector on virtual plane that is the "floor", use global up vector if wall climbing
		//This is great for slopes, as the player will go up and down the slope, rather than trying to go into it
		Vector3 desiredForwardVector;
		desiredForwardVector = isClimbing? Vector3.up : Camera.main.transform.forward;
		forward = Vector3.ProjectOnPlane(desiredForwardVector, floorNormal).normalized;

		//  Component of velocity that is parallel with the ground  //
		//Necessary to ensure friction doesn't affect "vertical" component of velocity
		Vector3 planarVelocity = Vector3.ProjectOnPlane(velocity, floorNormal);

		// ====================================================== //
		// === //  Calculating Desired Change in Velocity  // === //
		// ====================================================== //
			//Desired change in velocity is defined by the difference between how fast we are going and how fast we want to go.
			//By using this desired change in velocity every frame, but limited by the max acceleration per frame we can ensure
			//that constant acceleration is maintained, and the velocity never overshoots
		
		//Input
		if (input.sqrMagnitude > 0)
		{
			// ~~~~~~~ Generate target velocity ~~~~~~ //
			projectedInput = forward * input.y;									//Forward component
			projectedInput -= Vector3.Cross(forward, floorNormal) * input.x;	//Cross product the forward and floorNormal to get the left component
			targetVelocity = projectedInput * targetSpeed;						//Multiply by target speed to set the magnitude of targetVelocity
			
			//Desired change in velocity along the plane
			deltaVelocity = targetVelocity - planarVelocity;
			
			// ~~~~~~~~~~~~~~~ Backkick ~~~~~~~~~~~~~~ //
			//Give a boost if trying to go away from current velocity. Feels better.
			if (Vector3.Dot(deltaVelocity, planarVelocity) < 0)
			{
				accelerationThisFrame = friction;
			}
		}
		//No input
		else
		{
			// ~~~~~~~~~~~~~ Add Friction ~~~~~~~~~~~~ //
			//Set the desired change in velocity to the inverse of the current planar velocity
			if (grounded)
			{
				deltaVelocity = -planarVelocity;
				accelerationThisFrame = friction;
			}
		}
	
		//Reduce control in the air
		if (!grounded)
			accelerationThisFrame *= airControl;
		
		//Finally limit acceleration value to be for this frame
		accelerationThisFrame *= Time.deltaTime;

		//Limit change in position to accelerationThisFrame
		if (deltaVelocity.sqrMagnitude > accelerationThisFrame * accelerationThisFrame)
		{
			deltaVelocity = deltaVelocity.normalized * accelerationThisFrame;
		}

		//Reset jump if the player has reached the peak of the jump (player is now falling)
		if (isJumping && Vector3.Dot(velocity, upDirection) <= 0)
			CancelJump();

		//Add the acceleration calculated this frame to velocity
		velocity += deltaVelocity;
		
		//Hard limit to totalVelocity
		if (velocity.sqrMagnitude > maxVelocity * maxVelocity)
		{
			velocity = velocity.normalized * maxVelocity;
		}
		
		//lets goooooooooooooooooooooo
		characterController.Move(velocity * Time.deltaTime);
	}

	// ========================================================|
	//		--- Character Rotation ---
	//--------------------------------------------------------/
	void DoRotation()
	{
		//Propose a new lookDirection
		Vector3 newLookDirection = lookDirection;

		//Follow the target direction when the player is grounded and trying to move
		if (grounded)
		{
			if (projectedInput.sqrMagnitude > 0.01f)
				newLookDirection = projectedInput;
			//Set the new lookDirection to be the same as the old one but projected on the floor
			//This is great for situations where the player changes surfaces
			else
				newLookDirection = Vector3.ProjectOnPlane(lookDirection, floorNormal);
			
		}
		//Set direction to direction of velocity unless velocity isn't high enough
		else if (velocity.sqrMagnitude > 0.01f)
		{
			//If velocity is less than the maxVelocityFollowAngle then point in the direction of velocity
			//Otherwise create a new look direction by adding the current velocity to the current look direction which
			//	allows for vertical influence on the look direction.
			//Without this the player will either look straight when jumping or look directly up and get confused
			Vector3 velDirection = velocity.normalized;
			float velAngle = Mathf.Abs(Vector3.Angle(velDirection, floorNormal) - 90);
			if (velAngle < maxVelocityFollowAngle)
				newLookDirection = velDirection;
			else
				newLookDirection = (velDirection + lookDirection).normalized;
		}

		float lookDirAngle = Mathf.Abs(Vector3.Angle(newLookDirection, floorNormal) - 90);

		if (lookDirAngle < maxVelocityFollowAngle)
			lookDirection = newLookDirection;
		else
			lookDirection = forward;

		transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lookDirection, floorNormal), lookSpeed * Time.deltaTime);
	}

	// ========================================================|
	//		--- Start Dash ---
	//--------------------------------------------------------/
	//Starts a dash if the dash cooldown is done
	void Dash()
	{
		if (dashCDTimer <= 0)
		{
			//Default to lookDirection
			Vector3 dashDirection = lookDirection;
			//Use projected input if possible
			if (input.sqrMagnitude > 0)
				dashDirection = projectedInput;

			velocity = dashDirection * dashVelocity;
			dashCDTimer = dashCooldown;
		}
	}

	// ========================================================|
	//		--- Reset Dash ---
	//--------------------------------------------------------/
	//Simply clears the dash cooldown
	void ResetDash()
	{
		dashCDTimer = 0;
	}

	// ========================================================|
	//		--- Starts a jump ---
	//--------------------------------------------------------/
	//Change gravity value based on the jump arc
	//Removes the vertical component of velocity and adds an impulse based on jump arc
	void StartJump()
	{
		gravity = jumpArc.GetGravity();
		//Reset "vertical" velocity
		velocity -= upDirection * Vector3.Dot(velocity, upDirection);
		velocity += upDirection * jumpArc.GetJumpForce();
		isJumping = true;
	}

	// ========================================================|
	//		--- Cancels a jump ---
	//--------------------------------------------------------/
	//Resets gravity to normal and cancels the jump
	void CancelJump()
	{
		gravity = fallingArc.GetGravity();
		isJumping = false;
	}

	// ========================================================|
	//		--- Try to start climbing ---
	//--------------------------------------------------------/
	//Will cast spheres to check surrounding walls and try to stick to one
	bool TryClimb()
	{
		//No need to climb if youo're already climbing
		if (!isClimbing)
		{
			RaycastHit rayhit;
			//Initial rayDirection for the first pass
			Vector3 rayDirection = Vector3.forward;
			//A quaternion that is rotated on the y axis by an amount to evenly spread out rayDirections by the number of wallCheckAngles
			Quaternion evenRotation = Quaternion.Euler(0, 360.0f / (float)wallCheckAngles, 0);

			for (int i = 0; i < wallCheckAngles; ++i)
			{
				//Casts a sphere in rayDirection
				if (Physics.SphereCast(transform.position, characterController.radius * wallCheckFactor, rayDirection, out rayhit, wallCheckDistance, climbableLayers))
				{
					StartClimbing(rayhit.normal);
					return true;
				}

				//Rotate the ray
				rayDirection = evenRotation * rayDirection;
			}
		}

		return false;
	}

	// ========================================================|
	//		--- Start Climbing ---
	//--------------------------------------------------------/
	//Begins a climb where newUp is the newUpdirection or the normal of the plane
	//Also changes friction value
	void StartClimbing(Vector3 newUp)
	{
		isClimbing = true;
		upDirection = newUp;
		floorNormal = newUp;
		friction = climbFriction;
	}
	// ========================================================|
	//		--- Resets Climbing ---
	//--------------------------------------------------------/
	//Resets the upDirection and floorNormal, and resets friction
	void StopClimbing()
	{
		isClimbing = false;
		upDirection = Vector3.up;
		floorNormal = upDirection;
		friction = floorFriction;
	}

	// ========================================================|
	//		--- RAGDOLL MOMENT ---
	//--------------------------------------------------------/
	//Disables character controller and enables physics when true
	//Enables character controller and disables physics when false
	//Preserves velocity between states
	public void SetRagdollState(bool state)
	{
		isRagdolled = state;

		characterController.enabled = !isRagdolled;
		
		if (!isRagdolled)
		{
			rigidbody.interpolation = RigidbodyInterpolation.None;
			velocity = rigidbody.velocity;
			//This is important to prevent the player from having incorrect gravity
			StopClimbing();
		}

		rigidbody.isKinematic = !isRagdolled;

		if (isRagdolled)
		{
			rigidbody.velocity = velocity;
			rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
		}
	}

	// ====================================================== //
	// =================== Input Checking =================== //
	// ====================================================== //

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

	public void OnRagdoll()
	{
		SetRagdollState(!isRagdolled);
	}

	// ##################################################################### //
	// ############################## STRUCTS ############################## //
	// ##################################################################### //

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