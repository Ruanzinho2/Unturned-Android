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

            // World-space search radius scaled to screen pixels (clamped to prevent giant halos on close objects)
            float rScreen = clamp(_Radius / max(0.4, eyeDepth * 0.4), 0.001, 0.012);
            float2 radiusScaled = rScreen * float2(1.0, aspect);

            const float2 samples[4] = {
                float2( 1.0,  0.0),
                float2(-1.0,  0.0),
                float2( 0.0,  1.0),
                float2( 0.0, -1.0)
            };

            // Max occlusion range: 0.35m (tight contact shadow, eliminates background halo)
            const float maxDist = 0.35;
            float occlusion = 0.0;
            for (int k = 0; k < 4; k++)
            {
                float2 sampleUV = i.texcoord + samples[k] * radiusScaled;
                float sampleRaw = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, sampleUV);
                float sampleEye = LinearEyeDepth(sampleRaw);
                float diff = eyeDepth - sampleEye;
                // Only occlude if sample is in front of the center point and within contact distance
                if (diff > 0.01 && diff < maxDist)
                {
                    occlusion += (1.0 - diff / maxDist);
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

        // Pass 1: Depth-Aware Bilateral Blur (Prevents shadow bleeding across object silhouettes)
        float4 FragBlur(VaryingsDefault i) : SV_Target
        {
            float centerRaw = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoordStereo);
            float centerDepth = LinearEyeDepth(centerRaw);

            float2 off = _MainTex_TexelSize.xy * 1.5;
            half center = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord).r;
            half s0 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord - off).r;
            half s1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + off).r;
            half s2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + float2(-off.x, off.y)).r;
            half s3 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + float2(off.x, -off.y)).r;

            // Bilateral weights based on depth difference to preserve silhouette edges
            float d0 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoord - off));
            float d1 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoord + off));
            float d2 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoord + float2(-off.x, off.y)));
            float d3 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoord + float2(off.x, -off.y)));

            // Sharp depth boundary cutoff (0.2m)
            float w0 = saturate(1.0 - abs(centerDepth - d0) * 5.0);
            float w1 = saturate(1.0 - abs(centerDepth - d1) * 5.0);
            float w2 = saturate(1.0 - abs(centerDepth - d2) * 5.0);
            float w3 = saturate(1.0 - abs(centerDepth - d3) * 5.0);

            float totalWeight = 1.0 + (w0 + w1 + w2 + w3) * 0.5;
            half ao = (center + (s0 * w0 + s1 * w1 + s2 * w2 + s3 * w3) * 0.5h) / totalWeight;
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
