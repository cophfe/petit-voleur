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
	}

	public void UpdatePointUI()
	{
		UI.UpdatePointUI();
	}

	public void OnReachedPointThreshold()
	{
		canWin = true;
		UI.EnableNotifyText(true);
	}

	public void OnLeftPointThreshold()
	{
		canWin = false;
		UI.EnableNotifyText(false);
	}

	public void OnEnterStash()
	{
		if (canWin)
		{
			Debug.Log("pog");
			UI.OpenWinUI();
		}
	}

	public void OnToggleMenu()
	{
		UI.Pause(!UI.CheckIsPaused());
	}
}
