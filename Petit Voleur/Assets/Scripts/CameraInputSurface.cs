using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraInputSurface : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	CameraController cameraController;
	int inputCount = 0;

	private void Start()
	{
		cameraController = FindObjectOfType<CameraController>();
		if (!cameraController)
		{
			enabled = false;
		}
		else
			cameraController.enableInput = true;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		cameraController.enableInput = true;
		inputCount++;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		inputCount = Mathf.Max(inputCount-1, 0);
		if (inputCount > 0)
			cameraController.enableInput = false;

	}

	private void OnApplicationFocus(bool focus)
	{
		inputCount = 0;
	}
}
