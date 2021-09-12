/*==================================================
	Programmer: Connor Fettes
==================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
	[Tooltip("")]
	public float screenTransitionTime = 0.5f;
	[Tooltip("")]
	public RectTransform mainPanel = null;
	[Tooltip("")]
	public RectTransform optionsPanel = null;
	[Tooltip("The play scene's name.")]
	public string playSceneName = "Main";

	public Animator sceneTransitionAnimator;
	public float sceneTransitionTime = 1;
	
	enum ScreenState
	{
		TOPTIONSIN,
		TOPTIONSOUT,
		OPTIONS,
		MAINMENU
	}
	ScreenState state = ScreenState.MAINMENU;
	float mainMenuPanelDefaultHeight;
	float optionsPanelDefaultHeight; 
	float transitionTimer = 0;

	void Start()
	{
		optionsPanelDefaultHeight = optionsPanel.rect.height;
		mainMenuPanelDefaultHeight = mainPanel.rect.height;

		optionsPanel.gameObject.SetActive(false);
		mainPanel.gameObject.SetActive(true);
	}

	void Update()
	{
		switch (state)
		{
			case ScreenState.TOPTIONSIN:
				{
					if (transitionTimer >= screenTransitionTime)
					{
						state = ScreenState.OPTIONS;
						optionsPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, optionsPanelDefaultHeight);
						mainPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
						mainPanel.gameObject.SetActive(false);

						break;
					}

					float t = transitionTimer / screenTransitionTime;
					//transition panel height using ease out quad
					optionsPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (1 - (1 - t) * (1 - t)) * optionsPanelDefaultHeight);
					mainPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (1 - t) * (1 - t) * mainMenuPanelDefaultHeight);

					transitionTimer += Time.unscaledDeltaTime;
				}
				break;
			case ScreenState.TOPTIONSOUT:
				{
					if (transitionTimer >= screenTransitionTime)
					{
						state = ScreenState.MAINMENU;
						optionsPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
						mainPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, mainMenuPanelDefaultHeight);
						optionsPanel.gameObject.SetActive(false);
						break;
					}

					float t = transitionTimer / screenTransitionTime;
					//transition panel height using ease out quad
					optionsPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (1 - t) * optionsPanelDefaultHeight);
					mainPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, t * mainMenuPanelDefaultHeight);

					transitionTimer += Time.unscaledDeltaTime;
				}
				break;
		}
	}

	public void TransitionToPlay()
	{
		StartCoroutine(PlayGame());
	}

	IEnumerator PlayGame()
	{
		sceneTransitionAnimator.SetTrigger("Leave");

		yield return new WaitForSecondsRealtime(sceneTransitionTime);

		try
		{
			SceneManager.LoadScene(playSceneName);
		}
		catch
		{
			Debug.LogError("Could not load scene '" + playSceneName + "'.");
		}
	}

	public void OpenOptions()
	{
		transitionTimer = 0;
		state = ScreenState.TOPTIONSIN;

		mainPanel.gameObject.SetActive(true);
		optionsPanel.gameObject.SetActive(true);
	}

	public void CloseOptions()
	{
		transitionTimer = 0;
		state = ScreenState.TOPTIONSOUT;

		mainPanel.gameObject.SetActive(true);
	}

	IEnumerator ExitGame()
	{
		sceneTransitionAnimator.SetTrigger("Leave");

		yield return new WaitForSecondsRealtime(sceneTransitionTime);

		#if UNITY_EDITOR
		UnityEditor.EditorApplication.ExitPlaymode();
		#else
			Application.Quit();
		#endif
	}

	public void TransitionToExit()
	{
		StartCoroutine(ExitGame());
	}
}
