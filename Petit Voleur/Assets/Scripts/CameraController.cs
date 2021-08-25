using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
	[Tooltip("The target to orbit around.")]
	[SerializeField] Transform target;
	[Tooltip("The maximum distance the camera can be away from the target.")]
	[SerializeField] float maxFollowDistance = 10;
	[Tooltip("The camera sensitivity multiplier.")]
	[SerializeField] float sensitivity = 1;
	[Tooltip("If input is inverted or not.")]
	[SerializeField] bool inverted = false;
	[Tooltip("The minimum distance the camera can be to colliders.")]
	[SerializeField] float skin = 0.1f;
	[Tooltip("The layers that can obstruct the camera.")]
	[SerializeField] LayerMask obstructionLayers;
	//[Tooltip("Camera movement speed.")]
	//[SerializeField] float followSpeed = 1;
	[Tooltip("Camera orbit speed.")]
	[SerializeField] float rotateSpeed = 1;
	
	//The rotation of the targetQuaternion
	Vector2 rotation = Vector2.zero;
	//the offset from the target
	Vector3 orbitVector;

	Quaternion targetQuaternion;
	Quaternion currentQuaternion = Quaternion.identity;

	void Start()
    {
		//set default values for orbit vector
		orbitVector = Vector3.forward * maxFollowDistance;
		transform.position = target.position + orbitVector;
		transform.forward = -Vector3.forward;

		//should be in game manager
		Cursor.lockState = CursorLockMode.Confined;
		Cursor.visible = false;
	}

    void LateUpdate()
    {
		//this is teeechnically an incorrect use of interpolation, but idk the alternitive for quaternions
		//it's also not framerate independant
		currentQuaternion = Quaternion.Slerp(currentQuaternion, targetQuaternion, Time.deltaTime * rotateSpeed);
		
		//currentQuaternion = Quaternion.RotateTowards(currentQuaternion, targetQuaternion, Time.deltaTime * rotateSpeed);

			//set the orbit vector 
		orbitVector = currentQuaternion * Vector3.forward;
		//the camera will look in the opposite direction of the orbit vector, toward the target position
		transform.forward = -orbitVector;
		
		SetOrbitDistance();
		//set camera position
		transform.position = target.position + orbitVector;
	}

	public void OnLookAxis(InputValue value)
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
		targetQuaternion = Quaternion.Euler(rotation.y, rotation.x, 0);	
	}

	//Sets the orbitVector's magnitude to the correct value (checks obstructions)
	//requires orbitVector to be normalised first
	void SetOrbitDistance()
	{
		//check if camera is obstructed
		//sphere cast ensures the camera doesn't get too close to nuthin
		Ray ray = new Ray(target.position, orbitVector);
		if (Physics.SphereCast(ray, skin, out RaycastHit hit, maxFollowDistance, obstructionLayers.value))
			orbitVector *= hit.distance;
		else
			orbitVector *= maxFollowDistance;
	}
}
