using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VisualiseChefAlertLevel : MonoBehaviour
{
	public ChefAI chefAI;
	public Slider slider;

	void Update()
	{
		slider.value = chefAI.ferretStartAlertTimer / chefAI.alertedBeginDuration;
	}
}
