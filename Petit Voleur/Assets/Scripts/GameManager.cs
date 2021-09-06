/*==================================================
	Programmer: Connor Fettes
==================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	GameUI UI = null;
	bool canWin = false;

	private void Start()
	{
		UI = FindObjectOfType<GameUI>();
		Cursor.lockState = CursorLockMode.Confined;
		Cursor.visible = false;
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
		if (UI != null)
			UI.Pause(!UI.CheckIsPaused());
	}
}
