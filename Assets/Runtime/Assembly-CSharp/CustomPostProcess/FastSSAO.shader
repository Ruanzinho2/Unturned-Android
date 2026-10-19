Shader "Hidden/Custom/FastSSAO"
{
    HLSLINCLUDE

        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        TEXTURE2D(_AOTex);
        TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);

        float4 _CameraDepthTexture_TexelSize;
        float4 _MainTex_TexelSize;

        half _Intensity;
        half _Radius;
        float _AOFadeStart;
        float _AOFadeEnd;

        // Pass 0: Screen-Space Occlusion Generation
        float4 FragAO(VaryingsDefault i) : SV_Target
        {
            float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoordStereo);
            float depth = Linear01Depth(rawDepth);

            // Skip skybox
            if (depth > 0.99)
            {
                return float4(1.0, 1.0, 1.0, 1.0);
            }

            float eyeDepth = LinearEyeDepth(rawDepth);
            float fade = saturate((eyeDepth - _AOFadeStart) / max(1.0, _AOFadeEnd - _AOFadeStart));

            // Scale sample radius in screen space based on depth and aspect ratio
            float aspect = (_MainTex_TexelSize.w > 0.0) ? (_MainTex_TexelSize.z / _MainTex_TexelSize.w) : 1.0;
            if (isnan(aspect) || isinf(aspect) || aspect <= 0.0)
            {
                aspect = 1.0;
            }
            float2 radiusScaled = (_Radius / max(0.5, eyeDepth * 0.1)) * float2(1.0, aspect);

            const float2 samples[4] = {
                float2( 1.0,  0.0),
                float2(-1.0,  0.0),
                float2( 0.0,  1.0),
                float2( 0.0, -1.0)
            };

            float occlusion = 0.0;
            for (int k = 0; k < 4; k++)
            {
                float2 sampleUV = i.texcoord + samples[k] * radiusScaled;
                float sampleRaw = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, sampleUV);
                float sampleEye = LinearEyeDepth(sampleRaw);
                float diff = eyeDepth - sampleEye;
                if (diff > 0.02 && diff < 1.5)
                {
                    occlusion += (1.0 - diff / 1.5);
                }
            }

            occlusion = (occlusion * 0.25) * _Intensity * (1.0 - fade);
            if (isnan(occlusion) || isinf(occlusion))
            {
                occlusion = 0.0;
            }
            half ao = saturate(1.0h - occlusion);
            return float4(ao, ao, ao, 1.0);
        }

        // Pass 1: Fast Bilateral/Box Blur
        float4 FragBlur(VaryingsDefault i) : SV_Target
        {
            float2 off = _MainTex_TexelSize.xy * 1.5;
            half center = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord).r;
            half s0 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord - off).r;
            half s1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + off).r;
            half s2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + float2(-off.x, off.y)).r;
            half s3 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + float2(off.x, -off.y)).r;

            half ao = center * 0.40h + (s0 + s1 + s2 + s3) * 0.15h;
            ao = clamp(ao, 0.0h, 1.0h);
            return float4(ao, ao, ao, 1.0);
        }

        // Pass 2: Composite (Multiply AO into scene color)
        float4 FragComposite(VaryingsDefault i) : SV_Target
        {
            float4 sceneCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
            half ao = SAMPLE_TEXTURE2D(_AOTex, sampler_MainTex, i.texcoord).r;
            if (isnan(ao) || isinf(ao))
            {
                ao = 1.0h;
            }
            ao = clamp(ao, 0.0h, 1.0h);
            return float4(sceneCol.rgb * ao, sceneCol.a);
        }

    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        // Pass 0: AO Generation
        Pass
        {
            HLSLPROGRAM
                #pragma vertex VertDefault
                #pragma fragment FragAO
            ENDHLSL
        }

        // Pass 1: Blur
        Pass
        {
            HLSLPROGRAM
                #pragma vertex VertDefault
                #pragma fragment FragBlur
            ENDHLSL
        }

        // Pass 2: Composite
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
