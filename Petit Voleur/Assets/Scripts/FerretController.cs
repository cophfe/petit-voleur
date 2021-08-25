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
	public float gravity = 10;
	public float jumpForce = 5;
	public float acceleration = 50;
	public float targetSpeed = 30;
	public float friction = 80;
	public float frictionMultiplier = 2;
	public float airControl = 0.4f;
	public float maxSlopeFactor = 1.0f;
	public float minSlopeFactor = 0.0f;
	public float lookSpeed = 600f;

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

    // Update is called once per frame
    void FixedUpdate()
    {
		if (isRagdolled)
			return;
		
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
			}
			else
			{
				grounded = false;
			}
		}
		else
		{
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
				accelerationScaled = friction;
			}
		}
		else
		{
			if (grounded)
			{
				frameAcceleration = -velocity;
				accelerationScaled = friction;
			}
		}

		//Reduce control in the air
		if (!grounded)
			accelerationScaled *= airControl;
		
		//Multiply by slope ratio
		accelerationScaled *= Mathf.Lerp(maxSlopeFactor, minSlopeFactor, Vector3.Dot(frameAcceleration.normalized, Vector3.up));

		//Amount of acceleration in this frame
		accelerationScaled *= Time.fixedDeltaTime;

		//Limit change in position to the player's acceleration in this frame
		if (frameAcceleration.sqrMagnitude > accelerationScaled * accelerationScaled)
		{
			frameAcceleration = frameAcceleration.normalized * accelerationScaled;
		}

		//GRAVITY
		velocity += Vector3.down * gravity * Time.fixedDeltaTime;
		//Add velocity
		velocity += frameAcceleration;
		
		//LETS GOOOOO
		characterController.Move(velocity * Time.fixedDeltaTime);

		if (grounded && targetVelocity.sqrMagnitude > 0.01f)
		{
			lookDirection = targetVelocity.normalized;
		}
		else
		{
			if (velocity.sqrMagnitude > 0.1f)
				lookDirection = velocity.normalized;
			else
				lookDirection = forward;
		}

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

	public void OnMoveAxis(InputValue value)
	{
		input = value.Get<Vector2>();
	}

	public void OnJump()
	{
		if (grounded)
		{
			velocity += Vector3.up * jumpForce;
		}
	}

	public void OnDash()
	{
		isRagdolled = !isRagdolled;
		characterController.enabled = !isRagdolled;
		
		if (!isRagdolled)
		{
			velocity = rigidbody.velocity;
		}
		rigidbody.isKinematic = !isRagdolled;

		if (isRagdolled)
		{
			rigidbody.velocity = velocity;
		}
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawLine(transform.position, transform.position + floorNormal * 3);
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(transform.position, transform.position + frameAcceleration.normalized * 3);
		Gizmos.color = Color.green;
		Gizmos.DrawLine(transform.position, transform.position + velocity);
	}
}
