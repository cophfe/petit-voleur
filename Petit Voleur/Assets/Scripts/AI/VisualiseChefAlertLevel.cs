using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VisualiseChefAlertLevel : MonoBehaviour
{
	public ChefAI chefAI;
	public Slider slider;

	public float val;

	void Update()
	{
		val = chefAI.ferretStartAlertTimer / chefAI.alertedBeginDuration;
	}
}
