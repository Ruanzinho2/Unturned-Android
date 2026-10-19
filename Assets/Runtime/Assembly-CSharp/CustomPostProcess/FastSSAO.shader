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
        // Pass 0: Screen-Space Occlusion Generation (Concave Crevice Contact SSAO)
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

            float aspect = (_MainTex_TexelSize.w > 0.0) ? (_MainTex_TexelSize.z / _MainTex_TexelSize.w) : 1.0;
            if (isnan(aspect) || isinf(aspect) || aspect <= 0.0)
            {
                aspect = 1.0;
            }

            // Tightly clamp screen radius (max 0.6% screen) to restrict AO strictly to contact creases
            float rScreen = clamp(_Radius / max(0.6, eyeDepth * 0.5), 0.001, 0.006);
            float2 radiusScaled = rScreen * float2(1.0, aspect);

            // 4 orthogonal samples: Right (+X), Left (-X), Up (+Y), Down (-Y)
            float dR = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoord + float2(radiusScaled.x, 0.0)));
            float dL = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoord - float2(radiusScaled.x, 0.0)));
            float dU = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoord + float2(0.0, radiusScaled.y)));
            float dD = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoord - float2(0.0, radiusScaled.y)));

            // Contact threshold (8 cm): objects disconnected by more than 8cm can NEVER cast SSAO
            const float maxContact = 0.08;

            // Discontinuity checks: disconnected foreground or background objects are rejected
            bool validR = abs(dR - eyeDepth) < maxContact;
            bool validL = abs(dL - eyeDepth) < maxContact;
            bool validU = abs(dU - eyeDepth) < maxContact;
            bool validD = abs(dD - eyeDepth) < maxContact;

            float occlusion = 0.0;

            // Horizontal concavity (crevice test along X)
            // If the surface is flat (wall or floor), dR + dL == 2 * eyeDepth, curvature is 0.
            // If one sample is on a foreground silhouette, valid check fails and wall receives ZERO occlusion!
            if (validR && validL)
            {
                float curveX = eyeDepth - (dR + dL) * 0.5;
                if (curveX > 0.003)
                {
                    occlusion += saturate(curveX / (maxContact * 0.5));
                }
            }

            // Vertical concavity (crevice test along Y)
            if (validU && validD)
            {
                float curveY = eyeDepth - (dU + dD) * 0.5;
                if (curveY > 0.003)
                {
                    occlusion += saturate(curveY / (maxContact * 0.5));
                }
            }

            // Diagonal corner contacts (at 45-degree angle for tight wall-floor corners)
            float2 diagScaled = radiusScaled * 0.7071;
            float d00 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoord + float2( diagScaled.x,  diagScaled.y)));
            float d11 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoord - float2( diagScaled.x,  diagScaled.y)));
            if (abs(d00 - eyeDepth) < maxContact && abs(d11 - eyeDepth) < maxContact)
            {
                float curveDiag = eyeDepth - (d00 + d11) * 0.5;
                if (curveDiag > 0.003)
                {
                    occlusion += saturate(curveDiag / (maxContact * 0.5));
                }
            }

            occlusion = (occlusion * 0.333) * _Intensity * (1.0 - fade);
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

            // Sharp depth boundary cutoff (0.08m = 8cm cutoff)
            float w0 = saturate(1.0 - abs(centerDepth - d0) * 12.5);
            float w1 = saturate(1.0 - abs(centerDepth - d1) * 12.5);
            float w2 = saturate(1.0 - abs(centerDepth - d2) * 12.5);
            float w3 = saturate(1.0 - abs(centerDepth - d3) * 12.5);

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
