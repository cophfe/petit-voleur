/*==================================================
	Programmer: Connor Fettes
==================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BlurManager : MonoBehaviour
{
	public int defaultRendererIndex = 1;
	public int blurRendererIndex = 2;
	public bool enableBlur = true;
	public Material blurMaterial;

	int blurAmountId;
	UniversalAdditionalCameraData data = null;
	private void Start()
	{
		blurAmountId = Shader.PropertyToID("_BlurAmount");
		blurMaterial.SetFloat(blurAmountId, 0);
		SetBlurAmount(0);

		data = Camera.main.GetUniversalAdditionalCameraData();
	}

	public void EnableBlur()
	{
		if (data && enableBlur)
			data.SetRenderer(blurRendererIndex);
	}

	public void DisableBlur()
	{
		if (data && enableBlur)
			data.SetRenderer(defaultRendererIndex);
	}

	public void SetBlurAmount(float amount)
	{
		if (blurMaterial && enableBlur)
			blurMaterial.SetFloat(blurAmountId, amount);
	}

	private void OnDisable()
	{
		SetBlurAmount(0);
		DisableBlur();
	}
}
