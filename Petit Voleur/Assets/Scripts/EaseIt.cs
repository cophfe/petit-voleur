//--------------------------------------------------
//		--Actively updated by Rahul--
//------------------------------------------------/

using UnityEngine;
using System;

namespace EaseIt
{
	static class Easing
	{
		public delegate float EaseFunc(float t);

		private static float Pow2(float n)
		{
			return n * n;
		}

		private static float Pow3(float n)
		{
			return n * n * n;
		}

		private static float Pow4(float n)
		{
			return n * n * n * n;
		}

		private static float Pow5(float n)
		{
			return n * n * n * n * n;
		}

		public static float Mix(EaseFunc a, EaseFunc b, float blendWeight, float t)
		{
			return (1 - blendWeight) * a(t) + blendWeight * b(t);
		}

		public static float Crossfade(EaseFunc a, EaseFunc b, float t)
		{
			return (1 - t) * a(t) + t * b(t);
		}

		public static float BounceBottom(float t)
		{
			return Mathf.Abs(t);
		}

		public static float BounceTop(float t)
		{
			return 1.0f - Mathf.Abs(1.0f - t);
		}

		public static float BounceTopBottom(float t)
		{
			return BounceTop(BounceBottom(t));
		}

		public static float Flip(float t)
		{
			return 1 - t;
		}

		public static float Scale(EaseFunc ease, float t)
		{
			return t * ease(t);
		}

		public static float Scale(float easeVal, float t)
		{
			return t * easeVal;
		}

		public static float ReverseScale(EaseFunc ease, float t)
		{
			return (1 - t) * ease(t);
		}

		public static float ReverseScale(float easeVal, float t)
		{
			return (1 - t) * easeVal;
		}

		public static float SmoothStart2(float t)
		{
			return Pow2(t);
		}

		public static float SmoothStart3(float t)
		{
			return Pow3(t);
		}

		public static float SmoothStart4(float t)
		{
			return Pow4(t);
		}

		public static float SmoothStart5(float t)
		{
			return Pow5(t);
		}

		public static float SmoothStartN(float t, float n)
		{
			return Mathf.Pow(t, n);
		}

		public static float SmoothStop2(float t)
		{
			return 1 - Pow2(1 - t);
		}

		public static float SmoothStop3(float t)
		{
			return 1 - Pow3(1 - t);
		}

		public static float SmoothStop4(float t)
		{
			return 1 - Pow4(1 - t);
		}

		public static float SmoothStop5(float t)
		{
			return 1 - Pow5(1 - t);
		}

		public static float SmoothStopN(float t, float n)
		{
			return 1 - Mathf.Pow(1 - t, n);
		}

		public static float SmoothStep2(float t)
		{
			return Mathf.Lerp(SmoothStart2(t), SmoothStop2(t), t);
		}

		public static float SmoothStep3(float t)
		{
			return Mathf.Lerp(SmoothStart3(t), SmoothStop3(t), t);
		}

		public static float SmoothStep4(float t)
		{
			return Mathf.Lerp(SmoothStart4(t), SmoothStop4(t), t);
		}

		public static float SmoothStep5(float t)
		{
			return Mathf.Lerp(SmoothStart5(t), SmoothStop5(t), t);
		}

		public static float SmoothStepN(float t, float n)
		{
			return Mathf.Lerp(SmoothStartN(t, n), SmoothStopN(t, n), t);
		}

		public static class Arches
		{
			public static float Arch2(float t)
			{
				return Scale(Flip, t);
			}

			public static float ArchSmoothStart3(float t)
			{
				return Scale(Arch2, t);
			}

			public static float ArchSmoothStop3(float t)
			{
				return ReverseScale(Arch2, t);
			}

			public static float ArchSmoothStep4(float t)
			{
				return ReverseScale(Scale(Arch2, t), t);
			}

			public static float BellCurve6(float t)
			{
				return SmoothStart3(t) * SmoothStop3(t);
			}
		}

		public static class Bezier
		{
			public static float NormalisedBezier3(float b, float c, float t)
			{
				float s = 1.0f - t;
				float t2 = t*t;
				float s2 = s * s;
				float t3 = t2 * t;

				return (3.0f * b * s2 * t) + (3.0f * c * s * t2) + t3;
			}

			public static float NormalisedBezier4(float b, float c, float d, float t)
			{
				float s = 1.0f - t;
				float t2 = t*t;
				float s2 = s * s;
				float t3 = t2 * t;
				float s3 = s2 * s;
				float t4 = t3 * t;

				return (3.0f * b * s2 * t) + (3.0f * c * s * t2) + t3;
			}
		}
	}
}
