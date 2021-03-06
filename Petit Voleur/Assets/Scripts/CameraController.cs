/*==================================================
	Programmer: Connor Fettes
==================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using TMPro;

[RequireComponent(typeof(Camera)),DefaultExecutionOrder(-1)]
public partial class CameraController : MonoBehaviour
{
	//INSPECTOR STUFF
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	[Header("Behaviour")]
	[Tooltip("The target to orbit around.")]
	public Transform target;
	[Tooltip("The maximum distance the camera can be away from the target.")]
	public float maxFollowDistance = 10;
	[Tooltip("The layers that can obstruct the camera.")]
	public LayerMask obstructionLayers;

	[Header("Control")]
	[Space(5)]
	[Tooltip("Whether the camera accepts input or not.")]
	public bool enableInput = true;
	[Tooltip("The camera sensitivity multiplier.")]
	[Range(0, 1)] public float sensitivity = 0.1f;
	[Tooltip("If input is inverted or not.")]
	public bool inverted = false;
	[Tooltip("Up rotation cannot be higher than this value.")]
	[Range(-90, 90)] public float maximumUpRotation = 87;
	[Tooltip("Up rotation cannot be less than this value.")]
	[Range(-90, 90)] public float minimumUpRotation = -87;
	[Tooltip("Up rotation cannot be higher than this value (when climbing).")]
	[Range(-90, 90)] public float maximumUpRotationClimbing = 87;
	[Tooltip("Up rotation cannot be less than this value (when climbing).")]
	[Range(-90, 90)] public float minimumUpRotationClimbing = -87;
	[Tooltip("The minimum angle off of the wall the camera can have when climbing.")]
	[Range(0, 10)] public float minAngleFromWallWhenClimbing = 0;
	[Tooltip("The percent of the screen the camera takes input from on android")]
	[Range(0,1)] public float androidInputScreenPercentage = 0.7f;

	[Header("Movement")]
	[Space(5)]
	[Tooltip("Camera movement speed.")]
	public float followSpeed = 15;
	[Tooltip("The max amount the camera will turn to look away from the floor.")]
	public float cameraAvoidFloorRotationPower = 15;
	[Tooltip("The angular distance in which the camera avoids the floor.")]
	public float cameraAvoidFloorRotationAngleRange = 30;
	[Tooltip("The speed the camera zooms out.")]
	public float zoomOutSpeed = 15;
	[Tooltip("Propertional to the speed camera shake deteriorates")]
	public float shakeDeteriorateSpeed = 1;
	[Tooltip("Affects the amount of random movement applied to the screen shake (multiplied by shake magnitude)."), Range(0,1)]
	public float shakeNoiseMag = 0;
	public float yOffsetStartDistance = 0;
	public float yOffsetDistance = 3;
	public float yOffsetMagnitude = 1;
	public float yOffsetChangeSpeed = 1;
	[Tooltip("If rotation should be smoothed.")]
	public bool smoothCameraRotation = false;
	
	[Tooltip("Camera orbit speed.")]
	[HideInInspector] public float rotateSpeed = 1;
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

	Camera cam;
	FerretController ferretController = null;

	//The rotation of the targetQuaternion
	Vector2 rotation;
	//the offset from the target
	Vector3 orbitVector;

	//used for smooth zoom
	float targetDistance;
	float currentDistance = 0;
	//used for smooth rotation
	Quaternion targetOrbit;
	Quaternion currentOrbit = Quaternion.identity;
	Vector3 currentPivotPosition;
	float xRotationAddition = 0;
	float xRotationAdditionCurrent = 0;
	float targetYOffset = 0;
	float yOffset = 0;

	//used to stop camera from clipping into walls
	Vector3 cameraBoxHalfExtents;
	
	//camera shake
	Vector3 cameraShake = Vector3.zero;

	private void OnEnable()
	{
		//hook into touch
		EnhancedTouchSupport.Enable();
		UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerMove += OnFingerLook;
	}

	private void OnDisable()
	{
		//unhook into touch
		if (EnhancedTouchSupport.enabled)
			EnhancedTouchSupport.Disable();
		UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerMove -= OnFingerLook;
	}

	void Start()
	{
		//show error in console if target is null
		Debug.Assert(target != null);

		cam = GetComponent<Camera>();
		ferretController = target.gameObject.GetComponentInChildren<FerretController>();

		//set default values for camera
		rotation = new Vector2(-15, 180 + target.rotation.eulerAngles.y);
		targetOrbit = Quaternion.Euler(rotation);
		currentOrbit = targetOrbit;
		orbitVector = currentOrbit * Vector3.forward;
		currentPivotPosition = target.position;
		SetOrbitDistance();
		orbitVector = orbitVector.normalized * targetDistance;
		currentDistance = targetDistance;
		transform.position = currentPivotPosition + orbitVector;
		transform.forward = -orbitVector;

		//set camera box extents, used for obstruction checking
		float yExtend = Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad) * cam.nearClipPlane;
		cameraBoxHalfExtents = new Vector3(yExtend * cam.aspect, yExtend, cam.nearClipPlane) / 2;
	}

	void LateUpdate()
	{
		//smoothed camera movement
		if (smoothCameraRotation)
		{
			float sphericalDistance = Quaternion.Angle(currentOrbit, targetOrbit);

			currentOrbit = Quaternion.RotateTowards(currentOrbit, targetOrbit, sphericalDistance * Time.deltaTime * rotateSpeed);
			xRotationAdditionCurrent = Mathf.MoveTowardsAngle(xRotationAdditionCurrent, xRotationAddition, Mathf.Abs(xRotationAdditionCurrent - xRotationAddition) * Time.deltaTime * rotateSpeed);
			//set the orbit vector 
			orbitVector = currentOrbit * Vector3.forward;
			//the camera will look in the opposite direction of the orbit vector, toward the target position
			transform.forward = -orbitVector;
			transform.Rotate(new Vector3(-xRotationAdditionCurrent, 0, 0));
		}

		SetOrbitDistance();
		//set camera position
		transform.position = currentPivotPosition + orbitVector;

		//add camera shake
		float m = UpdateCameraShake();
		if (m > 0.001f)
		{
			Vector3 shakeVector = cameraShake + cameraShake.magnitude * Random.insideUnitSphere * shakeNoiseMag;
			if (!Physics.BoxCast(transform.position, cameraBoxHalfExtents, shakeVector.normalized, transform.rotation, shakeVector.magnitude, obstructionLayers.value))
			{
				transform.position += shakeVector;
			}
		}

		//add y offset
		if (Physics.BoxCast(transform.position, cameraBoxHalfExtents, Vector3.up, out RaycastHit hit, transform.rotation, yOffset * yOffsetMagnitude, obstructionLayers.value))
		{
			transform.position += Vector3.up * hit.distance;

		}
		else
		{
			transform.position += Vector3.up * (yOffset * yOffsetMagnitude);
		}
	}

	private void Update()
	{
		float distance = Vector3.Distance(currentPivotPosition, target.position);
		currentPivotPosition = Vector3.MoveTowards(currentPivotPosition, target.position, Time.deltaTime * followSpeed * distance);
	}

	//Called through unity input system
	/// <summary>
	/// Updates input based on a 2d input vector
	/// </summary>
	/// <param name="value">Information describing the input event</param>
	public void OnLook(InputValue value)
	{
		Vector2 input = value.Get<Vector2>();
		//get input from look axis
		InputMove(input);
	}

	/// <summary>
	/// Updates the camera movement based on an input vector
	/// </summary>
	/// <param name="input">A 2d vector representing a rotational movement on the x and y axis</param>
	void InputMove(Vector2 input)
	{
		if (!enableInput) return;

		input *= inverted ? -sensitivity : sensitivity;
		rotation += new Vector2(input.y, input.x);

		//clamp x rotation
		rotation.y = Mathf.Repeat(rotation.y, 360);

		//if not climbing
		if (ferretController.upDirection == Vector3.up)
		{
			rotation.x = Mathf.Clamp(rotation.x, -maximumUpRotation, -minimumUpRotation);

			//this is used to push camera away from floor when on ground so you can see more upward
			float rotationFromMin = -(rotation.x - (minimumUpRotation));
			if (rotationFromMin < cameraAvoidFloorRotationAngleRange)
			{
				xRotationAddition = cameraAvoidFloorRotationPower * (1 - rotationFromMin / cameraAvoidFloorRotationAngleRange);
			}
			else
				xRotationAddition = 0;
		}
		else
		{
			xRotationAddition = 0;
			//cancel x rotation
			rotation.x = Mathf.Clamp(rotation.x, -maximumUpRotationClimbing, -minimumUpRotationClimbing);
			
			//clamp y rotation that is going into climb surface (when climbing)
			Vector3 vecRep = Quaternion.Euler(rotation) * Vector3.forward;
			//rotation needs to be clamped if negative
			if (Vector3.Dot(vecRep, ferretController.upDirection) < 0)
			{
				//set rotation to be on the closest value on the ring of clamp values 
				vecRep = Vector3.ProjectOnPlane(vecRep, ferretController.upDirection).normalized;
				//convert back to euler rotation
				var v = Quaternion.FromToRotation(Vector3.forward, vecRep).eulerAngles;

				rotation.y = v.y;
				rotation.x = v.x;

				//since the x clamping used does not account for the circularness of rotations, ill just use this to make it work
				if (rotation.x > 180)
				{
					rotation.x -= 360;
				}
			}
		}

		//set target quaternion
		targetOrbit = Quaternion.Euler(rotation);

		//do everything here if not smoothing rotation
		if (!smoothCameraRotation)
		{
			currentOrbit = targetOrbit;
			orbitVector = currentOrbit * Vector3.forward;
			transform.forward = -orbitVector;
			transform.Rotate(new Vector3(-xRotationAddition, 0, 0));
		}
	}

	/// <summary>
	/// Updates the input (in order to take into account whether the ferret is climbing or not)
	/// </summary>
	public void OnUpdateClimb()
	{
		InputMove(Vector2.zero);
	}

	/// <summary>
	/// Updates input based on finger movement
	/// </summary>
	/// <param name="finger">Information describing the input event</param>
	void OnFingerLook(Finger finger)
	{
		if (!enableInput) return;

		if (finger.currentTouch.phase == UnityEngine.InputSystem.TouchPhase.Moved)
		{

			if (finger.touchHistory.Count <= 1 || finger.touchHistory[0].screenPosition.x / Screen.width < 1 - androidInputScreenPercentage)
			{
				return; 
			}

			Vector2 input = finger.currentTouch.screenPosition - finger.touchHistory[1].screenPosition;

			InputMove(2 * input);
		}
		
	}

	//When inspector values are changed, check they are valid
	void OnValidate()
	{
		if (maxFollowDistance < 0)
			maxFollowDistance = 0;
		if (rotateSpeed < 0)
			rotateSpeed = 0;
		if (maximumUpRotation < minimumUpRotation)
		{
			maximumUpRotation = minimumUpRotation;
		}
		if (maximumUpRotationClimbing < minimumUpRotationClimbing)
		{
			maximumUpRotation = minimumUpRotation;
		}
	}

	//requires orbitVector to be normalised first
	/// <summary>
	/// Sets the orbitVector's magnitude to the correct value, taking into account obstructions
	/// </summary>
	void SetOrbitDistance()
	{		
		

		//check if camera is obstructed

		Collider[] c = Physics.OverlapBox(((cam.nearClipPlane) / 2) * orbitVector + currentPivotPosition, cameraBoxHalfExtents, transform.rotation, obstructionLayers.value);

		//check if obstructing object is inside box used for raycast (if this is the case the raycast does not detect it)
		//if so, do not change target distance
		if (c.Length == 0)
		{
			if (Physics.BoxCast(((cam.nearClipPlane) / 2) * orbitVector + currentPivotPosition, cameraBoxHalfExtents, orbitVector,
			out RaycastHit boxHit, transform.rotation, maxFollowDistance, obstructionLayers.value))
			{
				targetDistance = boxHit.distance;
			}
			else
			{
				//if box cast doesn't detect it, it might still be obstructed
				//this should fix most cases of that happening
				//will not fix if raycast origin is inside of obstruction collider
				Ray ray = new Ray(currentPivotPosition, orbitVector);
				if (Physics.Raycast(ray, out RaycastHit rayHit, maxFollowDistance, obstructionLayers.value))
				{
					targetDistance = rayHit.distance;
				}
				else
				{
					targetDistance = maxFollowDistance;
				}

			}
		}

		//magnitude changes differently depending if zooming in or out
		//need to move currentdistance toward targetdistance
		if (currentDistance < targetDistance)
		{
			currentDistance = Mathf.MoveTowards(currentDistance, targetDistance, Time.deltaTime * zoomOutSpeed * (targetDistance - currentDistance));
		}
		else
		{
			currentDistance = targetDistance;
		}
		orbitVector.Normalize();
		//now set orbit vec
		orbitVector *= currentDistance;

		//now set y offset based on distance
		if (currentDistance < yOffsetStartDistance + yOffsetDistance)
		{
			float t = 1 - Mathf.Max(0, currentDistance - yOffsetStartDistance) / yOffsetDistance;
			targetYOffset = t;
		}
		else
		{
			targetYOffset = 0;
		}

		yOffset = Mathf.MoveTowards(yOffset, targetYOffset, Time.deltaTime * yOffsetChangeSpeed * Mathf.Abs(targetYOffset - yOffset));
	}

	/// <summary>
	/// Adds a vector to the camera shake vector
	/// </summary>
	/// <param name="shake">A world space displacement for the camera</param>
	public void AddCameraShake(Vector3 shake)
	{
		cameraShake += shake;
	}

	/// <summary>
	/// updates the camera shake vector using delta time 
	/// </summary>
	/// <returns>returns the magnitude of camera shake</returns>
	float UpdateCameraShake()
	{
		float magnitude = cameraShake.magnitude;
		cameraShake = Vector3.MoveTowards(cameraShake, Vector3.zero, magnitude * Time.unscaledDeltaTime * shakeDeteriorateSpeed);
		return magnitude;
	}
}
