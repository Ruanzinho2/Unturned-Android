using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace SDG.Unturned
{
	[Serializable]
	[PostProcess(typeof(MobileColorGradingRenderer), PostProcessEvent.AfterStack, "Custom/MobileColorGrading")]
	public sealed class MobileColorGrading : PostProcessEffectSettings
	{
		public BoolParameter acesTonemapping = new BoolParameter { value = true };

		[Range(0.2f, 3.0f)]
		public FloatParameter exposure = new FloatParameter { value = 1.0f };

		[Range(0.5f, 2.0f)]
		public FloatParameter contrast = new FloatParameter { value = 1.05f };

		[Range(0f, 2.0f)]
		public FloatParameter saturation = new FloatParameter { value = 1.1f };

		[Range(0f, 1.0f)]
		public FloatParameter vignetteIntensity = new FloatParameter { value = 0.2f };

		public override bool IsEnabledAndSupported(PostProcessRenderContext context)
		{
			return enabled.value;
		}
	}

	public sealed class MobileColorGradingRenderer : PostProcessEffectRenderer<MobileColorGrading>
	{
		private Shader shader;
		private int exposureId;
		private int contrastId;
		private int saturationId;
		private int vignetteIntensityId;
		private int acesEnabledId;

		public override void Init()
		{
			base.Init();
			shader = Shader.Find("Hidden/Custom/MobileColorGrading");
			ShaderLog.LogShaderInit("Hidden/Custom/MobileColorGrading", shader, "MobileColorGrading.Init");
			exposureId = Shader.PropertyToID("_Exposure");
			contrastId = Shader.PropertyToID("_Contrast");
			saturationId = Shader.PropertyToID("_Saturation");
			vignetteIntensityId = Shader.PropertyToID("_VignetteIntensity");
			acesEnabledId = Shader.PropertyToID("_AcesEnabled");
		}

		public override void Render(PostProcessRenderContext context)
		{
			if (shader == null)
			{
				shader = Shader.Find("Hidden/Custom/MobileColorGrading");
				if (shader == null)
				{
					context.command.Blit(context.source, context.destination);
					return;
				}
			}

			PropertySheet sheet = context.propertySheets.Get(shader);
			sheet.properties.Clear();

			sheet.properties.SetFloat(exposureId, settings.exposure.value);
			sheet.properties.SetFloat(contrastId, settings.contrast.value);
			sheet.properties.SetFloat(saturationId, settings.saturation.value);
			sheet.properties.SetFloat(vignetteIntensityId, settings.vignetteIntensity.value);
			sheet.properties.SetFloat(acesEnabledId, settings.acesTonemapping.value ? 1.0f : 0.0f);

			context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
		}
	}
}
