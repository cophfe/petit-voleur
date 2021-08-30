using UnityEngine;
using UnityEditor;

//Very simple editor script
//hides rotate speed when smoothCameraRotation is false

[CustomEditor(typeof(CameraController))]
public class CameraControllerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		CameraController cameraController = (CameraController)target;

		if (cameraController.smoothCameraRotation)
		{
			cameraController.rotateSpeed = EditorGUILayout.FloatField("Rotate Speed:", cameraController.rotateSpeed);
		}
	}
}

