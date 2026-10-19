using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace SDG.Unturned
{
	[Serializable]
	[PostProcess(typeof(MobileBloomRenderer), PostProcessEvent.AfterStack, "Custom/MobileBloom")]
	public sealed class MobileBloom : PostProcessEffectSettings
	{
		[Range(0f, 2f)]
		public FloatParameter threshold = new FloatParameter { value = 0.8f };

		[Range(0.1f, 3f)]
		public FloatParameter blurAmount = new FloatParameter { value = 1.2f };

		[Range(0f, 3f)]
		public FloatParameter intensity = new FloatParameter { value = 1.0f };

		public ColorParameter bloomColor = new ColorParameter { value = Color.white };

		[Range(1, 4)]
		public IntParameter numberOfPasses = new IntParameter { value = 2 };

		public override bool IsEnabledAndSupported(PostProcessRenderContext context)
		{
			return enabled.value && intensity.value > 0.001f;
		}
	}

	public sealed class MobileBloomRenderer : PostProcessEffectRenderer<MobileBloom>
	{
		private Shader shader;
		private int bloomTexId;
		private int thresholdId;
		private int blurAmountId;
		private int bloomAmountId;
		private int bloomColorId;

		private int rt0Id;
		private int rt1Id;

		public override void Init()
		{
			base.Init();
			shader = Shader.Find("Hidden/Custom/MobileBloom");
			ShaderLog.LogShaderInit("Hidden/Custom/MobileBloom", shader, "MobileBloom.Init");
			bloomTexId = Shader.PropertyToID("_BloomTex");
			thresholdId = Shader.PropertyToID("_Threshold");
			blurAmountId = Shader.PropertyToID("_BlurAmount");
			bloomAmountId = Shader.PropertyToID("_BloomAmount");
			bloomColorId = Shader.PropertyToID("_BloomColor");

			rt0Id = Shader.PropertyToID("_MobileBloomRT0");
			rt1Id = Shader.PropertyToID("_MobileBloomRT1");
		}

		public override void Render(PostProcessRenderContext context)
		{
			if (shader == null)
			{
				context.command.Blit(context.source, context.destination);
				return;
			}

			PropertySheet sheet = context.propertySheets.Get(shader);
			sheet.properties.SetFloat(thresholdId, settings.threshold.value);
			sheet.properties.SetFloat(blurAmountId, settings.blurAmount.value);
			sheet.properties.SetFloat(bloomAmountId, settings.intensity.value);
			sheet.properties.SetColor(bloomColorId, settings.bloomColor.value);

			int width = Mathf.Max(context.width / 4, 16);
			int height = Mathf.Max(context.height / 4, 16);

			context.command.GetTemporaryRT(rt0Id, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.Default);
			context.command.GetTemporaryRT(rt1Id, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.Default);

			// Pass 0: Downsample + Threshold from source to rt0
			context.command.BlitFullscreenTriangle(context.source, rt0Id, sheet, 0);

			// Pass 1: Kawase blur passes
			int passes = Mathf.Clamp(settings.numberOfPasses.value, 1, 4);
			int readRT = rt0Id;
			int writeRT = rt1Id;

			for (int p = 0; p < passes; p++)
			{
				sheet.properties.SetFloat(blurAmountId, settings.blurAmount.value * (1.0f + p * 0.5f));
				context.command.BlitFullscreenTriangle(readRT, writeRT, sheet, 1);
				int temp = readRT;
				readRT = writeRT;
				writeRT = temp;
			}

			// Pass 2: Composite onto destination
			sheet.properties.SetTexture(bloomTexId, readRT);
			context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 2);

			context.command.ReleaseTemporaryRT(rt0Id);
			context.command.ReleaseTemporaryRT(rt1Id);
		}
	}
}
