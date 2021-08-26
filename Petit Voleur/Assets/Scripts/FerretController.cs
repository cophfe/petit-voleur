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
	public float friction = 80;
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

	private CharacterController characterController;
	new private Rigidbody rigidbody;
	private Vector3 targetVelocity;
	private Vector3 frameAcceleration;
	private float accelerationScaled;

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
		rigidbody = GetComponent<Rigidbody>();
    }

    // FixedUpdate is called once per physics step
    void FixedUpdate()
    {
		//No need to run these if the player is ragdolled
		if (isRagdolled)
			return;
		
		//Reset these
		targetVelocity = Vector3.zero;
		frameAcceleration = Vector3.zero;
		accelerationScaled = acceleration;

		//Calculate floor normal and ground check
		RaycastHit rayhit;
		if (Physics.SphereCast(transform.position, characterController.radius * groundCheckRadiusFactor, -floorNormal, out rayhit, groundCheckDistance, groundCheckLayerMask))
		{
			//Don't consider surfaces that are too steep
			if (Vector3.Angle(Vector3.up, rayhit.normal) <= characterController.slopeLimit)
			{
				floorNormal = rayhit.normal;
				grounded = true;
				//CancelJump();
			}
			else
			{
				grounded = false;
			}
		}
		else
		{
			//Reset floor normal
			floorNormal = Vector3.up;
			grounded = false;
		}

		//Component of velocity that is parallel with the ground
		Vector3 planarVelocity = Vector3.ProjectOnPlane(velocity, floorNormal);
		//Project camera's forward vector on virtual plane that is the "floor"
		Vector3 forward = Vector3.ProjectOnPlane(Camera.main.transform.forward, floorNormal).normalized;

		if (input.x != 0 || input.y != 0)
		{
			// ~~~~~~~ Generate target velocity ~~~~~~ //
			//Forward component
			targetVelocity = forward * input.y;
			//Cross product the forward and global up to get the right component
			targetVelocity -= Vector3.Cross(forward, floorNormal) * input.x;
			targetVelocity *= targetSpeed;
			
			//Desired change in velocity
			frameAcceleration = targetVelocity - planarVelocity;
			
			//Give a boost if trying to go away from current velocity
			if (Vector3.Dot(frameAcceleration, planarVelocity) < 0)
			{
				accelerationScaled += friction;
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
		accelerationScaled *= Time.fixedDeltaTime;

		//Limit change in position to the player's acceleration in this frame
		if (frameAcceleration.sqrMagnitude > accelerationScaled * accelerationScaled)
		{
			frameAcceleration = frameAcceleration.normalized * accelerationScaled;
		}

		//Jump reset check
		if (isJumping)
		{
			if (velocity.y <= 0)
			{
				CancelJump();
			}
		}

		//GRAVITY
		frameAcceleration += Vector3.down * gravity * Time.fixedDeltaTime;
		//Add the acceleration calculated this frame to velocity
		velocity += frameAcceleration;
		
		//verlet integration
		characterController.Move(velocity * Time.fixedDeltaTime);

		//Rotation
		Vector3 newLookDirection = lookDirection;
		if (grounded && targetVelocity.sqrMagnitude > 0.01f)
		{
			newLookDirection = targetVelocity.normalized;
		}
		else
		{
			if (velocity.sqrMagnitude > 0.01f)
			{
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

		transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lookDirection, floorNormal), lookSpeed * Time.fixedDeltaTime);
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
		if (grounded)
		{
			gravity = jumpArc.GetGravity();
			velocity.y = jumpArc.GetJumpForce();
			isJumping = true;
		}
	}

	public void OnJumpRelease()
	{
		CancelJump();
	}

	public void OnDash()
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