using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraInputSurface : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	CameraController cameraController;

	private void Start()
	{
		cameraController = FindObjectOfType<CameraController>();
		if (!cameraController)
		{
			enabled = false;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		Debug.Log("ENTER");
		cameraController.enableInput = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Debug.Log("EXIT");
		cameraController.enableInput = false;
	}
}
