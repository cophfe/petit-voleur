/*===================================================================
		  Created by Rahul J
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
	public bool inputEnabled = true;
	public Vector3 velocity;
	public FerretStats stats = new FerretStats();
	public float speed;
	public float gravity = 10;
	private float friction = 80;
	public bool grounded = false;
	public bool isJumping = false;
	public bool isDashing = false;
	public bool isClimbing = false;
	public bool isRagdolled = false;
	public Vector3 upDirection = Vector3.up;
	private Vector3 floorNormal = Vector3.up;
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
	public float maxVelocity = 300.0f;

	[Header("Jumping")]
	public JumpArc fallingArc;
	public JumpArc jumpArc;

	[Header("Wall Climbing")]
	public LayerMask climbableLayers;
	public float climbFriction = 140;
	public float wallCheckDistance = 0.12f;
	public float wallCheckFactor = 0.9f;
	public int wallCheckAngles = 4;

	[Header("Dashing and Impact")]
	public float dashSpeed = 40.0f;
	public float dashImpactMaxAngle = 20.0f;
	public Vector3 dashImpactBox = Vector3.one;
	public float dashImpactForce = 100.0f;
	public float dashImpactShake = 1.0f;
	public LayerMask dashImpactLayers;
	public float dashRecoil = 3.0f;
	public float dashRecoilShake = 2.0f;
	public float dashRecoilRagdollDuration = 2.0f;
	public LayerMask dashRagdollLayers;
	public AnimationCurve dashSpeedCurve;
	public float dashDuration = 1.0f;
	public float dashCooldown = 1.4f;
	public float dashImpactSlowdown = 0.0f;
	public float dashImpactSlowdownDuration = 0.05f;
	public float defaultImpactMultiplier = 0.5f;

	[HideInInspector]
	public FerretHealth health;
	private CharacterController characterController;
	[HideInInspector]
	new public Rigidbody rigidbody;
	[HideInInspector]
	public FerretAudio ferretAudio;
	private FerretPickup ferretPickup;
	private TimeManager timeManager;
	private CameraController cameraController;
	private Vector3 forward;
	private Vector3 projectedInput;
	private Vector3 targetVelocity;
	private Vector3 deltaVelocity;
	private Vector3 dashVelocity;
	private float accelerationThisFrame;
	//Dash timers
	private float dashTimer = 0f;
	private float dashCDTimer = 0f;
	private float ragdollTimer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
		rigidbody = GetComponent<Rigidbody>();
		ferretPickup = GetComponent<FerretPickup>();
		health = GetComponent<FerretHealth>();
		ferretAudio = GetComponent<FerretAudio>();
		timeManager = FindObjectOfType<TimeManager>();
		cameraController = FindObjectOfType<CameraController>();
		StopClimbing();
		stats.Reset();
    }

	//Called every frame
    void Update()
    {
		stats.Update();
		if (isRagdolled)
		{
			if (ragdollTimer > 0)
				ragdollTimer -= Time.deltaTime;
			else
				CancelRagdoll();
		}
		else
		{
			//Decrement dash timer and cancel if its over
			if (isDashing)
			{
				if (dashTimer > 0)
					dashTimer -= Time.deltaTime;
				else
					CancelDash();
			}

			Move();
			
			DoRotation();

			//Decrement dash timer when on the ground
			if (!isDashing && dashCDTimer > 0)
				dashCDTimer -= Time.deltaTime * stats.DashFrequency;

			//Player deaaad
			if (health.CurrentHealth <= 0)
			{
				StartRagdoll(999);
				ferretAudio.PlayFerretDead();
			}
		}
    }

	// ========================================================|
	//		--- Collision Response ---
	//--------------------------------------------------------/
	//Solves velocity modification when hitting objects and pushes other physics objects around
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

		if (isDashing)
		{
			//Call dash impact if the angle between the dash velocity and inverse of the normal is small enough
			if (Vector3.Angle(-hit.normal, dashVelocity.normalized) < dashImpactMaxAngle)
			{
				bool ragdoll = ((1 << hit.gameObject.layer) & dashRagdollLayers.value) > 0;
				DashImpact(hit.point, dashVelocity.normalized, ragdoll);
				
				if (ragdoll)
					ferretAudio.PlayWallImpact();
				else
					ferretAudio.PlayItemImpact();
			}
		}
		else
		{
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
	}

	// ========================================================|
	//		--- Main Movement Loop ---
	//--------------------------------------------------------/
	void Move()
	{
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

			if (!grounded)
			{
				grounded = true;
				ferretAudio.PlayFerretLanded();
			}
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

		//GRAVITY
		//Happens before everything else so that slope calculations can include gravity and resolve it BEFORE rotation happens
		velocity -= upDirection * gravity * Time.deltaTime;

		//  Component of velocity that is parallel with the ground  //
		//Necessary to ensure friction doesn't affect "vertical" component of velocity
		Vector3 planarVelocity = Vector3.ProjectOnPlane(velocity, floorNormal);

		// ====================================================== //
		// === //  Calculating Desired Change in Velocity  // === //
		// ====================================================== //
			//Desired change in velocity is defined by the difference between how fast we are going and how fast we want to go.
			//By using this desired change in velocity every frame, but limited by the max acceleration per frame we can ensure
			//that constant acceleration is maintained, and the velocity never overshoots
		
		//Override input if control is blocked
		if (!inputEnabled)
			input = Vector2.zero;
		//Input
		if (input.sqrMagnitude > 0)
		{
			// ~~~~~~~ Generate target velocity ~~~~~~ //
			projectedInput = forward * input.y;									//Forward component
			projectedInput -= Vector3.Cross(forward, floorNormal) * input.x;	//Cross product the forward and floorNormal to get the left component
			targetVelocity = projectedInput * (targetSpeed * stats.Speed);						//Multiply by target speed to set the magnitude of targetVelocity
			
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

		//Multiply acceleration by speed to keep movement proportionate
		//Finally limit acceleration value to be for this frame
		accelerationThisFrame *= stats.Speed * Time.deltaTime;

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

		//Override velocity when dashing. Multiplies by the curve based on time
		if (isDashing)
		{
			velocity = dashVelocity * Mathf.Clamp01(dashSpeedCurve.Evaluate(1 - (dashTimer / dashDuration)));
		}

		
		//Hard limit to totalVelocity
		if (velocity.sqrMagnitude > maxVelocity * maxVelocity)
		{
			velocity = velocity.normalized * maxVelocity;
		}
		
		//lets goooooooooooooooooooooo
		characterController.Move(velocity * Time.deltaTime);
		
		speed = velocity.magnitude;
		if (speed < 0.0001)
			speed = 0;
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

		//Override direction if dashing
		if (isDashing)
			lookDirection = dashVelocity.normalized;

		transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lookDirection, floorNormal), lookSpeed * Time.deltaTime);
	}

	// ========================================================|
	//		--- Add Velocity ---
	//--------------------------------------------------------/
	public void AddVelocity(Vector3 addedAcceleration)
	{
		velocity += addedAcceleration;
	}

	// ========================================================|
	//		--- Start Dash ---
	//--------------------------------------------------------/
	//Starts a dash if the dash cooldown is done
	void StartDash()
	{
		if (!isDashing)
		{
			if (dashCDTimer <= 0)
			{
				//Default to lookDirection
				Vector3 dashDirection = Vector3.ProjectOnPlane(lookDirection, floorNormal).normalized;
				//Use projected input if possible
				if (input.sqrMagnitude > 0)
					dashDirection = projectedInput;

				dashVelocity = dashDirection * dashSpeed;
				dashTimer = dashDuration;
				isDashing = true;

				ferretAudio.PlayFerretDash();
			}
		}
	}

	// ========================================================|
	//		--- Cancel Dash ---
	//--------------------------------------------------------/
	//Resets dash state and starts cooldown
	void CancelDash()
	{
		isDashing = false;
		dashTimer = 0;
		dashCDTimer = dashCooldown;
	}

	// ========================================================|
	//		--- Dash has hit a wall ---
	//--------------------------------------------------------/
	//Cancels dash and adds an impulse to nearby rigidbodies
	void DashImpact(Vector3 point, Vector3 impulseDirection, bool ragdoll = false)
	{
		//Recoil player from impact
		velocity = -impulseDirection * dashRecoil;

		//Ragdoll player
		if (ragdoll)
		{
			StartRagdoll(dashRecoilRagdollDuration);
			cameraController.SetCameraShake(Vector2.up, dashRecoil, 3);
		}
		else
		{
			timeManager.StartTimeModifier(dashImpactSlowdown, dashImpactSlowdownDuration, 0f);
			cameraController.SetCameraShake(Vector2.up, dashImpactShake, 3);
		}

		//Get all colliders in the impact area
		Collider[] results = Physics.OverlapBox(point, dashImpactBox, Quaternion.LookRotation(impulseDirection, floorNormal), dashImpactLayers);

		//loop through all colliders and add forces to the ones with rigidbodies
		for (int i = 0; i < results.Length; ++i)
		{
			if (results[i].attachedRigidbody)
			{
				results[i].attachedRigidbody.velocity = impulseDirection * (dashImpactForce * stats.DashPower) / results[i].attachedRigidbody.mass;
			}
		}

		//End dash
		CancelDash();

	}

	// ========================================================|
	//		--- Reset Dash ---
	//--------------------------------------------------------/
	//Simply clears the dash cooldown
	void ResetDashCooldown()
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
		velocity += upDirection * jumpArc.GetJumpForce() * stats.Jump;
		isJumping = true;

		ferretAudio.PlayFerretJump();
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
	void SetRagdollState(bool state)
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

	// ========================================================|
	//		--- START RAGDOLL ---
	//--------------------------------------------------------/
	public void StartRagdoll(float time)
	{
		if (isRagdolled)
			return;
		
		ragdollTimer = time;
		SetRagdollState(true);
	}

	// ========================================================|
	//		--- CANCEL RAGDOLL ---
	//--------------------------------------------------------/
	public void CancelRagdoll()
	{
		ragdollTimer = 0.0f;
		SetRagdollState(false);
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
		StartDash();
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

	[System.Serializable]
	public class FerretStats
	{
		[SerializeField]
		private float[] stats = new float[4];
		private float[] statTimers = new float[4];

		//Getters
		public float Speed
		{
			get
			{
				return stats[(int)Stat.Speed];
			}
		}

		public float Jump
		{
			get
			{
				return stats[(int)Stat.Jump];
			}
		}

		public float DashPower
		{
			get
			{
				return stats[(int)Stat.DashPower];
			}
		}

		public float DashFrequency
		{
			get
			{
				return stats[(int)Stat.DashFrequency];
			}
		}

		//Revert to all initial values and reset timers
		public void Reset()
		{
			for (int i = 0; i < 4; ++i)
			{
				stats[i] = 1.0f;
				statTimers[i] = 0.0f;
			}
		}

		//Decrement all stats and reset if timer is complete
		public void Update()
		{
			for (int i = 0; i < 4; ++i)
			{
				if (statTimers[i] > 0)
				{
					statTimers[i] -= Time.deltaTime;

					if (statTimers[i] <= 0)
						stats[i] = 1.0f;
				}
			}
		}

		//Return a stat by enum value
		public float GetStat(Stat stat)
		{
			return stats[(int)stat];
		}
		
		//Sets a stat value for *duration* seconds
		public void SetStat(Stat stat, float newValue, float duration)
		{
			stats[(int)stat] = newValue;
			statTimers[(int)stat] = duration;
		}

		//Base enum for reference
		[System.Serializable]
		public enum Stat : int
		{
			Speed,
			Jump,
			DashPower,
			DashFrequency
		}
	}
}