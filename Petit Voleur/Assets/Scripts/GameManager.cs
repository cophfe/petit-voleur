/*==================================================
	Programmer: Connor Fettes
==================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
	public bool canLockCursor = true;
	public bool enableMenuToggle = true;

	GameUI UI = null;
	bool canWin = false;

	PlayerInput playerInput;
	PlayerInput cameraInput;

	private void Start()
	{
		UI = FindObjectOfType<GameUI>();
		CursorLocked = true;

		//find the player's input
		try
		{
			playerInput = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<PlayerInput>();
			cameraInput = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerInput>();
		}
		catch
		{
			Debug.LogWarning("Error in finding input components.");
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
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				UI.OpenWinUI();
			}
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

	public bool CursorLocked
	{
		get
		{
			return Cursor.lockState == CursorLockMode.Locked;
		}
		set
		{
			if (value && canLockCursor) 
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
			else
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}
	}

	public bool GameInputEnabled
	{
		get
		{
			return playerInput.enabled && cameraInput.enabled;
		}
		set
		{
			playerInput.enabled = value;
			cameraInput.enabled = value;
		}
	}

	public void OnDeath()
	{
		Debug.Log("PLAYER DIED!!");
		//Call UI
	}
}
