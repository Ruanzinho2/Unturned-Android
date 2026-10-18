Shader "Hidden/Custom/MobileSunShafts"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _ColorBuffer ("Shafts (RGB)", 2D) = "black" {}
        _SunPosition ("Sun Position (XY=ScreenPos, W=Radius)", Vector) = (0.5, 0.5, 0.0, 0.75)
        _SunColor ("Sun Color", Color) = (1.0, 0.95, 0.8, 1.0)
        _SunIntensity ("Sun Intensity", Float) = 1.0
        _BlurRadius ("Blur Radius", Float) = 0.015
    }

    CGINCLUDE
    #include "UnityCG.cginc"

    sampler2D _MainTex;
    sampler2D _ColorBuffer;
    UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

    float4 _SunPosition;
    half4 _SunColor;
    half _SunIntensity;
    float _BlurRadius;

    struct appdata
    {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct v2f
    {
        float4 pos : SV_POSITION;
        float2 uv : TEXCOORD0;
    };

    v2f vert(appdata v)
    {
        v2f o;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;
        return o;
    }

    // Pass 0: Sky/Sun Depth Occlusion Mask
    half4 frag_mask(v2f i) : SV_Target
    {
        float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
        float depth = Linear01Depth(rawDepth);

        // Only skybox pixels pass through to generate rays
        if (depth < 0.99)
        {
            return half4(0.0, 0.0, 0.0, 0.0);
        }

        float dist = length(i.uv - _SunPosition.xy);
        float maxRad = max(0.001, _SunPosition.w);
        float rayFalloff = saturate(1.0 - dist / maxRad);
        rayFalloff = rayFalloff * rayFalloff; // Smooth quadratic falloff

        return half4(rayFalloff, rayFalloff, rayFalloff, 1.0);
    }

    // Pass 1: Radial Blur (Iteration 1)
    half4 frag_radial_blur1(v2f i) : SV_Target
    {
        float2 step = (_SunPosition.xy - i.uv) * _BlurRadius;
        half4 sum = 0;
        for (int s = 0; s < 6; s++)
        {
            sum += tex2D(_MainTex, i.uv + step * s);
        }
        return sum * (1.0 / 6.0);
    }

    // Pass 2: Radial Blur (Iteration 2 with wider step)
    half4 frag_radial_blur2(v2f i) : SV_Target
    {
        float2 step = (_SunPosition.xy - i.uv) * (_BlurRadius * 2.0);
        half4 sum = 0;
        for (int s = 0; s < 6; s++)
        {
            sum += tex2D(_MainTex, i.uv + step * s);
        }
        return sum * (1.0 / 6.0);
    }

    // Pass 3: Composite (Screen Blend shafts with scene)
    half4 frag_composite(v2f i) : SV_Target
    {
        half4 sceneCol = tex2D(_MainTex, i.uv);
        half4 shaftCol = tex2D(_ColorBuffer, i.uv) * _SunColor * _SunIntensity;

        // Screen blend mode: 1 - (1 - scene) * (1 - shafts)
        half3 finalRGB = 1.0 - (1.0 - sceneCol.rgb) * (1.0 - saturate(shaftCol.rgb));
        return half4(finalRGB, sceneCol.a);
    }
    ENDCG

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        // Pass 0: Depth Mask
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_mask
            #pragma target 3.0
            ENDCG
        }

        // Pass 1: Radial Blur 1
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_radial_blur1
            #pragma target 3.0
            ENDCG
        }

        // Pass 2: Radial Blur 2
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_radial_blur2
            #pragma target 3.0
            ENDCG
        }

        // Pass 3: Composite
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_composite
            #pragma target 3.0
            ENDCG
        }
    }
    FallBack Off
}
