using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChefAlertUI : MonoBehaviour
{
	public Image suspicionLevelImage;
	public Image alertLevelImage;
	public float alertFillSpeed = 1;
	public Animator notifyTextAnimator;
	GameManager gM;

	ChefAI chef;
	bool alert = false;

    void Start()
    {
		gM = FindObjectOfType<GameManager>();

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

		//chef is alert!
		if (chef.alertedTimer > 0 && !alert)
		{
			notifyTextAnimator.SetBool("Panic", true);
			alert = true;
		}
		else if (chef.alertedTimer <= 0 && alert)
		{
			notifyTextAnimator.SetBool("Panic", false);
			alert = false;
		}
	}
}