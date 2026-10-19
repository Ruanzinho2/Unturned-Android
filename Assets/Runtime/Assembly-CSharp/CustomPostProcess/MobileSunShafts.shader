Shader "Hidden/Custom/MobileSunShafts"
{
    HLSLINCLUDE

        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        TEXTURE2D_SAMPLER2D(_ColorBuffer, sampler_ColorBuffer);
        TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);

        float4 _SunPosition;
        half4 _SunColor;
        half _SunIntensity;
        float _BlurRadius;

        // Pass 0: Sky/Sun Depth Occlusion Mask
        float4 FragMask(VaryingsDefault i) : SV_Target
        {
            float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoordStereo);
            float depth = Linear01Depth(rawDepth);

            // Only skybox pixels pass through to generate rays
            if (depth < 0.99)
            {
                return float4(0.0, 0.0, 0.0, 0.0);
            }

            float dist = length(i.texcoord - _SunPosition.xy);
            float maxRad = max(0.001, _SunPosition.w);
            float rayFalloff = saturate(1.0 - dist / maxRad);
            rayFalloff = rayFalloff * rayFalloff;

            return float4(rayFalloff, rayFalloff, rayFalloff, 1.0);
        }

        // Pass 1: Radial Blur (Iteration 1)
        float4 FragRadialBlur1(VaryingsDefault i) : SV_Target
        {
            float2 step = (_SunPosition.xy - i.texcoord) * _BlurRadius;
            float4 sum = 0.0;
            for (int s = 0; s < 6; s++)
            {
                sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + step * (float)s);
            }
            return sum * (1.0 / 6.0);
        }

        // Pass 2: Radial Blur (Iteration 2 with wider step)
        float4 FragRadialBlur2(VaryingsDefault i) : SV_Target
        {
            float2 step = (_SunPosition.xy - i.texcoord) * (_BlurRadius * 2.0);
            float4 sum = 0.0;
            for (int s = 0; s < 6; s++)
            {
                sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + step * (float)s);
            }
            return sum * (1.0 / 6.0);
        }

        // Pass 3: Composite (Screen Blend shafts with scene)
        float4 FragComposite(VaryingsDefault i) : SV_Target
        {
            float4 sceneCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
            float4 shaftCol = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.texcoord) * _SunColor * _SunIntensity;

            // Screen blend mode: 1 - (1 - scene) * (1 - shafts)
            float3 finalRGB = 1.0 - (1.0 - sceneCol.rgb) * (1.0 - saturate(shaftCol.rgb));
            return float4(finalRGB, sceneCol.a);
        }

    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        // Pass 0: Depth Mask
        Pass
        {
            HLSLPROGRAM
                #pragma vertex VertDefault
                #pragma fragment FragMask
            ENDHLSL
        }

        // Pass 1: Radial Blur 1
        Pass
        {
            HLSLPROGRAM
                #pragma vertex VertDefault
                #pragma fragment FragRadialBlur1
            ENDHLSL
        }

        // Pass 2: Radial Blur 2
        Pass
        {
            HLSLPROGRAM
                #pragma vertex VertDefault
                #pragma fragment FragRadialBlur2
            ENDHLSL
        }

        // Pass 3: Composite
        Pass
        {
            HLSLPROGRAM
                #pragma vertex VertDefault
                #pragma fragment FragComposite
            ENDHLSL
        }
    }
    FallBack Off
}
