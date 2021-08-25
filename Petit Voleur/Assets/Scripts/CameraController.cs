using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public partial class CameraController : MonoBehaviour
{
	//INSPECTOR STUFF
	[Tooltip("The target to orbit around.")]
	public Transform target;
	[Tooltip("The maximum distance the camera can be away from the target.")]
	public float maxFollowDistance = 10;
	[Tooltip("The camera sensitivity multiplier.")]
	[Range(0, 1)] public float sensitivity = 0.1f;
	[Tooltip("If input is inverted or not.")]
	public bool inverted = false;
	[Tooltip("The layers that can obstruct the camera.")]
	public LayerMask obstructionLayers;
	[Tooltip("Camera movement speed.")]
	public float followSpeed = 15;
	[Tooltip("If rotation should be smoothed")]
	public bool smoothCameraRotation = false;
	[Tooltip("Camera orbit speed.")]
	[HideInInspector] public float rotateSpeed = 1;

	Camera cam;

	//When inspector values are changed, check they are valid
	void OnValidate()
	{
		if (maxFollowDistance < 0)
			maxFollowDistance = 0;
		if (rotateSpeed < 0)
			rotateSpeed = 0;
	}

	//The rotation of the targetQuaternion
	Vector2 rotation = Vector2.zero;
	//the offset from the target
	Vector3 orbitVector;

	Quaternion targetOrbit;
	Quaternion currentOrbit = Quaternion.identity;
	Vector3 currentPivotPosition;

	void Start()
    {
		//show error in console if target is null
		Debug.Assert(target != null);

		//set default values for orbit vector
		orbitVector = Vector3.forward * maxFollowDistance;
		currentPivotPosition = target.position;
		transform.forward = -Vector3.forward;

		cam = GetComponent<Camera>();
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

	}

	private void FixedUpdate()
	{
		float distance = Vector3.Distance(currentPivotPosition, target.position);
		currentPivotPosition = Vector3.MoveTowards(currentPivotPosition, target.position, Time.fixedDeltaTime * followSpeed * distance);
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
		//y rotation is clamped
		rotation.y = Mathf.Clamp(rotation.y, -88.0f, 88.0f);
		//set target quaternion
		targetOrbit = Quaternion.Euler(rotation.y, rotation.x, 0);
		
		if (!smoothCameraRotation)
		{
			currentOrbit = targetOrbit;
			orbitVector = currentOrbit * Vector3.forward;
			transform.forward = -orbitVector;
		}
	}

	//Sets the orbitVector's magnitude to the correct value (checks obstructions)
	void SetOrbitDistance()
	{
		orbitVector.Normalize();
		
		float yExtend = Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad) * cam.nearClipPlane;
		//regenerated every frame because it could change, could maybe optimise by only calling on camera change
		Vector3 boxExtents = new Vector3(yExtend * cam.aspect, yExtend, cam.nearClipPlane);

		//check if camera is obstructed
		if (Physics.BoxCast(((cam.nearClipPlane) / 2) * orbitVector + currentPivotPosition, boxExtents, orbitVector,
			out RaycastHit boxHit, transform.rotation, maxFollowDistance, obstructionLayers.value))
			orbitVector *= boxHit.distance;
		else
		{
			//if box cast doesn't detect it, it might still be obstructed
			//this should fix most cases of that happening
			//will not fix if raycast origin is inside of obstruction collider
			Ray ray = new Ray(currentPivotPosition, orbitVector);
			if (Physics.Raycast(ray, out RaycastHit rayHit, maxFollowDistance, obstructionLayers.value))
			{
				orbitVector *= rayHit.distance;
			}
			else
			{
				orbitVector *= maxFollowDistance;
			}
		}
		
		

		
	}
}
