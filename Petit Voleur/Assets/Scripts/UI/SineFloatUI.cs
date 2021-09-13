using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SineFloatUI : MonoBehaviour
{
	public float floatMagnitude = 1;
	public float floatTime = 1;
	public float floatTimeOffset = 0;
	public bool randTimeOffset = false;
	Vector2 initialPosition;
	RectTransform rectTransform;

	private void Start()
	{
		rectTransform = GetComponent<RectTransform>();
		initialPosition = rectTransform.anchoredPosition;
		if (randTimeOffset)
		{
			floatTimeOffset += Random.Range(-99, 99);
		}
	}

	private void Update()
	{
		rectTransform.anchoredPosition = new Vector2(initialPosition.x, initialPosition.y + floatMagnitude * Mathf.Sin(Time.timeSinceLevelLoad + floatTimeOffset));
	}
}
