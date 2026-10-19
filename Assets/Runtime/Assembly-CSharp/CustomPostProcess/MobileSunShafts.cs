using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace SDG.Unturned
{
	[Serializable]
	[PostProcess(typeof(MobileSunShaftsRenderer), PostProcessEvent.BeforeStack, "Custom/MobileSunShafts")]
	public sealed class MobileSunShafts : PostProcessEffectSettings
	{
		[Range(1, 4)]
		public IntParameter blurIterations = new IntParameter { value = 2 };

		[Range(0.1f, 2.0f)]
		public FloatParameter maxRadius = new FloatParameter { value = 0.85f };

		[Range(0.005f, 0.05f)]
		public FloatParameter blurRadius = new FloatParameter { value = 0.02f };

		public static Transform sunTransform;
		public static Color sunColor = Color.white;
		public static float sunIntensity = 1.0f;

		public override bool IsEnabledAndSupported(PostProcessRenderContext context)
		{
			return enabled.value && sunIntensity > 0.01f;
		}
	}

	public sealed class MobileSunShaftsRenderer : PostProcessEffectRenderer<MobileSunShafts>
	{
		public override DepthTextureMode GetCameraFlags()
		{
			return DepthTextureMode.Depth;
		}

		private Shader shader;
		private int colorBufferId;
		private int sunPositionId;
		private int sunColorId;
		private int sunIntensityId;
		private int blurRadiusId;

		private int rt0Id;
		private int rt1Id;

		public override void Init()
		{
			base.Init();
			shader = Shader.Find("Hidden/Custom/MobileSunShafts");
			ShaderLog.LogShaderInit("Hidden/Custom/MobileSunShafts", shader, "MobileSunShafts.Init");
			colorBufferId = Shader.PropertyToID("_ColorBuffer");
			sunPositionId = Shader.PropertyToID("_SunPosition");
			sunColorId = Shader.PropertyToID("_SunColor");
			sunIntensityId = Shader.PropertyToID("_SunIntensity");
			blurRadiusId = Shader.PropertyToID("_BlurRadius");

			rt0Id = Shader.PropertyToID("_MobileSunShaftsRT0");
			rt1Id = Shader.PropertyToID("_MobileSunShaftsRT1");
		}

		public override void Render(PostProcessRenderContext context)
		{
			if (shader == null || MobileSunShafts.sunTransform == null || MobileSunShafts.sunIntensity < 0.01f)
			{
				context.command.Blit(context.source, context.destination);
				return;
			}

			Vector3 sunViewport = context.camera.WorldToViewportPoint(MobileSunShafts.sunTransform.position);
			if (sunViewport.z <= 0f)
			{
				// Sun is behind the camera plane
				context.command.Blit(context.source, context.destination);
				return;
			}

			PropertySheet sheet = context.propertySheets.Get(shader);
			sheet.properties.SetVector(sunPositionId, new Vector4(sunViewport.x, sunViewport.y, 0f, settings.maxRadius.value));
			sheet.properties.SetColor(sunColorId, MobileSunShafts.sunColor);
			sheet.properties.SetFloat(sunIntensityId, MobileSunShafts.sunIntensity);
			sheet.properties.SetFloat(blurRadiusId, settings.blurRadius.value);

			int width = Mathf.Max(context.width / 4, 16);
			int height = Mathf.Max(context.height / 4, 16);

			context.command.GetTemporaryRT(rt0Id, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.Default);
			context.command.GetTemporaryRT(rt1Id, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.Default);

			// Pass 0: Sky/Depth mask
			context.command.BlitFullscreenTriangle(context.source, rt0Id, sheet, 0);

			// Pass 1: Radial blur 1
			context.command.BlitFullscreenTriangle(rt0Id, rt1Id, sheet, 1);

			int iterations = Mathf.Clamp(settings.blurIterations.value, 1, 4);
			int finalShaftsRT = rt1Id;

			if (iterations >= 2)
			{
				// Pass 2: Radial blur 2 with larger step
				context.command.BlitFullscreenTriangle(rt1Id, rt0Id, sheet, 2);
				finalShaftsRT = rt0Id;
			}

			// Pass 3: Composite screen blend onto destination
			context.command.SetGlobalTexture(colorBufferId, finalShaftsRT);
			context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 3);

			context.command.ReleaseTemporaryRT(rt0Id);
			context.command.ReleaseTemporaryRT(rt1Id);
		}
	}
}
