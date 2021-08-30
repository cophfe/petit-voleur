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
	[Header("Pause Menu Variables ")]
	[Tooltip("The screen overlay the pause menu uses.")]
	public Image screenOverlay = null;
	[Tooltip("The pause menu panel.")]
	public RectTransform panel = null;
	public float pauseTransitionTime = 0.5f;

	//private variables
	enum PauseTransitionState
	{
		TRANSITIONIN,
		TRANSITIONOUT,
		IN,
		OUT
	}
	float pausePanelDefaultHeight;
	float overlayDefaultAlpha;
	PauseTransitionState pauseState = PauseTransitionState.OUT;
	float pauseTransitionTimer = 0;
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

	//these values are used for the easing function
	int targetPointValue;
	int startPointValue = 0;
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    public void SetPointUI(int points)
	{
		//set initial transition values
		pointTransitionTimer = 0;
		pointsTransitioning = true;
		//starts halfway done
		startPointValue = targetPointValue + (targetPointValue - startPointValue)/2;
		targetPointValue = points;
	}

	public void Resume()
	{
		if (pauseState == PauseTransitionState.IN)
		{
			Pause(false);
		}
	}

	public bool Pause(bool pause)
	{
		switch (pauseState)
		{
			case PauseTransitionState.IN:
				{

					//if already in correct state, return
					if (pause)
						return false;

					pauseTransitionTimer = 0;
					pauseState = PauseTransitionState.TRANSITIONOUT;
					return true;
				}

			case PauseTransitionState.OUT:
				{
					if (!pause)
						return false;

					screenOverlay.gameObject.SetActive(true);
					pauseState = PauseTransitionState.TRANSITIONIN;

					//pause time
					lastTimeScale = Time.timeScale;
					Time.timeScale = 0;
					pauseTransitionTimer = 0;

					//set initial values for transition
					panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
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
		//initialise pause menu values
		pausePanelDefaultHeight = panel.rect.height;
		overlayDefaultAlpha = screenOverlay.color.a;
		screenOverlay.gameObject.SetActive(false);

		//initialise point values
		defaultFontSize = pointValueText.fontSize;
		//startPointValue = GameManager.GetPoints();
	}

	private void Update()
	{
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		//~~~~~~~~~PAUSE MENU TRANSITION~~~~~~~~~~~~
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		switch (pauseState)
		{
			case PauseTransitionState.TRANSITIONIN:
				{
					//end transition
					if (pauseTransitionTimer >= pauseTransitionTime)
					{
						pauseState = PauseTransitionState.IN;
						panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pausePanelDefaultHeight);
						Color finalColor = screenOverlay.color;
						finalColor.a = overlayDefaultAlpha;
						screenOverlay.color = finalColor;
						break;
					}

					float t = pauseTransitionTimer / pauseTransitionTime;
					//transition panel height using ease out quad
					panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (1 - (1 - t) * (1 - t)) * pausePanelDefaultHeight);
					//transition overlay alpha linearly
					Color c = screenOverlay.color;
					c.a = overlayDefaultAlpha * t;
					screenOverlay.color = c;

					pauseTransitionTimer += Time.unscaledDeltaTime;
				}
				break;
			case PauseTransitionState.TRANSITIONOUT:
				{
					//end transition
					if (pauseTransitionTimer >= pauseTransitionTime)
					{
						pauseState = PauseTransitionState.OUT;

						//set final values
						panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
						var color = screenOverlay.color;
						color.a = 0;
						screenOverlay.color = color;
						Time.timeScale = lastTimeScale;

						screenOverlay.gameObject.SetActive(false);
						break;
					}

					float t = 1 - pauseTransitionTimer / pauseTransitionTime;
					//transition panel height using ease in quad
					panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (t * t) * pausePanelDefaultHeight);
					//transition overlay alpha linearly
					Color c = screenOverlay.color;
					c.a = overlayDefaultAlpha * t;
					screenOverlay.color = c;

					pauseTransitionTimer += Time.unscaledDeltaTime;
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
			if (startPointValue < targetPointValue)
				pointValueText.fontSize = defaultFontSize * (1 + scaleAddition);
			else
				//font size decrease is max 25%, otherwise it looks bad
				pointValueText.fontSize = defaultFontSize * (pointBounceMagnitude - scaleAddition* 0.25f) / pointBounceMagnitude;

			//set text value (linear, finishes 1/3 of the way through)
			if (pointTransitionTimer <= pointTransitionTime / 3)
			{
				int currentPointValue = startPointValue + (int)((targetPointValue - startPointValue) * t * 3);
				pointValueText.text = currentPointValue.ToString();
			}
			else
			{
				pointValueText.text = targetPointValue.ToString();
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

	////TEMPORARY (SET POINTS VALUE)
	//int pValue = 0;
	//public void OnDash()
	//{
	//	Debug.Log("POINTS!!!");
	//	pValue += 14;
	//	SetPointUI(pValue);
	//}

	////TEMPORARY (SET PAUSE VALUE)
	//public void OnGrab()
	//{
	//	Pause(true);
	//}
}
