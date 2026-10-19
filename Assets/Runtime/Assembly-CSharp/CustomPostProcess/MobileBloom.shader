Shader "Hidden/Custom/MobileBloom"
{
    HLSLINCLUDE

        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        TEXTURE2D(_BloomTex);

        float4 _MainTex_TexelSize;
        half _Threshold;
        half _BlurAmount;
        half _BloomAmount;
        half4 _BloomColor;

        // Pass 0: Downsample & Threshold
        float4 FragThreshold(VaryingsDefault i) : SV_Target
        {
            float2 off = _MainTex_TexelSize.xy * _BlurAmount;
            float4 s0 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord - off);
            float4 s1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + float2(off.x, -off.y));
            float4 s2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + float2(-off.x, off.y));
            float4 s3 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + off);
            float4 col = (s0 + s1 + s2 + s3) * 0.25;

            half m = max(col.r, max(col.g, col.b));
            half factor = max(0.0h, m - _Threshold) / max(m, 0.00001h);
            return col * factor;
        }

        // Pass 1: Kawase Box Blur
        float4 FragBlur(VaryingsDefault i) : SV_Target
        {
            float2 off = _MainTex_TexelSize.xy * _BlurAmount;
            float4 s0 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord - off);
            float4 s1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + float2(off.x, -off.y));
            float4 s2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + float2(-off.x, off.y));
            float4 s3 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + off);
            return (s0 + s1 + s2 + s3) * 0.25;
        }

        // Pass 2: Composite (Scene + Bloom)
        float4 FragComposite(VaryingsDefault i) : SV_Target
        {
            float4 sceneCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
            float4 bloomCol = SAMPLE_TEXTURE2D(_BloomTex, sampler_MainTex, i.texcoord) * _BloomColor * _BloomAmount;
            if (isnan(bloomCol.r) || isinf(bloomCol.r) || isnan(bloomCol.g) || isinf(bloomCol.g) || isnan(bloomCol.b) || isinf(bloomCol.b))
            {
                bloomCol = float4(0.0, 0.0, 0.0, 0.0);
            }
            bloomCol = clamp(bloomCol, 0.0, 10.0);
            return sceneCol + bloomCol;
        }

    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        // Pass 0: Downsample + Threshold
        Pass
        {
            HLSLPROGRAM
                #pragma vertex VertDefault
                #pragma fragment FragThreshold
            ENDHLSL
        }

        // Pass 1: Kawase Blur
        Pass
        {
            HLSLPROGRAM
                #pragma vertex VertDefault
                #pragma fragment FragBlur
            ENDHLSL
        }

        // Pass 2: Additive Composite
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
