﻿/*==================================================
	Programmer: Connor Fettes
==================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FerretHealth : MonoBehaviour
{
	[Tooltip("The maximum and initial health value for the ferret.")]
	public int maxHealth = 3;
	int currentHealth;
	bool dead = false;

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
		if (!dead && currentHealth <= 0)
		{
			gM.OnDeath();
			dead = true;
			UI.SetHealthUI(0);
		}
		else
		{
			UI.SetHealthUI(currentHealth);
		}

	}

	public void Heal(int healAmount = 1)
	{
		currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
		UI.SetHealthUI(currentHealth);
	}
}
