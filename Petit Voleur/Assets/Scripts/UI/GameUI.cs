/*==================================================
	Programmer: Connor Fettes
==================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//~~~~~~~~~~~PAUSE MENU STUFF~~~~~~~~~~~~~~~
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
	public GameObject notifyText = null;
	public float screenTransitionTime = 0.5f;

	//private variables
	enum ScreenTransitionState
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
	float pausePanelDefaultHeight;
	float winPanelDefaultHeight;
	float optionsPanelDefaultHeight;
	float overlayDefaultAlpha;
	ScreenTransitionState screenState = ScreenTransitionState.NOTHING;
	float screenTransitionTimer = 0;
	float lastTimeScale = 1;
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

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

	//these value osused for the easing function
	int startPointValue = 0;
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    public void UpdatePointUI()
	{
		//set initial transition values
		pointTransitionTimer = 0;
		pointsTransitioning = true;
		//starts halfway done
		startPointValue = pointTracker.GetPreviousScore() + (pointTracker.GetPoints() - pointTracker.GetPreviousScore()) /2;
	}

	public void Resume()
	{
		if (screenState == ScreenTransitionState.PAUSE)
		{
			Pause(false);
		}
	}

	public bool Pause(bool pause)
	{
		switch (screenState)
		{
			case ScreenTransitionState.PAUSE:
				{

					//if already in correct state, return
					if (pause)
						return false;

					screenTransitionTimer = 0;
					screenState = ScreenTransitionState.TPAUSEOUT;
					return true;
				}

			case ScreenTransitionState.NOTHING:
				{
					if (!pause)
						return false;

					EnableScreen(ScreenTransitionState.PAUSE);

					screenState = ScreenTransitionState.TPAUSEIN;

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

	private void Start()
	{
		//initialise menu values
		pausePanelDefaultHeight = pausePanel.rect.height;
		winPanelDefaultHeight = winPanel.rect.height;
		optionsPanelDefaultHeight = optionsPanel.rect.height;
		overlayDefaultAlpha = screenOverlay.color.a;
		EnableScreen(ScreenTransitionState.NOTHING);

		//initialise point values
		defaultFontSize = pointValueText.fontSize;

		GameObject eS = GameObject.Find("EventSystem");
		if (eS != null)
			pointTracker = eS.GetComponent<PointTracker>();
	}

	int pointValue = 0;
	private void Update()
	{
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		//~~~~~~~~~~~~MENU TRANSITION~~~~~~~~~~~~~~~
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		switch (screenState)
		{
			case ScreenTransitionState.TPAUSEIN:
				{
					//end transition
					if (screenTransitionTimer >= screenTransitionTime)
					{
						screenState = ScreenTransitionState.PAUSE;
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
			case ScreenTransitionState.TPAUSEOUT:
				{
					//end transition
					if (screenTransitionTimer >= screenTransitionTime)
					{
						screenState = ScreenTransitionState.NOTHING;

						//set final values
						pausePanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
						var color = screenOverlay.color;
						color.a = 0;
						screenOverlay.color = color;
						Time.timeScale = lastTimeScale;

						EnableScreen(ScreenTransitionState.NOTHING);
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
			case ScreenTransitionState.TWININ:
				{
					//end transition
					if (screenTransitionTimer >= screenTransitionTime)
					{
						screenState = ScreenTransitionState.WIN;
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
			}

			pointTransitionTimer += Time.deltaTime;
		}
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	}

	void EnableScreen(ScreenTransitionState screenType)
	{
		switch (screenType)
		{
			case ScreenTransitionState.WIN:
				pausePanel.gameObject.SetActive(false);
				optionsPanel.gameObject.SetActive(false);
				winPanel.gameObject.SetActive(true);
				screenOverlay.gameObject.SetActive(true);
				break;
			case ScreenTransitionState.PAUSE:
				pausePanel.gameObject.SetActive(true);
				optionsPanel.gameObject.SetActive(false);
				winPanel.gameObject.SetActive(false);
				screenOverlay.gameObject.SetActive(true);
				break;
			case ScreenTransitionState.OPTIONS:
				pausePanel.gameObject.SetActive(false);
				optionsPanel.gameObject.SetActive(true);
				winPanel.gameObject.SetActive(false);
				screenOverlay.gameObject.SetActive(true);
				break;
			case ScreenTransitionState.NOTHING:
				pausePanel.gameObject.SetActive(false);
				optionsPanel.gameObject.SetActive(false);
				winPanel.gameObject.SetActive(false);
				screenOverlay.gameObject.SetActive(false);
				break;		
		}
	}

	public void OpenOptions()
	{

	}

	public void CloseOptions()
	{

	}

	public void ExitGame()
	{
		Application.Quit();
	}

	public void OpenWinUI()
	{
		EnableScreen(ScreenTransitionState.WIN);
		screenState = ScreenTransitionState.TWININ;

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
		notifyText.SetActive(enabled);
	}

	public bool CheckIsPaused()
	{
		return screenState == ScreenTransitionState.PAUSE;
	}	
}
