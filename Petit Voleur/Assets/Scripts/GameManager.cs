/*==================================================
	Programmer: Connor Fettes
==================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public bool lockCursor = true;
	public bool enableMenuToggle = true;

	GameUI UI = null;
	bool canWin = false;
	
	private void Start()
	{
		UI = FindObjectOfType<GameUI>();
		if (lockCursor)
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
	}

	public void UpdatePointUI()
	{
		if (UI != null)
			UI.UpdatePointUI();
	}

	public void OnReachedPointThreshold()
	{
		canWin = true;

		if (UI != null)
			UI.EnableNotifyText(true);
	}

	public void OnLeftPointThreshold()
	{
		canWin = false;
		if (UI != null)
			UI.EnableNotifyText(false);
	}

	public void OnEnterStash()
	{
		if (canWin)
		{
			if (UI != null)
				UI.OpenWinUI();
		}
	}

	public void OnToggleMenu()
	{
		if (enableMenuToggle && UI != null)
		{
			//its not necessarily *good* that ui controls pause logic, but also it makes stuff easier soooooooo
			UI.ToggleMenu();
		}
	}

	public void OnDeath()
	{
		Debug.Log("PLAYER DIED!!");
		//Call UI
	}
}
