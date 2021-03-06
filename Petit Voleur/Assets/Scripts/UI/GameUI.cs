/*==================================================
	Programmer: Connor Fettes
==================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using UnityEngine.Events;
using UnityEngine.Audio;

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
	[Tooltip("The lose menu panel.")]
	public RectTransform losePanel = null;
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
	[Tooltip("The amount of time it takes to open the death UI when you die")]
	public float deathTransitionTime = 5;
	[Tooltip("The maximum blur amount.")]
	public float maxBlurAmount = 0.02f;
	[Tooltip("The Hit Overlay Animator")]
	public Animator hitAnimator = null;
	[Tooltip("An event that is invoked when closing the menu")]
	public UnityEvent onLeaveMenu = null;
	[Tooltip("The button used on android to win the game")]
	public GameObject androidWinButton;
	[Tooltip("The gameobject used to notify users on how to win on pc")]
	public GameObject windowsWinNotify;

	//private variables
	/// <summary>
	/// all different screen states.
	/// </summary>
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
		TLOSEIN,
		TLOSEOUT,
		LOSE,
		NOTHING
	}

	//default heights for panels (used for transitions)
	float pausePanelDefaultHeight;
	float winPanelDefaultHeight;
	float optionsPanelDefaultHeight;
	float losePanelDefaultHeight;
	//the initial alpha of the screen overlay
	float overlayDefaultAlpha;
	//the current screen state
	ScreenState screenState = ScreenState.NOTHING;
	//the timer used for transitioning menus
	float screenTransitionTimer = 0;
	Animator notifyTextAnimator = null;

	//manager used for blurring background
	BlurManager blurManager;
	//other stuff
	GameManager gM;
	TimeManager tM;
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
	[Tooltip("The object representing 1 unit of health.")]
	public GameObject healthPrefab = null;
	[Tooltip("The object that holds the health unit objects. Should have a horizontal or vertical layout group attached.")]
	public RectTransform healthParent = null;

	//private variables
	List<Animator> healthChildren = null;
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

	private void Start()
	{
		gM = FindObjectOfType<GameManager>();
		tM = FindObjectOfType<TimeManager>();
		notifyTextAnimator = notifyText.GetComponent<Animator>();

		//initialise menu values
		pausePanelDefaultHeight = pausePanel.rect.height;
		winPanelDefaultHeight = winPanel.rect.height;
		optionsPanelDefaultHeight = optionsPanel.rect.height;
		losePanelDefaultHeight = losePanel.rect.height;
		overlayDefaultAlpha = screenOverlay.color.a;
		EnableScreen(ScreenState.NOTHING);

		//initialise point values
		defaultFontSize = pointValueText.fontSize;

		//find the point tracker
		pointTracker = FindObjectOfType<PointTracker>();

		//find the blur manager
		blurManager = GetComponent<BlurManager>();
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
						blurManager.SetBlurAmount(maxBlurAmount);
						break;
					}

					float t = screenTransitionTimer / screenTransitionTime;
					t = (1 - (1 - t) * (1 - t));
					//transition overlay alpha linearly
					Color c = screenOverlay.color;
					c.a = overlayDefaultAlpha * t;
					screenOverlay.color = c;
					blurManager.SetBlurAmount(maxBlurAmount * t);
					//transition panel height using ease out quad
					pausePanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, t * pausePanelDefaultHeight);


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
						tM.Unpause();

						EnableScreen(ScreenState.NOTHING);

						//enable input from player
						gM.GameInputEnabled = true;

						blurManager.SetBlurAmount(0);
						blurManager.DisableBlur();

						break;
					}

					float t = 1 - screenTransitionTimer / screenTransitionTime;
					t = t * t;
					blurManager.SetBlurAmount(t * maxBlurAmount);

					//transition panel height using ease in quad
					pausePanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, t * pausePanelDefaultHeight);
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
						blurManager.SetBlurAmount(maxBlurAmount);

						break;
					}

					float t = screenTransitionTimer / screenTransitionTime;
					t = (1 - (1 - t) * (1 - t));

					//transition panel height using ease out quad
					winPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, t * winPanelDefaultHeight);
					//transition overlay alpha linearly
					blurManager.SetBlurAmount(t * maxBlurAmount);
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
			case ScreenState.TLOSEIN:
				{
					//end transition
					if (screenTransitionTimer >= screenTransitionTime)
					{
						screenState = ScreenState.LOSE;
						losePanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, losePanelDefaultHeight);
						Color finalColor = screenOverlay.color;
						finalColor.a = overlayDefaultAlpha;
						screenOverlay.color = finalColor;
						blurManager.SetBlurAmount(maxBlurAmount);

						break;
					}

					float t = screenTransitionTimer / screenTransitionTime;
					t = (1 - (1 - t) * (1 - t));

					//transition panel height using ease out quad
					losePanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, t * losePanelDefaultHeight);
					//transition overlay alpha linearly
					blurManager.SetBlurAmount(t * maxBlurAmount);
					Color c = screenOverlay.color;
					c.a = overlayDefaultAlpha * t;
					screenOverlay.color = c;

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
				pointValueText.fontSize = defaultFontSize * (pointBounceMagnitude - scaleAddition * 0.25f) / pointBounceMagnitude;

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
				completionBar.fillAmount = (float)pointTracker.GetPoints() / pointTracker.GetMaxLimit();
			}

			pointTransitionTimer += Time.deltaTime;
		}
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	}

	/// <summary>
	/// Updates the point UI based on the current point value
	/// </summary>
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

	/// <summary>
	/// Resumes the game
	/// </summary>
	public void Resume()
	{
		if (screenState == ScreenState.PAUSE)
		{
			Pause(false);
		}
	}

	/// <summary>
	/// Toggles menu state based on current state
	/// </summary>
	public void ToggleMenu()
	{
		switch (screenState)
		{
			case ScreenState.PAUSE:
				{
					Pause(false);
					if (onLeaveMenu != null)
						onLeaveMenu.Invoke();
				}
				break;
			case ScreenState.OPTIONS:
				{
					CloseOptions();
				}
				break;
			case ScreenState.NOTHING:
				{
					Pause(true);
				}
				break;
		}
	}

	/// <summary>
	/// Pauses/unpauses menu
	/// </summary>
	/// <param name="pause">whether to pause or unpause</param>
	/// <returns>if the pause was a success or not</returns>
	public bool Pause(bool pause)
	{
		switch (screenState)
		{
			case ScreenState.PAUSE:
				{
					//if already in correct state, return
					if (pause)
						return false;

					//lock cursor
					gM.CursorLocked = true;

					screenTransitionTimer = 0;
					screenState = ScreenState.TPAUSEOUT;
					return true;
				}

			case ScreenState.NOTHING:
				{
					if (!pause)
						return false;

					//free cursor
					gM.CursorLocked = false;

					//enable blur forward renderer (quicker than a second camera apparently, and waaaaaaay quicker than disabling a render feature i think)
					blurManager.EnableBlur();
					blurManager.SetBlurAmount(0);

					//disable input from player
					gM.GameInputEnabled = false;

					EnableScreen(ScreenState.PAUSE);

					screenState = ScreenState.TPAUSEIN;

					//pause time
					tM.Pause();

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

	/// <summary>
	/// Sets what UI objects are active based on a screentype
	/// </summary>
	/// <param name="screenType">the screen type to enable (transition screens will not work)</param>
	void EnableScreen(ScreenState screenType)
	{
		switch (screenType)
		{
			case ScreenState.WIN:
				pausePanel.gameObject.SetActive(false);
				optionsPanel.gameObject.SetActive(false);
				winPanel.gameObject.SetActive(true);
				screenOverlay.gameObject.SetActive(true);
				losePanel.gameObject.SetActive(false);
				break;
			case ScreenState.PAUSE:
				pausePanel.gameObject.SetActive(true);
				optionsPanel.gameObject.SetActive(false);
				winPanel.gameObject.SetActive(false);
				screenOverlay.gameObject.SetActive(true);
				losePanel.gameObject.SetActive(false);
				break;
			case ScreenState.OPTIONS:
				pausePanel.gameObject.SetActive(false);
				optionsPanel.gameObject.SetActive(true);
				winPanel.gameObject.SetActive(false);
				screenOverlay.gameObject.SetActive(true);
				losePanel.gameObject.SetActive(false);
				break;
			case ScreenState.NOTHING:
				pausePanel.gameObject.SetActive(false);
				optionsPanel.gameObject.SetActive(false);
				winPanel.gameObject.SetActive(false);
				screenOverlay.gameObject.SetActive(false);
				losePanel.gameObject.SetActive(false);
				break;
			case ScreenState.LOSE:
				screenOverlay.gameObject.SetActive(true);
				losePanel.gameObject.SetActive(true);
				pausePanel.gameObject.SetActive(false);
				optionsPanel.gameObject.SetActive(false);
				winPanel.gameObject.SetActive(false);
				break;
		}
	}

	/// <summary>
	/// Opens the options menu
	/// </summary>
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

	/// <summary>
	/// Closes the options menu
	/// </summary>
	public void CloseOptions()
	{
		if (screenState == ScreenState.OPTIONS)
		{
			screenState = ScreenState.TOPTIONSOUT;
			pausePanel.gameObject.SetActive(true);
			screenTransitionTimer = 0;
		}
	}

	/// <summary>
	/// Starts transitioning to the exit screen
	/// </summary>
	public void TransitionToExit()
	{
		StartCoroutine(ExitGame());
	}

	/// <summary>
	/// Exits the scene after a leave animation
	/// </summary>
	IEnumerator ExitGame()
	{
		sceneTransitionAnimator.SetTrigger("Leave");

		yield return new WaitForSecondsRealtime(sceneTransitionTime);

		try
		{
			tM.Unpause();
			Time.timeScale = 1;
			SceneManager.LoadScene(menuSceneName);
		}
		catch
		{
			Debug.LogError("Could not load scene '" + menuSceneName + "'.");
		}
	}

	/// <summary>
	/// Starts restarting
	/// </summary>
	public void TransitionToRestart()
	{
		StartCoroutine(RestartGame());
	}

	/// <summary>
	/// Exits the scene after a leave animation
	/// </summary>
	IEnumerator RestartGame()
	{
		sceneTransitionAnimator.SetTrigger("Leave");

		yield return new WaitForSecondsRealtime(sceneTransitionTime);

		try
		{
			tM.Unpause();
			Time.timeScale = 1;
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}
		catch
		{
			Debug.LogError("Could not load scene '" + menuSceneName + "'.");
		}
	}

	/// <summary>
	/// Opens the win ui
	/// </summary>
	public void OpenWinUI()
	{
		EnableScreen(ScreenState.WIN);
		screenState = ScreenState.TWININ;
		blurManager.EnableBlur();
		gM.GameInputEnabled = false;

		//unlock cursor
		gM.CursorLocked = false;

		//pause time
		tM.Pause();

		screenTransitionTimer = 0; 

		//set initial values for transition
		pausePanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
		var c = screenOverlay.color;
		c.a = 0;
		screenOverlay.color = c;
	}

	/// <summary>
	/// Enables win UI based on current platform
	/// </summary>
	public void EnableWinNotifyUI(bool enabled)
	{
		switch (Application.platform)
		{
			case RuntimePlatform.Android:
			{
				androidWinButton.SetActive(enabled);
				break;
			}
			default:
			{
				windowsWinNotify.SetActive(enabled);
				break;
			}
		}
		
	}

	/// <summary>
	/// Gets the animator for the notify text
	/// </summary>
	/// <returns></returns>
	public Animator GetNotifyAnimator()
	{
		return notifyTextAnimator;
	}

	/// <summary>
	/// tells gamemanager to try to win
	/// </summary>
	public void OnWinButtonClicked()
	{
		gM.OnWin();
	}

	/// <summary>
	/// Checks if game is paused
	/// </summary>
	/// <returns>pause value</returns>
	public bool CheckIsPaused()
	{
		return screenState == ScreenState.PAUSE;
	}	

	/// <summary>
	/// Sets initial value of health ui
	/// </summary>
	/// <param name="maxHealth"></param>
	public void InitializeHealthUI(int maxHealth)
	{
		//if health UI has already been initialized
		if (healthChildren != null)
		{
			return;
		}
		else if (maxHealth > 0)
		{
			healthChildren = new List<Animator>(maxHealth);

			for (int i = 0; i < maxHealth; i++)
			{
				healthChildren.Add(Instantiate(healthPrefab, healthParent).GetComponentInChildren<Animator>());
				healthChildren[i].Play("Alive", 0, (i % maxHealth) * 0.05f);
			}
		}
	}

	/// <summary>
	/// Sets current health UI to correct amount of hearts
	/// </summary>
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
				healthChildren[healthChildren.Count - i - 1].SetBool("Living", currentHealth - i > 0);
			}
			if (hitAnimator)
				hitAnimator.SetTrigger("Hit");
		}
	}
	
	/// <summary>
	/// Transitions to the lose game state
	/// </summary>
	public void TransitionToLose()
	{
		StartCoroutine(LoseGame());
	}

	/// <summary>
	/// Opens up lose UI
	/// </summary>
	/// <returns></returns>
	IEnumerator LoseGame()
	{
		yield return new WaitForSecondsRealtime(deathTransitionTime);
		EnableScreen(ScreenState.LOSE);
		screenState = ScreenState.TLOSEIN;

		blurManager.EnableBlur();
		gM.GameInputEnabled = false;

		//unlock cursor
		gM.CursorLocked = false;

		screenTransitionTimer = 0;

		//set initial values for transition
		losePanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
		var c = screenOverlay.color;
		c.a = 0;
		screenOverlay.color = c;
	}
}
