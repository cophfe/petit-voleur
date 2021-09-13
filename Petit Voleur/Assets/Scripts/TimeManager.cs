using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
	//Just used to visualise in inspector
	public float timeScale = 1.0f;
	public float targetTimeScale = 1.0f;
	public float defaultTransitionRatio = 0.1f;
	public float maxTransitionTime = 0.25f;
	private float timer = 0;
	private float currentTransitionRatio = 0;
	private float currentDuration = 0;

	private bool isPaused = false;
	private float prePauseTimeScale = 1;

    // Update is called once per frame
    void Update()
    {
		if (isPaused)
			return;

        if (timer > 0)
		{
			//Decrement timer by unscaled delta time so that timeScale doesn't affect the timer
			timer -= Time.unscaledDeltaTime;

			//Default lerp value
			float lerpValue = 1.0f;
			//0-1 value indicating how close the timer is to 0 from the duration
			float progress = Mathf.Clamp01(1 - (timer / currentDuration));
			
			//A transition was desired
			if (currentTransitionRatio > 0)
			{
				//Transition line
				lerpValue = (1 / currentTransitionRatio) * progress;
				
				//Offset and invert if returning to normal
				if (progress > 0.5f)
					lerpValue = (1 / currentTransitionRatio) - lerpValue; 
			}

			//Set timescale
			timeScale = Mathf.Lerp(1.0f, targetTimeScale, lerpValue);
			Time.timeScale = timeScale;

			//timer done, time to pack up
			if (timer <= 0)
			{
				ResetTimeModifier();
			}
		}
    }

	// ========================================================|
	//		--- Reset All TimeScale variables---
	//--------------------------------------------------------/
	public void ResetTimeModifier()
	{
		timeScale = 1.0f;
		Time.timeScale = 1.0f;
		targetTimeScale = 1.0f;
		timer = 0f;
	}

	// ========================================================|
	//		--- Start a time modifier ---
	//--------------------------------------------------------/
	//Transition ratio affects the duration of the transition; a ratio of 0.1 will dedicated 10%
	//		of the duration to the entry transition and a further 10% to the exit transition
	public void StartTimeModifier(float timeScale, float duration, float transitionRatio)
	{
		currentDuration = duration;
		//Start timer
		timer = currentDuration;

		targetTimeScale = timeScale;

		//If the transition ratio makes the transitionDuration too high, then limit it to the max transition time
		currentTransitionRatio = Mathf.Min(currentDuration * transitionRatio, maxTransitionTime) / currentDuration;
	}
	//Uses default transition ratio
	public void StartTimeModifier(float timeScale, float duration)
	{
		StartTimeModifier(timeScale, duration, defaultTransitionRatio);
	}

	// ========================================================|
	//		--- Start Pause ---
	//--------------------------------------------------------/
	public void Pause()
	{
		if (!isPaused)
		{
			prePauseTimeScale = Time.timeScale;
			Time.timeScale = 0;
			isPaused = true;
		}
	}

	// ========================================================|
	//		--- Stop Pause State ---
	//--------------------------------------------------------/
	public void Unpause()
	{
		if (isPaused)
		{
			Time.timeScale = prePauseTimeScale;
			isPaused = false;
		}
	}


}
