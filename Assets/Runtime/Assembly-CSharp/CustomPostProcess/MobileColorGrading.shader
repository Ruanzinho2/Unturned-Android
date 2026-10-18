Shader "Hidden/Custom/MobileColorGrading"
{
    HLSLINCLUDE

        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);

        half _Exposure;
        half _Contrast;
        half _Saturation;
        half _VignetteIntensity;
        half _AcesEnabled;

        // ACES Film Tone Mapping curve (Narkowicz 2015 mobile approximation)
        inline half3 ACESFilm(half3 x)
        {
            half a = 2.51h;
            half b = 0.03h;
            half c = 2.43h;
            half d = 0.59h;
            half e = 0.14h;
            return saturate((x * (a * x + b)) / (x * (c * x + d) + e));
        }

        float4 Frag(VaryingsDefault i) : SV_Target
        {
            float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo);

            // 1. Exposure adjustment
            half3 c = color.rgb * _Exposure;

            // 2. ACES Tone Mapping
            if (_AcesEnabled > 0.5h)
            {
                c = ACESFilm(c);
            }

            // 3. Contrast adjustment (midpoint 0.5)
            c = (c - 0.5h) * _Contrast + 0.5h;

            // 4. Saturation adjustment
            half luma = dot(c, half3(0.2126h, 0.7152h, 0.0722h));
            c = lerp(half3(luma, luma, luma), c, _Saturation);

            // 5. Subtle mobile vignette
            if (_VignetteIntensity > 0.001h)
            {
                half2 coord = (i.texcoord - 0.5h) * 2.0h;
                half dist = dot(coord, coord);
                half vig = saturate(1.0h - dist * _VignetteIntensity * 0.5h);
                c *= vig;
            }

            return float4(saturate(c), color.a);
        }

    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
                #pragma vertex VertDefault
                #pragma fragment Frag
            ENDHLSL
        }
    }
}
