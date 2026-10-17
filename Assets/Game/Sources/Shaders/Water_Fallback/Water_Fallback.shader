Shader "Custom/Water_Fallback"
{
    Properties
    {
        _Color ("Main Color", Color) = (0.21176, 0.74711, 1.0, 0.588)
        _BaseColor ("Base Color", Color) = (0.21176, 0.74711, 1.0, 0.588)
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
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
        Cull Back
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma surface surf Lambert alpha:fade
        #pragma target 2.0

        sampler2D _MainTex;
        fixed4 _Color;
        fixed4 _BaseColor;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 col = (_BaseColor.a > 0.01) ? _BaseColor : _Color;
            fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = col.rgb * tex.rgb;
            o.Alpha = col.a * tex.a;
        }
        ENDCG
    }
    FallBack "Transparent/Diffuse"
}
