﻿/*==================================================
	Programmer: Connor Fettes
==================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//~~~~~~~~~~~~~~MENU STUFF~~~~~~~~~~~~~~~~~~
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//public variables
	[Header("Menu Variables ")]
	[Tooltip("The screen overlay menus use.")]
	public Image screenOverlay = null;
	[Tooltip("The pause menu panel.")]
	public RectTransform pausePanel = null;
	[Tooltip("The win menu panel.")]
	public RectTransform winPanel = null;
	[Tooltip("The options menu panel.")]
	public RectTransform optionsPanel = null;
	[Tooltip("The points notification text.")]
	public TextMeshProUGUI notifyText = null;
	[Tooltip("The points progress bar.")]
	public Image completionBar = null;
	[Tooltip("The time it takes to transition between menus.")]
	public float screenTransitionTime = 0.5f;
	[Tooltip("The main menu scene's name.")]
	public string menuSceneName = "Menu";
	[Tooltip("The scene transition animator.")]
	public Animator sceneTransitionAnimator;
	[Tooltip("The amount of time it takes to transition to a new scene")]
	public float sceneTransitionTime = 1;
	//private variables
	enum ScreenState
	{
		TPAUSEIN,
		TPAUSEOUT,
		PAUSE,
		TWININ,
		TWINOUT,
		WIN,
		TOPTIONSIN,
		TOPTIONSOUT,
		OPTIONS,
		NOTHING
	}
	//default heights for panels (used for transitions)
	float pausePanelDefaultHeight;
	float winPanelDefaultHeight;
	float optionsPanelDefaultHeight;
	//the initial alpha of the screen overlay
	float overlayDefaultAlpha;
	//the current screen state
	ScreenState screenState = ScreenState.NOTHING;
	//the timer used for transitioning menus
	float screenTransitionTimer = 0;
	//the time scale before pausing
	float lastTimeScale = 1;
	//The player's input component
	PlayerInput playerInput = null;
	//The camera's input component
	PlayerInput cameraInput = null;

	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//~~~~~~~~~~~~~POINTS STUFF~~~~~~~~~~~~~~~~~
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

	//public variables
	[Header("Points Variables")]
	[Space(5)]
	[Tooltip("The textmeshpro component that shows the point value.")]
	public TextMeshProUGUI pointValueText = null;
	[Tooltip("The time it takes for the text to transition to a new value.")]
	public float pointTransitionTime = 0.4f;
	[Tooltip("Affects the change in font size when transitioning.")]
	public float pointBounceMagnitude = 1;
	
	//private variables
	float pointTransitionTimer = 0;
	//the initial font size of the tmp component
	float defaultFontSize;
	//whether the point value is transitioning or not
	bool pointsTransitioning = false;
	//the component that controls point values
	PointTracker pointTracker;

	//this value is used for the easing function
	int startPointValue = 0;

	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//~~~~~~~~~~~~~HEALTH STUFF~~~~~~~~~~~~~~~~~
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

	//public variables
	[Header("Health Variables")]
	[Space(5)]
	[Tooltip("")]
	public GameObject healthPrefab = null;
	[Tooltip("")]
	public RectTransform healthParent = null;

	//private variables
	List<GameObject> healthChildren = null;
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

	private void Start()
	{
		//initialise menu values
		pausePanelDefaultHeight = pausePanel.rect.height;
		winPanelDefaultHeight = winPanel.rect.height;
		optionsPanelDefaultHeight = optionsPanel.rect.height;
		overlayDefaultAlpha = screenOverlay.color.a;
		EnableScreen(ScreenState.NOTHING);

		//initialise point values
		defaultFontSize = pointValueText.fontSize;

		//find the point tracker
		pointTracker = FindObjectOfType<PointTracker>();

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
	
	private void Update()
	{
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		//~~~~~~~~~~~~MENU TRANSITION~~~~~~~~~~~~~~~
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		switch (screenState)
		{
			case ScreenState.TPAUSEIN:
				{
					//end transition
					if (screenTransitionTimer >= screenTransitionTime)
					{
						screenState = ScreenState.PAUSE;
						pausePanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pausePanelDefaultHeight);
						Color finalColor = screenOverlay.color;
						finalColor.a = overlayDefaultAlpha;
						screenOverlay.color = finalColor;
						break;
					}

					float t = screenTransitionTimer / screenTransitionTime;
					//transition panel height using ease out quad
					pausePanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (1 - (1 - t) * (1 - t)) * pausePanelDefaultHeight);
					//transition overlay alpha linearly
					Color c = screenOverlay.color;
					c.a = overlayDefaultAlpha * t;
					screenOverlay.color = c;

					screenTransitionTimer += Time.unscaledDeltaTime;
				}
				break;
			case ScreenState.TPAUSEOUT:
				{
					//end transition
					if (screenTransitionTimer >= screenTransitionTime)
					{
						screenState = ScreenState.NOTHING;

						//set final values
						pausePanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
						var color = screenOverlay.color;
						color.a = 0;
						screenOverlay.color = color;
						Time.timeScale = lastTimeScale;

						EnableScreen(ScreenState.NOTHING);

						//enable input from player
						if (playerInput)
							playerInput.enabled = true;

						if (cameraInput)
							cameraInput.enabled = true;

						break;
					}

					float t = 1 - screenTransitionTimer / screenTransitionTime;
					//transition panel height using ease in quad
					pausePanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (t * t) * pausePanelDefaultHeight);
					//transition overlay alpha linearly
					Color c = screenOverlay.color;
					c.a = overlayDefaultAlpha * t;
					screenOverlay.color = c;

					screenTransitionTimer += Time.unscaledDeltaTime;
				}
				break;
			case ScreenState.TWININ:
				{
					//end transition
					if (screenTransitionTimer >= screenTransitionTime)
					{
						screenState = ScreenState.WIN;
						winPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, winPanelDefaultHeight);
						Color finalColor = screenOverlay.color;
						finalColor.a = overlayDefaultAlpha;
						screenOverlay.color = finalColor;
						break;
					}

					float t = screenTransitionTimer / screenTransitionTime;
					//transition panel height using ease out quad
					winPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (1 - (1 - t) * (1 - t)) * winPanelDefaultHeight);
					//transition overlay alpha linearly
					Color c = screenOverlay.color;
					c.a = overlayDefaultAlpha * t;
					screenOverlay.color = c;

					screenTransitionTimer += Time.unscaledDeltaTime;
				}
				break;
			case ScreenState.TOPTIONSIN:
				{
					//end transition
					if (screenTransitionTimer >= screenTransitionTime)
					{
						screenState = ScreenState.OPTIONS;
						optionsPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, optionsPanelDefaultHeight);
						pausePanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
						EnableScreen(ScreenState.OPTIONS);
						break;
					}

					float t = screenTransitionTimer / screenTransitionTime;
					//transition panel height using ease out quad
					optionsPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (1 - (1 - t) * (1 - t)) * optionsPanelDefaultHeight);
					pausePanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (1 - t) * (1 - t) * pausePanelDefaultHeight);
					
					screenTransitionTimer += Time.unscaledDeltaTime;
				}
				break;
			case ScreenState.TOPTIONSOUT:
				{
					//end transition
					if (screenTransitionTimer >= screenTransitionTime)
					{
						screenState = ScreenState.PAUSE;
						optionsPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
						pausePanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pausePanelDefaultHeight);
						EnableScreen(ScreenState.PAUSE);
						break;
					}

					float t = screenTransitionTimer / screenTransitionTime;
					//transition panel height using ease out quad
					optionsPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (1 - t) * optionsPanelDefaultHeight);
					pausePanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, t * pausePanelDefaultHeight);


					screenTransitionTimer += Time.unscaledDeltaTime;
				}
				break;
			}
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		//~~~~~~~~~~~~POINTS TRANSITION~~~~~~~~~~~~~
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		if (pointsTransitioning)
		{
			//it is easier if transitions from 0 to 1
			float t = pointTransitionTimer / pointTransitionTime;
			//set font scale using half circle function
			//w is width, r is height, min x is at 0,0
			//y = sqrt(r^2 - ((2rx)/w - r)^2)
			completionBar.fillAmount = (startPointValue + (pointTracker.GetPoints() - startPointValue) * t) / pointTracker.GetMaxLimit();

			float scaleTransformedX = (2 * pointBounceMagnitude * t - pointBounceMagnitude);
			float scaleAddition = Mathf.Sqrt(pointBounceMagnitude * pointBounceMagnitude - scaleTransformedX * scaleTransformedX);

			if (startPointValue < pointTracker.GetPoints())
				pointValueText.fontSize = defaultFontSize * (1 + scaleAddition);
			else
				//font size decrease is max 25%, otherwise it looks bad
				pointValueText.fontSize = defaultFontSize * (pointBounceMagnitude - scaleAddition* 0.25f) / pointBounceMagnitude;

			//set text value (linear, finishes 1/3 of the way through)
			if (pointTransitionTimer <= pointTransitionTime / 3)
			{
				int currentPointValue = startPointValue + (int)((pointTracker.GetPoints() - startPointValue) * t * 3);
				pointValueText.text = currentPointValue.ToString();
			}
			else
			{
				pointValueText.text = pointTracker.GetPoints().ToString();
			}

			//end transition at transitionTime
			if (pointTransitionTimer >= pointTransitionTime)
			{
				pointsTransitioning = false;
				pointValueText.fontSize = defaultFontSize;
				completionBar.fillAmount = (float)pointTracker.GetPoints()/ pointTracker.GetMaxLimit();
			}

			pointTransitionTimer += Time.deltaTime;
		}
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	}

	public void UpdatePointUI()
	{
		try
		{
			//points starts halfway done
			startPointValue = pointTracker.GetPreviousScore() + (pointTracker.GetPoints() - pointTracker.GetPreviousScore()) / 2;
			//set initial transition values
			pointTransitionTimer = 0;
			pointsTransitioning = true;
		}
		catch
		{
			Debug.LogError("Cannot update point UI");
		}
	}

	public void Resume()
	{
		if (screenState == ScreenState.PAUSE)
		{
			Pause(false);
		}
	}

	public bool Pause(bool pause)
	{
		switch (screenState)
		{
			case ScreenState.PAUSE:
				{
					//if already in correct state, return
					if (pause)
						return false;

					screenTransitionTimer = 0;
					screenState = ScreenState.TPAUSEOUT;
					return true;
				}

			case ScreenState.NOTHING:
				{
					if (!pause)
						return false;

					//disable input from player
					if (playerInput)
						playerInput.enabled = false;

					if (cameraInput)
						cameraInput.enabled = false;

					EnableScreen(ScreenState.PAUSE);

					screenState = ScreenState.TPAUSEIN;

					//pause time
					lastTimeScale = Time.timeScale;
					Time.timeScale = 0;
					screenTransitionTimer = 0;

					//set initial values for transition
					pausePanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
					var c = screenOverlay.color;
					c.a = 0;
					screenOverlay.color = c;
					return true;
				}

			default:
				return false;
		}
	}

	void EnableScreen(ScreenState screenType)
	{
		switch (screenType)
		{
			case ScreenState.WIN:
				pausePanel.gameObject.SetActive(false);
				optionsPanel.gameObject.SetActive(false);
				winPanel.gameObject.SetActive(true);
				screenOverlay.gameObject.SetActive(true);
				break;
			case ScreenState.PAUSE:
				pausePanel.gameObject.SetActive(true);
				optionsPanel.gameObject.SetActive(false);
				winPanel.gameObject.SetActive(false);
				screenOverlay.gameObject.SetActive(true);
				break;
			case ScreenState.OPTIONS:
				pausePanel.gameObject.SetActive(false);
				optionsPanel.gameObject.SetActive(true);
				winPanel.gameObject.SetActive(false);
				screenOverlay.gameObject.SetActive(true);
				break;
			case ScreenState.NOTHING:
				pausePanel.gameObject.SetActive(false);
				optionsPanel.gameObject.SetActive(false);
				winPanel.gameObject.SetActive(false);
				screenOverlay.gameObject.SetActive(false);
				break;		
		}
	}

	public void OpenOptions()
	{
		if (screenState == ScreenState.PAUSE)
		{
			screenState = ScreenState.TOPTIONSIN;
			screenTransitionTimer = 0;
			EnableScreen(ScreenState.OPTIONS);
			pausePanel.gameObject.SetActive(true);
		}
	}

	public void CloseOptions()
	{
		if (screenState == ScreenState.OPTIONS)
		{
			screenState = ScreenState.TOPTIONSOUT;
			pausePanel.gameObject.SetActive(true);
			screenTransitionTimer = 0;
		}
	}

	public void TransitionToExit()
	{
		StartCoroutine(ExitGame());
	}

	IEnumerator ExitGame()
	{
		sceneTransitionAnimator.SetTrigger("Leave");

		yield return new WaitForSecondsRealtime(sceneTransitionTime);

		try
		{
			Time.timeScale = 1;
			SceneManager.LoadScene(menuSceneName);
		}
		catch
		{
			Debug.LogError("Could not load scene '" + menuSceneName + "'.");
		}
	}

	public void OpenWinUI()
	{
		EnableScreen(ScreenState.WIN);
		screenState = ScreenState.TWININ;

		//pause time
		lastTimeScale = Time.timeScale;
		Time.timeScale = 0;
		screenTransitionTimer = 0;

		//set initial values for transition
		pausePanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
		var c = screenOverlay.color;
		c.a = 0;
		screenOverlay.color = c;
	}

	public void EnableNotifyText(bool enabled)
	{
		notifyText.enabled = enabled;
	}

	public bool CheckIsPaused()
	{
		return screenState == ScreenState.PAUSE;
	}	

	public void InitializeHealthUI(int maxHealth)
	{
		//if health UI has already been initialized
		if (healthChildren != null)
		{
			if (maxHealth > healthChildren.Count)
			{
				for (int i = healthChildren.Count - 1; i < maxHealth; i++)
				{
					healthChildren.Add(Instantiate(healthPrefab, healthParent));
				}
			}
			else if (maxHealth < healthChildren.Count)
			{
				for (int i = maxHealth; i < healthChildren.Count; i++)
				{
					GameObject.Destroy(healthChildren[i]);
					healthChildren.RemoveAt(i);
				}
			}
		}
		else if (maxHealth > 0)
		{
			healthChildren = new List<GameObject>(maxHealth);

			for (int i = 0; i < maxHealth; i++)
			{
				healthChildren.Add(Instantiate(healthPrefab, healthParent));
			}
		}
	}

	public void SetHealthUI(int currentHealth)
	{
		if (healthChildren == null)
		{
			Debug.LogError("Health UI has not been initialized");
		}
		else
		{
			for (int i = 0; i < healthChildren.Count; i++)
			{
				healthChildren[i].SetActive(i < currentHealth);
			}
		}
	}
}
