Shader "Custom/Water_Fallback"
{
    Properties
    {
        _Color ("Main Color", Color) = (0.21176, 0.74711, 1.0, 0.588)
        _BaseColor ("Base Color", Color) = (0.21176, 0.74711, 1.0, 0.588)
        _SpecularColor ("Specular Color", Color) = (0.8, 0.8, 0.8, 1.0)
        _Shininess ("Shininess", Range (0.01, 100)) = 30.0
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        LOD 200
        Cull Off
        ZWrite Off
        ZTest LEqual
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma surface surf WaterMobile alpha:fade
        #pragma target 2.0

        fixed4 _Color;
        fixed4 _BaseColor;
        fixed4 _SpecularColor;
        half _Shininess;

        struct Input
        {
            float3 viewDir;
        };

        inline fixed4 LightingWaterMobile (SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten)
        {
            half3 h = normalize (lightDir + viewDir);
            fixed diff = max (0.0, dot (s.Normal, lightDir));
            float nh = max (0.0, dot (s.Normal, h));
            float specPower = clamp(s.Specular, 1.0, 128.0);
            float spec = pow (nh, specPower) * s.Gloss;

            fixed3 specCol = (_SpecularColor.a > 0.01) ? _SpecularColor.rgb : fixed3(0.8, 0.8, 0.8);

            fixed4 c;
            c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * specCol * spec) * atten;
            c.a = s.Alpha;
            return c;
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 col = (_BaseColor.a > 0.01) ? _BaseColor : _Color;
            o.Albedo = col.rgb;
            o.Specular = _Shininess > 0.001 ? _Shininess : 30.0;
            o.Gloss = 0.6;
            o.Alpha = col.a > 0.01 ? col.a : 0.588;
        }
        ENDCG
    }
    FallBack "Transparent/Diffuse"
}
