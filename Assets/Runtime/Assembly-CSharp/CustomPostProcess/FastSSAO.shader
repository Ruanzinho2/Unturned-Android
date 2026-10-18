Shader "Hidden/Custom/FastSSAO"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _AOTex ("AO (R)", 2D) = "white" {}
        _Intensity ("AO Intensity", Range(0.0, 3.0)) = 1.2
        _Radius ("Sample Radius", Range(0.001, 0.05)) = 0.015
        _AOFadeStart ("AO Fade Start (m)", Float) = 40.0
        _AOFadeEnd ("AO Fade End (m)", Float) = 100.0
    }

    CGINCLUDE
    #include "UnityCG.cginc"

    sampler2D _MainTex;
    sampler2D _AOTex;
    UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
    float4 _CameraDepthTexture_TexelSize;
    float4 _MainTex_TexelSize;

    half _Intensity;
    half _Radius;
    float _AOFadeStart;
    float _AOFadeEnd;

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

    struct v2f_blur
    {
        float4 pos : SV_POSITION;
        float2 uv : TEXCOORD0;
        float4 uv01 : TEXCOORD1;
    };

    v2f vert(appdata v)
    {
        v2f o;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;
        return o;
    }

    v2f_blur vert_blur(appdata v)
    {
        v2f_blur o;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;
        float2 off = _MainTex_TexelSize.xy * 1.5;
        o.uv01 = float4(v.uv - off, v.uv + off);
        return o;
    }

    // Pass 0: Screen-Space Occlusion Generation
    half4 frag_ao(v2f i) : SV_Target
    {
        float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
        float depth = Linear01Depth(rawDepth);

        // Skip skybox
        if (depth > 0.99)
        {
            return half4(1.0, 1.0, 1.0, 1.0);
        }

        float eyeDepth = LinearEyeDepth(rawDepth);
        float fade = saturate((eyeDepth - _AOFadeStart) / max(1.0, _AOFadeEnd - _AOFadeStart));

        // Sample 4 cross points for fast occlusion
        float2 radiusScaled = (_Radius / max(0.5, eyeDepth * 0.1)) * float2(1.0, _CameraDepthTexture_TexelSize.y / _CameraDepthTexture_TexelSize.x);
        
        static const float2 samples[4] = {
            float2( 1.0,  0.0),
            float2(-1.0,  0.0),
            float2( 0.0,  1.0),
            float2( 0.0, -1.0)
        };

        float occlusion = 0.0;
        for (int k = 0; k < 4; k++)
        {
            float2 sampleUV = i.uv + samples[k] * radiusScaled;
            float sampleRaw = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampleUV);
            float sampleEye = LinearEyeDepth(sampleRaw);
            float diff = eyeDepth - sampleEye;
            if (diff > 0.02 && diff < 1.5)
            {
                occlusion += (1.0 - diff / 1.5);
            }
        }

        occlusion = (occlusion * 0.25) * _Intensity * (1.0 - fade);
        half ao = saturate(1.0 - occlusion);
        return half4(ao, ao, ao, 1.0);
    }

    // Pass 1: Fast Bilateral/Box Blur
    half4 frag_blur(v2f_blur i) : SV_Target
    {
        half center = tex2D(_MainTex, i.uv).r;
        half s0 = tex2D(_MainTex, i.uv01.xy).r;
        half s1 = tex2D(_MainTex, i.uv01.zw).r;
        half s2 = tex2D(_MainTex, float2(i.uv01.x, i.uv01.w)).r;
        half s3 = tex2D(_MainTex, float2(i.uv01.z, i.uv01.y)).r;

        half ao = center * 0.40 + (s0 + s1 + s2 + s3) * 0.15;
        return half4(ao, ao, ao, 1.0);
    }

    // Pass 2: Composite (Multiply AO into scene color)
    half4 frag_composite(v2f i) : SV_Target
    {
        half4 sceneCol = tex2D(_MainTex, i.uv);
        half ao = tex2D(_AOTex, i.uv).r;
        return half4(sceneCol.rgb * ao, sceneCol.a);
    }
    ENDCG

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        // Pass 0: AO Generation
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_ao
            #pragma target 3.0
            ENDCG
        }

        // Pass 1: Blur
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_blur
            #pragma fragment frag_blur
            #pragma target 3.0
            ENDCG
        }

        // Pass 2: Composite
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
