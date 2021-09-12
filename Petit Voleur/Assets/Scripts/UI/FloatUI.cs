/*==================================================
	Programmer: Connor Fettes
==================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FloatUI : MonoBehaviour
{
	[Header("Transforms")]
	[Tooltip("The transform for the chef character.")]
	public RectTransform chefTransform;
	[Tooltip("The transform for the ferret character.")]
	public RectTransform ferretTransform;
	[Tooltip("The object that has all item images as children.")]
	public RectTransform itemsParent;
	[Tooltip("The object that has all images that need to wobble (items and characters both) as children.")]
	public RectTransform imagesParent;

	[Header("Character Movement")]
	[Space(5)]
	[Tooltip("The amount of time before the characters rotate onto screen.")]
	public float characterStartTime = 2;
	[Tooltip("The amount the characters rotate left or right in degrees.")]
	public float characterWobbleMag = 1;
	[Tooltip("Proportional to the speed of the character's wobble.")]
	public float characterWobbleSpeed = 1;
	[Tooltip("Proportional to the effect noise has on the character's rotations.")]
	public float characterWobbleNoiseMag = 1;
	[Tooltip("Proportional to the effect noise has on the character's position.")]
	public float characterMoveNoiseMag = 1;
	[Tooltip("Proportional to the speed the noise sample positions change.")]
	public float characterNoiseSpeed = 1;
	[Tooltip("The base value used for the dampening sine wave used for character's wobble.")]
	public float wobbleBaseValue = 2.7182818284590452353602874713527f;

	[Header("Items Movement")]
	[Space(5)]
	[Tooltip("Proportional to the effect noise has on the items's rotations.")]
	public float wobbleNoiseMag = 1;
	[Tooltip("Proportional to the effect noise has on the item's position.")]
	public float moveNoiseMag = 1;
	[Tooltip("Proportional to the speed the noise sample positions change.")]
	public float noiseSpeed = 1;

	[Header("Image Rotate")]
	[Space(5)]
	[Tooltip("The max amount imagesParent can be rotated around the Y axis.")]
	public float maxYRotation = 10;
	[Tooltip("The max amount imagesParent can be rotated around the X axis.")]
	public float maxXRotation = 10;

	RectTransformStore[] items;
	RectTransformStore chef;
	RectTransformStore ferret;
	float timer = 0;

	void Start()
    {
		chef = new RectTransformStore(chefTransform);
		chef.transform.localRotation = Quaternion.Euler(0,0,180);
		ferret = new RectTransformStore(ferretTransform);
		ferret.transform.localRotation = Quaternion.Euler(0,0,180);

		items = new RectTransformStore[itemsParent.childCount];

		for (int i = 0; i < itemsParent.childCount; i++)
		{
			var child = itemsParent.GetChild(i);
			if (child)
			{
				items[i] = new RectTransformStore(child.GetComponent<RectTransform>());
			}
		}

	}

    void Update()
    {
		timer += Time.deltaTime;
		if (timer > characterStartTime)
		{
			//make characters follow sine wave
			float tX = ExponentialDampeningSineWave(characterWobbleMag, characterWobbleSpeed, wobbleBaseValue, timer - characterStartTime);

			//Make characters randomly float around
			chef.transform.localRotation = Quaternion.Euler(0, 0, tX + characterWobbleNoiseMag * (Mathf.PerlinNoise(characterNoiseSpeed * timer, 0) - 0.5f));
			ferret.transform.localRotation = Quaternion.Euler(0, 0, -tX + characterWobbleNoiseMag * (Mathf.PerlinNoise(characterNoiseSpeed * timer, 7753) - 0.5f));
			ferret.transform.anchoredPosition = ferret.initialAnchoredPosition + new Vector3(characterWobbleNoiseMag * (Mathf.PerlinNoise(56347, characterNoiseSpeed * timer) - 0.5f), characterWobbleNoiseMag * (Mathf.PerlinNoise(434987, characterNoiseSpeed * timer) - 0.5f), characterWobbleNoiseMag * (Mathf.PerlinNoise(655387, characterNoiseSpeed * timer) - 0.5f));
			chef.transform.anchoredPosition = chef.initialAnchoredPosition + new Vector3(characterWobbleNoiseMag * (Mathf.PerlinNoise(1456347, characterNoiseSpeed * timer) - 0.5f), characterWobbleNoiseMag * (Mathf.PerlinNoise(17434987, characterNoiseSpeed * timer) - 0.5f), characterWobbleNoiseMag * (Mathf.PerlinNoise(85655387, characterNoiseSpeed * timer) - 0.5f));
		}

		//Make items randomly float around
		for (int i = 0; i < items.Length; i++)
		{
			items[i].transform.localRotation = Quaternion.Euler(0, 0, wobbleNoiseMag * (Mathf.PerlinNoise(noiseSpeed * timer, 2663 * i) - 0.5f));
			items[i].transform.anchoredPosition = items[i].initialAnchoredPosition + new Vector3(moveNoiseMag * (Mathf.PerlinNoise(563 * i, noiseSpeed * timer) - 0.5f), moveNoiseMag * (Mathf.PerlinNoise(4349 * i, noiseSpeed * timer) - 0.5f), moveNoiseMag * (Mathf.PerlinNoise(6553 * i, noiseSpeed* timer) - 0.5f));
		}

	}

	float ExponentialDampeningSineWave(float magnitude, float speed, float baseVal, float x)
	{
		return magnitude * Mathf.Pow(baseVal, -speed * x) * Mathf.Cos(2 * speed * Mathf.PI * x);
	}

	public void OnMouseChange(InputValue value)
	{
		Vector2 input = value.Get<Vector2>();
		input.x = input.x / Screen.width - 0.5f;
		input.y = input.y / Screen.height - 0.5f;

		imagesParent.localRotation = Quaternion.Euler(input.y * maxXRotation, -input.x * maxYRotation, 0);
	}
}

[System.Serializable]
public struct RectTransformStore
{
	public RectTransform transform;
	public Vector3 initialAnchoredPosition { get; }

	public RectTransformStore(RectTransform transform)
	{
		this.transform = transform;
		initialAnchoredPosition = transform.anchoredPosition;
		
	}
}