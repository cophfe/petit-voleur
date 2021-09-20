/*==================================================
	Programmer: Connor Fettes
==================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.Audio;

public class GameManager : MonoBehaviour
{
	public bool canLockCursor = true;
	public bool enableMenuToggle = true;

	GameUI UI = null;
	bool canWin = false;
	bool inStash = false;
	bool currentlyWinning = false;

	PlayerInput[] inputs;

	public AudioMixerSnapshot defaultSnapshot;
	public AudioMixerSnapshot deadSnapshot;
	public AudioMixerSnapshot winSnapshot;
	public AudioClip youWin;
	public AudioClip pointGet;
	public AudioSource audioSrc;
	public float snapshotTransitionTime;
	
	private void Start()
	{
		UI = FindObjectOfType<GameUI>();
		CursorLocked = true;
		
		//find the player inputs
		inputs = FindObjectsOfType<PlayerInput>(false);
	}

	/// <summary>
	/// updates point ui and plays point sound effect
	/// </summary>
	public void UpdatePointUI()
	{
		UI.UpdatePointUI();
		audioSrc.clip = pointGet;
		audioSrc.Play();
	}

	/// <summary>
	/// Lets the player win
	/// </summary>
	public void OnReachedPointThreshold()
	{
		canWin = true;
		if (inStash)
		{
			UI.EnableWinNotifyUI(true);
		}
		UI.GetNotifyAnimator().SetBool("CanWin", true);
	}

	/// <summary>
	/// Notifies player that they can win
	/// </summary>
	public void OnEnterStash()
	{
		inStash = true;
		if (canWin)
		{
			UI.EnableWinNotifyUI(true);
		}
	}

	/// <summary>
	/// Unnotifies the player that they can win
	/// </summary>
	public void OnExitStash()
	{
		inStash = false;
		UI.EnableWinNotifyUI(false);

	}

	/// <summary>
	/// Transitions to win state
	/// </summary>
	public void OnWin()
	{
		if (inStash && canWin && !currentlyWinning)
		{
			currentlyWinning = true;
			winSnapshot.TransitionTo(snapshotTransitionTime/2);
			StartCoroutine(YouWinPlayer());
		}
	}

	/// <summary>
	/// Wins the game
	/// </summary>
	IEnumerator YouWinPlayer()
	{
		GameInputEnabled = false;
		yield return new WaitForSeconds(0.125f);
		audioSrc.PlayOneShot(youWin);
		yield return new WaitForSecondsRealtime(youWin.length);
		UI.OpenWinUI();
	}

	/// <summary>
	/// Toggles menus
	/// </summary>
	public void OnToggleMenu()
	{
		if (enableMenuToggle)
		{
			//its not necessarily *good* that ui controls pause logic, but also it makes stuff easier soooooooo
			UI.ToggleMenu();
		}
	}

	/// <summary>
	/// cursor lock state
	/// </summary>
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

	/// <summary>
	/// game input enable state
	/// </summary>
	public bool GameInputEnabled
	{
		get
		{
			for (int i = 0; i < inputs.Length; i++)
			{
				if (inputs[i].gameObject != gameObject)
				{
					return inputs[i].enabled;
				}
			}

			return false;
		}
		set
		{
			for (int i = 0; i < inputs.Length; i++)
			{
				if (inputs[i].gameObject != gameObject)
				{
					inputs[i].enabled = value;
				}
			}
		}
	}

	/// <summary>
	/// Transitions to losing
	/// </summary>
	public void OnDeath()
	{
		UI.TransitionToLose();
		deadSnapshot.TransitionTo(snapshotTransitionTime);
	}

	private void OnDestroy()
	{
		defaultSnapshot.TransitionTo(0);
	}
}
