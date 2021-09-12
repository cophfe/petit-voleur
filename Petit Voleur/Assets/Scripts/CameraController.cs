/*==================================================
	Programmer: Connor Fettes
==================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
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
	[Tooltip("The camera sensitivity multiplier.")]
	[Range(0, 1)] public float sensitivity = 0.1f;
	[Tooltip("If input is inverted or not.")]
	public bool inverted = false;
	[Tooltip("Y rotation cannot be higher than this value.")]
	[Range(-90, 90)] public float maximumUpRotation = 87;
	[Tooltip("Y rotation cannot be less than this value.")]
	[Range(-90, 90)] public float minimumUpRotation = -87;

	[Header("Movement")]
	[Space(5)]
	[Tooltip("Camera movement speed.")]
	public float followSpeed = 15;
	[Tooltip("The speed the camera zooms in. Setting this too low will cause clipping issues.")]
	public float zoomInSpeed = 15;
	[Tooltip("The speed the camera zooms out.")]
	public float zoomOutSpeed = 15;
	[Tooltip("Affects the amount of noise applied to the screen shake."), Range(0,1)]
	public float shakeNoiseMag = 0;
	[Tooltip("If rotation should be smoothed.")]
	public bool smoothCameraRotation = false;
	[Tooltip("Camera orbit speed.")]
	[HideInInspector] public float rotateSpeed = 1;
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

	Camera cam;

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
	//used for camera shake
	Vector3 shakeDir = Vector3.zero;
	float shakeMag = 0;
	float shakeTime = 0;
	float shakeTimer = 0;
	
	//used to stop camera from clipping into walls
	Vector3 cameraBoxExtents;
	public bool physicsShake = true;

	void Start()
	{
		//show error in console if target is null
		Debug.Assert(target != null);

		cam = GetComponent<Camera>();

		//set default values for camera
		rotation = new Vector2(180 + target.rotation.eulerAngles.y, -15);
		targetOrbit = Quaternion.Euler(rotation.y, rotation.x, 0);
		currentOrbit = targetOrbit;
		orbitVector = currentOrbit * Vector3.forward;
		currentPivotPosition = target.position;
		SetOrbitDistance();
		orbitVector = orbitVector.normalized * targetDistance;
		currentDistance = targetDistance;
		transform.position = currentPivotPosition + orbitVector;
		transform.forward = -orbitVector;
	}

	void LateUpdate()
	{
		//smoothed camera movement
		if (smoothCameraRotation)
		{
			float sphericalDistance = Quaternion.Angle(currentOrbit, targetOrbit);

			currentOrbit = Quaternion.RotateTowards(currentOrbit, targetOrbit, sphericalDistance * Time.deltaTime * rotateSpeed);
			//set the orbit vector 
			orbitVector = currentOrbit * Vector3.forward;
			//the camera will look in the opposite direction of the orbit vector, toward the target position
			transform.forward = -orbitVector;
		}

		SetOrbitDistance();
		//set camera position
		transform.position = currentPivotPosition + orbitVector;
		//camera shake
		if (shakeTimer > 0)
		{
			shakeTimer -= Time.deltaTime;
			float t = (shakeTimer) / shakeTime;
			Vector3 shakeVector = (shakeDir + t * shakeMag * shakeNoiseMag * Random.insideUnitSphere) * (shakeMag * t);
			float shakeMagnitude = shakeVector.magnitude;
			if (physicsShake && Physics.Raycast(new Ray(transform.position, shakeVector / shakeMagnitude), out var hit, shakeMagnitude, obstructionLayers.value))
			{
				shakeVector = Vector3.zero;
				//	float newDist = hit.distance - Vector3.Dot(transform.rotation * boxExtents, shakeVector / shakeMagnitude);
				//	if (newDist < 0)
				//	{
				//		shakeVector = shakeVector / shakeMagnitude * hit.distance;
				//	}
				//	else
				//	{
				//		shakeVector = shakeVector / shakeMagnitude * newDist;
				//	}
			}
			transform.position += shakeVector;
		}
	}

	private void Update()
	{
		float distance = Vector3.Distance(currentPivotPosition, target.position);
		currentPivotPosition = Vector3.MoveTowards(currentPivotPosition, target.position, Time.deltaTime * followSpeed * distance);
	}

	//Called through unity input system
	public void OnLook(InputValue value)
	{
		//get input from look axis
		Vector2 input = value.Get<Vector2>();
		//have to check invert everytime, it could change
		if (inverted)
			input *= -sensitivity;
		else
			input *= sensitivity;
		rotation += input;
		//up rotation is clamped
		rotation.y = Mathf.Clamp(rotation.y, -maximumUpRotation, -minimumUpRotation);
		//set target quaternion
		targetOrbit = Quaternion.Euler(rotation.y, rotation.x, 0);
		
		if (!smoothCameraRotation)
		{
			currentOrbit = targetOrbit;
			orbitVector = currentOrbit * Vector3.forward;
			transform.forward = -orbitVector;
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
	}

	//Sets the orbitVector's magnitude to the correct value (checks obstructions)
	//requires orbitVector to be normalised first
	void SetOrbitDistance()
	{		
		float yExtend = Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad) * cam.nearClipPlane;
		//regenerated every frame because it could change, could maybe optimise by only calling on camera change
		cameraBoxExtents = new Vector3(yExtend * cam.aspect, yExtend, cam.nearClipPlane) / 2;

		//check if camera is obstructed

		Collider[] c = Physics.OverlapBox(((cam.nearClipPlane) / 2) * orbitVector + currentPivotPosition, cameraBoxExtents, transform.rotation, obstructionLayers.value);

		//check if obstructing object is inside box used for raycast (if this is the case the raycast does not detect it)
		//if so, do not change target distance
		if (c.Length == 0)
		{
			if (Physics.BoxCast(((cam.nearClipPlane) / 2) * orbitVector + currentPivotPosition, cameraBoxExtents, orbitVector,
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
			currentDistance = Mathf.MoveTowards(currentDistance, targetDistance, Time.deltaTime * zoomInSpeed * (currentDistance - targetDistance));
		}
		orbitVector.Normalize();
		//now set orbit vec
		orbitVector *= currentDistance;
	}

	//
	public void SetCameraShake(Vector2 screenSpaceDirection, float magnitude, float time)
	{
		//shakeDir = Vector3.ProjectOnPlane(worldSpaceDirection, cam.transform.forward).normalized;
		shakeDir = screenSpaceDirection;
		shakeMag = magnitude;
		shakeTime = time;
		shakeTimer = shakeTime;
	}

	//public void OnAaa()
	//{
	//	SetCameraShake(target.forward, 3, 0.5f);
	//}
}
