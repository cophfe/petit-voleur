/*==================================================
	Programmer: Connor Fettes
==================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FerretHealth : MonoBehaviour
{
	public int maxHealth = 3;
	int currentHealth;

	public int CurrentHealth
	{
		get
		{
			return currentHealth;
		}
	}

	GameUI UI;
	GameManager gM;

    void Start()
    {
		currentHealth = maxHealth;
		UI = FindObjectOfType<GameUI>();
		gM = FindObjectOfType<GameManager>();
		UI.InitializeHealthUI(maxHealth);
	}

    public void SetHealth(int health)
	{
		currentHealth = Mathf.Min(health, maxHealth);
		if (currentHealth <= 0)
			gM.OnDeath();

		UI.SetHealthUI(currentHealth);
	}

	public void Damage(int damageAmount = 1)
	{
		currentHealth -= damageAmount;
		if (currentHealth <= 0)
			gM.OnDeath();

		UI.SetHealthUI(currentHealth);
	}

	public void Heal(int healAmount = 1)
	{
		currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
		UI.SetHealthUI(currentHealth);
	}
}
