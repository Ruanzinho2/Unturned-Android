Shader "Custom/Water_Fallback"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.21, 0.74, 1.0, 0.65)
        _Color ("Main Color", Color) = (0.21, 0.74, 1.0, 0.65)
        _SpecularColor ("Specular Color", Color) = (0.9, 0.9, 0.9, 1.0)
        _SpecColor ("Spec Color", Color) = (0.9, 0.9, 0.9, 1.0)
        _ReflectionColor ("Reflection Color", Color) = (0.35, 0.7, 0.95, 1.0)
        _Shininess ("Shininess", Range (1.0, 128.0)) = 32.0
        _WaveSpeed ("Wave Speed", Range (0.0, 5.0)) = 0.6
        _WaveScale ("Wave Scale", Range (0.1, 50.0)) = 8.0
        _WaveIntensity ("Wave Intensity", Range (0.0, 1.0)) = 0.12
        _FresnelPower ("Fresnel Power", Range (0.5, 5.0)) = 2.5
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent-10"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        LOD 200
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        ZTest LEqual

        Pass
        {
            Tags { "LightMode" = "ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_fwdbase
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                UNITY_FOG_COORDS(2)
            };

            fixed4 _BaseColor;
            fixed4 _Color;
            fixed4 _SpecularColor;
            fixed4 _ReflectionColor;
            half _Shininess;
            half _WaveSpeed;
            half _WaveScale;
            half _WaveIntensity;
            half _FresnelPower;

            v2f vert(appdata v)
            {
                v2f o;
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.worldPos = worldPos.xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.pos = UnityWorldToClipPos(worldPos);
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Direction vectors
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);

                // Base color fallback
                fixed4 col = (_BaseColor.a > 0.01) ? _BaseColor : _Color;
                if (col.a < 0.01) col.a = 0.65;

                // Specular & reflection fallback
                fixed3 specCol = (_SpecularColor.a > 0.01) ? _SpecularColor.rgb : _SpecColor.rgb;
                fixed3 reflCol = (_ReflectionColor.a > 0.01) ? _ReflectionColor.rgb : (col.rgb * 1.3);

                // Procedural wave normal calculation in world space (seamless across tiles)
                float t = _Time.y * _WaveSpeed;
                float2 uv1 = i.worldPos.xz * (_WaveScale * 0.04) + float2(t * 0.35, t * 0.25);
                float2 uv2 = i.worldPos.xz * (_WaveScale * 0.07) - float2(t * 0.2, -t * 0.3);

                float w1 = sin(uv1.x + uv1.y * 1.2);
                float w2 = cos(uv2.x * 1.1 - uv2.y);
                float3 waveNormal = normalize(float3((w1 + w2) * _WaveIntensity, 1.0, (w1 - w2) * _WaveIntensity));

                // Two-sided shading: flip normal if viewed from underwater
                float3 normal = (dot(waveNormal, viewDir) >= 0.0) ? waveNormal : -waveNormal;

                // Direct lighting & ambient
                float NdotL = max(0.0, dot(normal, lightDir));
                float3 ambient = ShadeSH9(float4(normal, 1.0));
                float3 lightContribution = _LightColor0.rgb * NdotL + ambient;

                // Blinn-Phong specular highlight from the sun / moon
                float3 halfDir = normalize(lightDir + viewDir);
                float NdotH = max(0.0, dot(normal, halfDir));
                float specPower = clamp(_Shininess, 2.0, 128.0);
                float spec = pow(NdotH, specPower);
                float3 specular = spec * specCol * _LightColor0.rgb;

                // Fresnel reflection approximation
                float NdotV = max(0.0, dot(normal, viewDir));
                float fresnel = pow(1.0 - NdotV, clamp(_FresnelPower, 1.0, 5.0));

                // Combine base water tint with horizon reflection
                float3 waterRGB = lerp(col.rgb, reflCol, fresnel * 0.45);
                float3 finalRGB = (waterRGB * lightContribution) + specular;
                float finalAlpha = saturate(col.a + fresnel * 0.25);

                fixed4 result = fixed4(finalRGB, finalAlpha);
                UNITY_APPLY_FOG(i.fogCoord, result);
                return result;
            }
            ENDCG
        }
    }
    FallBack "Transparent/Diffuse"
}
