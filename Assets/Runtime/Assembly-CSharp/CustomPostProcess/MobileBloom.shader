Shader "Hidden/Custom/MobileBloom"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _BloomTex ("Bloom (RGB)", 2D) = "black" {}
        _Threshold ("Threshold", Float) = 0.8
        _BlurAmount ("Blur Amount", Float) = 1.0
        _BloomAmount ("Bloom Amount", Float) = 1.0
        _BloomColor ("Bloom Color", Color) = (1.0, 1.0, 1.0, 1.0)
    }

    CGINCLUDE
    #include "UnityCG.cginc"

    sampler2D _MainTex;
    sampler2D _BloomTex;
    float4 _MainTex_TexelSize;
    half _Threshold;
    half _BlurAmount;
    half _BloomAmount;
    half4 _BloomColor;

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
        float4 uv01 : TEXCOORD0; // (uv - off), (uv + off.x - off.y)
        float4 uv23 : TEXCOORD1; // (uv - off.x + off.y), (uv + off)
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
        float2 off = _MainTex_TexelSize.xy * _BlurAmount;
        o.uv01 = float4(v.uv - off, v.uv + float2(off.x, -off.y));
        o.uv23 = float4(v.uv + float2(-off.x, off.y), v.uv + off);
        return o;
    }

    // Pass 0: Downsample & Threshold
    half4 frag_threshold(v2f_blur i) : SV_Target
    {
        half4 s0 = tex2D(_MainTex, i.uv01.xy);
        half4 s1 = tex2D(_MainTex, i.uv01.zw);
        half4 s2 = tex2D(_MainTex, i.uv23.xy);
        half4 s3 = tex2D(_MainTex, i.uv23.zw);
        half4 col = (s0 + s1 + s2 + s3) * 0.25;

        half m = max(col.r, max(col.g, col.b));
        half factor = max(0.0, m - _Threshold) / max(m, 0.00001);
        return col * factor;
    }

    // Pass 1: Fast Kawase/Box Blur
    half4 frag_blur(v2f_blur i) : SV_Target
    {
        half4 s0 = tex2D(_MainTex, i.uv01.xy);
        half4 s1 = tex2D(_MainTex, i.uv01.zw);
        half4 s2 = tex2D(_MainTex, i.uv23.xy);
        half4 s3 = tex2D(_MainTex, i.uv23.zw);
        return (s0 + s1 + s2 + s3) * 0.25;
    }

    // Pass 2: Composite (Scene + Bloom)
    half4 frag_composite(v2f i) : SV_Target
    {
        half4 sceneCol = tex2D(_MainTex, i.uv);
        half4 bloomCol = tex2D(_BloomTex, i.uv) * _BloomColor * _BloomAmount;
        return sceneCol + bloomCol;
    }
    ENDCG

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        // Pass 0: Downsample + Threshold
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_blur
            #pragma fragment frag_threshold
            #pragma target 3.0
            ENDCG
        }

        // Pass 1: Kawase Blur
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_blur
            #pragma fragment frag_blur
            #pragma target 3.0
            ENDCG
        }

        // Pass 2: Additive Composite
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
