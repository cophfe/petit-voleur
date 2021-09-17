using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChefAlertUI : MonoBehaviour
{
	public Image suspicionLevelImage;
	public Image alertLevelImage;
	public float alertFillSpeed = 1;

	ChefAI chef;

    void Start()
    {
		chef = FindObjectOfType<ChefAI>();
		if (chef == null)
		{
			Debug.LogWarning("Alert UI could not find ChefAI component.");
			enabled = false;
		}
	}

    void Update()
    {
		suspicionLevelImage.fillAmount = chef.ferretStartAlertTimer / chef.alertedBeginDuration;
		
		float tAlert = chef.alertedTimer / chef.alertedDuration;
		float alertFill = alertLevelImage.fillAmount;
		alertLevelImage.fillAmount = Mathf.MoveTowards(alertFill, tAlert, Mathf.Abs(tAlert - alertFill) * Time.deltaTime * alertFillSpeed);
	}
}