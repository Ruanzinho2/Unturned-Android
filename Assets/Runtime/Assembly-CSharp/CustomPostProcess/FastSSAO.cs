using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace SDG.Unturned
{
	[Serializable]
	[PostProcess(typeof(FastSSAORenderer), PostProcessEvent.BeforeStack, "Custom/FastSSAO")]
	public sealed class FastSSAO : PostProcessEffectSettings
	{
		[Range(0f, 3f)]
		public FloatParameter intensity = new FloatParameter { value = 1.2f };

		[Range(0.005f, 0.05f)]
		public FloatParameter radius = new FloatParameter { value = 0.015f };

		public FloatParameter fadeStart = new FloatParameter { value = 40.0f };

		public FloatParameter fadeEnd = new FloatParameter { value = 100.0f };

		public override bool IsEnabledAndSupported(PostProcessRenderContext context)
		{
			return enabled.value && intensity.value > 0.001f;
		}
	}

	public sealed class FastSSAORenderer : PostProcessEffectRenderer<FastSSAO>
	{
		private Shader shader;
		private int aoTexId;
		private int intensityId;
		private int radiusId;
		private int fadeStartId;
		private int fadeEndId;
		private int rt0Id;
		private int rt1Id;

		public override void Init()
		{
			base.Init();
			shader = Shader.Find("Hidden/Custom/FastSSAO");
			aoTexId = Shader.PropertyToID("_AOTex");
			intensityId = Shader.PropertyToID("_Intensity");
			radiusId = Shader.PropertyToID("_Radius");
			fadeStartId = Shader.PropertyToID("_AOFadeStart");
			fadeEndId = Shader.PropertyToID("_AOFadeEnd");

			rt0Id = Shader.PropertyToID("_FastSSAORT0");
			rt1Id = Shader.PropertyToID("_FastSSAORT1");
		}

		public override void Render(PostProcessRenderContext context)
		{
			if (shader == null)
			{
				context.command.Blit(context.source, context.destination);
				return;
			}

			PropertySheet sheet = context.propertySheets.Get(shader);
			sheet.properties.SetFloat(intensityId, settings.intensity.value);
			sheet.properties.SetFloat(radiusId, settings.radius.value);
			sheet.properties.SetFloat(fadeStartId, settings.fadeStart.value);
			sheet.properties.SetFloat(fadeEndId, settings.fadeEnd.value);

			int width = Mathf.Max(context.width / 2, 16);
			int height = Mathf.Max(context.height / 2, 16);

			context.command.GetTemporaryRT(rt0Id, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.R8);
			context.command.GetTemporaryRT(rt1Id, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.R8);

			// Pass 0: Generate AO from camera depth
			context.command.BlitFullscreenTriangle(context.source, rt0Id, sheet, 0);

			// Pass 1: Blur AO mask
			context.command.BlitFullscreenTriangle(rt0Id, rt1Id, sheet, 1);

			// Pass 2: Multiply AO with source onto destination
			context.command.SetGlobalTexture(aoTexId, rt1Id);
			context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 2);

			context.command.ReleaseTemporaryRT(rt0Id);
			context.command.ReleaseTemporaryRT(rt1Id);
		}
	}
}
