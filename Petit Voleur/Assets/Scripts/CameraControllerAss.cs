using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControllerAss : MonoBehaviour
{
	private Camera cam;
	public Transform target;
	public float height = 3.0f;
	public float speed;
	public float distance = 3;
	public Vector3 offset = Vector3.forward;
	public Vector2 delta;


	// Start is called before the first frame update
	void Start()
	{
		cam = Camera.main;
	}

	// Update is called once per frame
	void LateUpdate()
	{
		Quaternion rot = Quaternion.Euler(0, delta.x * speed * Time.deltaTime, 0);
		offset = rot * offset;

		cam.transform.position = target.position + offset * distance + Vector3.up * height;
		cam.transform.LookAt(target);
	}

	public void OnLook(InputValue value)
	{
		delta = value.Get<Vector2>();
	}
}
