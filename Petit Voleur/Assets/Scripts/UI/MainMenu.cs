using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
	public float screenTransitionTime = 0.5f;
	public RectTransform mainPanel = null;
	public RectTransform optionsPanel = null;
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

					transitionTimer += Time.unscaledDeltaTime;
				}
				break;
			case ScreenState.TOPTIONSOUT:
				{

					transitionTimer += Time.unscaledDeltaTime;
				}
				break;
		}
	}

	public void PlayGame()
	{

	}

	public void OpenOptions()
	{

	}

	public void Exit()
	{

	}
}
