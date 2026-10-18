Shader "Custom/Water_Fallback"
{
    Properties
    {
        [Header(Water Colors)]
        _BaseColor ("Base Color (Shallow)", Color) = (0.08, 0.55, 0.70, 0.65)
        _DeepColor ("Deep Color", Color) = (0.02, 0.18, 0.28, 0.75)
        _Color ("Main Color (Fallback)", Color) = (0.08, 0.55, 0.70, 0.65)
        _ReflectionColor ("Reflection Tint", Color) = (0.60, 0.85, 1.0, 0.45)
        _SpecularColor ("Specular Color", Color) = (1.0, 1.0, 1.0, 0.65)
        _WorldLightDir ("Specular Light Dir", Vector) = (0.3, 0.9, -0.2, 0.0)
        _Shininess ("Shininess", Range (4.0, 256.0)) = 53.0

        [Header(Fresnel)]
        _FresnelScale ("Fresnel Scale", Range (0.0, 4.0)) = 1.6
        _FresnelPower ("Fresnel Power", Range (0.5, 8.0)) = 3.2
        _FresnelBias ("Fresnel Bias", Range (0.0, 1.5)) = 0.06

        [Header(Procedural Ripples)]
        _RippleScale ("Ripple Scale", Range (0.05, 20.0)) = 2.5
        _RippleSpeed ("Ripple Speed", Range (0.0, 5.0)) = 1.0
        _RippleStrength ("Ripple Normal Strength", Range (0.0, 1.0)) = 0.55

        [Header(Foam Settings)]
        _FoamColor ("Foam Color", Color) = (0.92, 0.97, 1.0, 0.90)
        _Foam ("Legacy Foam Color", Color) = (0.92, 0.97, 1.0, 0.90)
        _FoamIntensity ("Foam Intensity", Range (0.0, 3.0)) = 1.0
        _FoamNoiseScale ("Foam Noise Scale", Range (0.05, 20.0)) = 2.0
        _FoamNoiseSpeed ("Foam Noise Speed", Range (0.0, 5.0)) = 1.1
        _FoamCrestThreshold ("Crest Foam Threshold", Range (0.0, 1.0)) = 0.3
        _FoamCrestIntensity ("Crest Foam Intensity", Range (0.0, 3.0)) = 1.0
        _EdgeFoamIntensity ("Edge Foam Intensity", Range (0.0, 3.0)) = 1.2
        _EdgeFoamThreshold ("Edge Foam Threshold", Range (0.001, 1.0)) = 0.08

        [Header(Gerstner Waves)]
        _GerstnerIntensity ("Gerstner Intensity", Range (0.0, 3.0)) = 0.5
        _GAmplitude ("Wave Amplitude", Vector) = (0.22, 0.18, 0.14, 0.10)
        _GFrequency ("Wave Frequency", Vector) = (1.25, 1.65, 1.10, 0.85)
        _GSteepness ("Wave Steepness", Vector) = (0.85, 0.75, 0.65, 0.60)
        _GSpeed ("Wave Speed", Vector) = (1.20, 1.45, 1.05, 0.80)
        _GDirectionAB ("Wave Direction AB", Vector) = (0.25, 0.85, 0.90, 0.20)
        _GDirectionCD ("Wave Direction CD", Vector) = (0.10, 0.95, 0.55, 0.55)

        [Header(Quality Settings)]
        _WaterQualityLevel ("Water Quality Level (0=Low, 1=Med, 2=High, 3=Ultra)", Float) = 2.0
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
            #pragma multi_compile _ _WATER_QUALITY_LOW _WATER_QUALITY_MEDIUM _WATER_QUALITY_HIGH _WATER_QUALITY_ULTRA
            #pragma target 3.0

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
                float4 screenPos : TEXCOORD2;
                float waveDispY : TEXCOORD3;
                UNITY_FOG_COORDS(4)
            };

            fixed4 _BaseColor;
            fixed4 _DeepColor;
            fixed4 _Color;
            fixed4 _ReflectionColor;
            fixed4 _SpecularColor;
            float4 _WorldLightDir;
            half _Shininess;

            half _FresnelScale;
            half _FresnelPower;
            half _FresnelBias;

            half _RippleScale;
            half _RippleSpeed;
            half _RippleStrength;

            fixed4 _FoamColor;
            fixed4 _Foam;
            half _FoamIntensity;
            half _FoamNoiseScale;
            half _FoamNoiseSpeed;
            half _FoamCrestThreshold;
            half _FoamCrestIntensity;
            half _EdgeFoamIntensity;
            half _EdgeFoamThreshold;

            half _GerstnerIntensity;
            float4 _GAmplitude;
            float4 _GFrequency;
            float4 _GSteepness;
            float4 _GSpeed;
            float4 _GDirectionAB;
            float4 _GDirectionCD;

            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

            void EvaluateGerstnerWaves4(
                float3 worldPos,
                out float3 waveDisp,
                out float3 waveNormal)
            {
                float2 p = worldPos.xz;

                // Normalize wave direction vectors with safe fallbacks
                float lAB_xy = max(0.0001, length(_GDirectionAB.xy));
                float lAB_zw = max(0.0001, length(_GDirectionAB.zw));
                float lCD_xy = max(0.0001, length(_GDirectionCD.xy));
                float lCD_zw = max(0.0001, length(_GDirectionCD.zw));

                float2 d0 = _GDirectionAB.xy / lAB_xy;
                float2 d1 = _GDirectionAB.zw / lAB_zw;
                float2 d2 = _GDirectionCD.xy / lCD_xy;
                float2 d3 = _GDirectionCD.zw / lCD_zw;

                float4 freq = max(_GFrequency, float4(0.0001, 0.0001, 0.0001, 0.0001));
                float4 speed = freq * _GSpeed * _Time.y;

                float4 dotP = float4(dot(d0, p), dot(d1, p), dot(d2, p), dot(d3, p));
                float4 phase = freq * dotP + speed;

                float4 sinP = sin(phase);
                float4 cosP = cos(phase);

                float4 QA = _GAmplitude * _GSteepness;

                // Horizontal & vertical displacement
                float dx = dot(QA, float4(d0.x * cosP.x, d1.x * cosP.y, d2.x * cosP.z, d3.x * cosP.w));
                float dz = dot(QA, float4(d0.y * cosP.x, d1.y * cosP.y, d2.y * cosP.z, d3.y * cosP.w));
                float dy = dot(_GAmplitude, sinP);

                waveDisp = float3(dx, dy, dz) * _GerstnerIntensity;

                // Analytical wave normal
                float4 WA = freq * _GAmplitude;
                float dNx = dot(WA, float4(d0.x * cosP.x, d1.x * cosP.y, d2.x * cosP.z, d3.x * cosP.w));
                float dNz = dot(WA, float4(d0.y * cosP.x, d1.y * cosP.y, d2.y * cosP.z, d3.y * cosP.w));
                float dNy = dot(QA * freq, sinP);

                waveNormal = normalize(float3(-dNx, 1.0 - dNy, -dNz));
            }

            void EvaluateGerstnerWaves2(
                float3 worldPos,
                out float3 waveDisp,
                out float3 waveNormal)
            {
                float2 p = worldPos.xz;

                float lAB_xy = max(0.0001, length(_GDirectionAB.xy));
                float lAB_zw = max(0.0001, length(_GDirectionAB.zw));

                float2 d0 = _GDirectionAB.xy / lAB_xy;
                float2 d1 = _GDirectionAB.zw / lAB_zw;

                float2 freq = max(_GFrequency.xy, float2(0.0001, 0.0001));
                float2 speed = freq * _GSpeed.xy * _Time.y;

                float2 dotP = float2(dot(d0, p), dot(d1, p));
                float2 phase = freq * dotP + speed;

                float2 sinP = sin(phase);
                float2 cosP = cos(phase);

                float2 QA = _GAmplitude.xy * _GSteepness.xy;

                float dx = dot(QA, float2(d0.x * cosP.x, d1.x * cosP.y));
                float dz = dot(QA, float2(d0.y * cosP.x, d1.y * cosP.y));
                float dy = dot(_GAmplitude.xy, sinP);

                waveDisp = float3(dx, dy, dz) * _GerstnerIntensity;

                float2 WA = freq * _GAmplitude.xy;
                float dNx = dot(WA, float2(d0.x * cosP.x, d1.x * cosP.y));
                float dNz = dot(WA, float2(d0.y * cosP.x, d1.y * cosP.y));
                float dNy = dot(QA * freq, sinP);

                waveNormal = normalize(float3(-dNx, 1.0 - dNy, -dNz));
            }

            v2f vert(appdata v)
            {
                v2f o;
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);

                float3 waveDisp;
                float3 waveNormal;

                #if defined(_WATER_QUALITY_LOW)
                // Low quality: completely flat plane, zero trigonometry in vertex shader
                waveDisp = float3(0.0, 0.0, 0.0);
                waveNormal = float3(0.0, 1.0, 0.0);
                #elif defined(_WATER_QUALITY_MEDIUM)
                // Medium quality: 2 Gerstner waves (half the trigonometry)
                EvaluateGerstnerWaves2(worldPos.xyz, waveDisp, waveNormal);
                #else
                // High / Ultra / Default: 4 Gerstner waves
                EvaluateGerstnerWaves4(worldPos.xyz, waveDisp, waveNormal);
                #endif

                worldPos.xyz += waveDisp;
                o.worldPos = worldPos.xyz;
                o.worldNormal = waveNormal;
                o.waveDispY = waveDisp.y;
                o.pos = UnityWorldToClipPos(worldPos);
                o.screenPos = ComputeScreenPos(o.pos);
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

                // Light direction (from LevelLighting or sun/moon)
                float3 lightDir = normalize(_WorldLightDir.xyz);
                if (dot(lightDir, lightDir) < 0.01)
                {
                    lightDir = normalize(float3(0.35, 0.85, -0.25));
                }

                fixed4 baseCol = (_BaseColor.a > 0.01) ? _BaseColor : _Color;
                if (baseCol.a < 0.01) baseCol.a = 0.65;

                #if defined(_WATER_QUALITY_LOW)
                // ==================== LOW QUALITY ====================
                // Zero noise sampling, zero ripples, direct Blinn-Phong lighting
                float3 normal = (dot(float3(0.0, 1.0, 0.0), viewDir) >= 0.0) ? float3(0.0, 1.0, 0.0) : float3(0.0, -1.0, 0.0);
                float3 halfDir = normalize(lightDir + viewDir);
                float NdotH = max(0.0, dot(normal, halfDir));
                float spec = pow(NdotH, clamp(_Shininess, 4.0, 128.0));
                fixed3 specCol = (_SpecularColor.a > 0.01) ? _SpecularColor.rgb : fixed3(1.0, 1.0, 1.0);

                float NdotL = max(0.0, dot(normal, lightDir));
                float3 litWater = baseCol.rgb * (0.50 + 0.50 * NdotL) + spec * specCol * _SpecularColor.a * 0.45;
                float finalAlpha = max(baseCol.a, 0.70);

                fixed4 result = fixed4(litWater, finalAlpha);
                UNITY_APPLY_FOG(i.fogCoord, result);
                return result;

                #elif defined(_WATER_QUALITY_MEDIUM)
                // ==================== MEDIUM QUALITY ====================
                // Single-octave ripple, fast quadratic Fresnel, smoothstep crest foam
                float t = _Time.y * _RippleSpeed;
                float2 uv1 = i.worldPos.xz * (_RippleScale * 0.4) + float2(t * 0.35, t * 0.25);
                float w1 = sin(uv1.x + uv1.y * 1.2);
                float3 rippleNormal = normalize(float3(w1 * _RippleStrength * 0.7, 1.0, w1 * _RippleStrength * 0.7));
                float3 normal = normalize(float3(i.worldNormal.xz + rippleNormal.xz * 0.5, i.worldNormal.y));
                normal = (dot(normal, viewDir) >= 0.0) ? normal : -normal;

                float3 halfDir = normalize(lightDir + viewDir);
                float NdotH = max(0.0, dot(normal, halfDir));
                float spec = pow(NdotH, clamp(_Shininess, 4.0, 128.0));
                fixed3 specCol = (_SpecularColor.a > 0.01) ? _SpecularColor.rgb : fixed3(1.0, 1.0, 1.0);
                float3 specular = spec * specCol * _SpecularColor.a;

                float NdotV = max(0.0, dot(normal, viewDir));
                float oneMinusV = 1.0 - NdotV;
                float fresnel = clamp(_FresnelBias + _FresnelScale * (oneMinusV * oneMinusV), 0.0, 1.0);

                fixed3 deepCol = (_DeepColor.a > 0.01) ? _DeepColor.rgb : (baseCol.rgb * 0.45);
                fixed3 reflCol = (_ReflectionColor.a > 0.01) ? _ReflectionColor.rgb : (baseCol.rgb * 1.35);

                float NdotL = max(0.0, dot(normal, lightDir));
                float lightIntensity = 0.45 + 0.55 * NdotL;
                float3 shallowColor = baseCol.rgb * lightIntensity;
                float3 waterBody = lerp(deepCol * lightIntensity, shallowColor, 0.75);
                float3 litWater = lerp(waterBody, reflCol, fresnel * 0.50) + specular;

                float crestFactor = saturate((i.waveDispY - _FoamCrestThreshold) / max(0.01, 1.0 - _FoamCrestThreshold));
                float crestFoam = crestFactor * _FoamCrestIntensity * 0.75;
                fixed4 foamCol = (_FoamColor.a > 0.01) ? _FoamColor : ((_Foam.a > 0.01) ? _Foam : fixed4(0.92, 0.97, 1.0, 0.90));
                float totalFoam = saturate(crestFoam * _FoamIntensity);

                float3 finalRGB = lerp(litWater, foamCol.rgb, totalFoam);
                float alphaBase = max(baseCol.a, 0.65);
                float finalAlpha = saturate(alphaBase + fresnel * 0.20 + totalFoam * 0.35);

                fixed4 result = fixed4(finalRGB, finalAlpha);
                UNITY_APPLY_FOG(i.fogCoord, result);
                return result;

                #else
                // ==================== HIGH & ULTRA QUALITY ====================
                // Dual-octave ripples, full Fresnel power, procedural noise crest foam
                float t = _Time.y * _RippleSpeed;
                float2 uv1 = i.worldPos.xz * (_RippleScale * 0.4) + float2(t * 0.35, t * 0.25);
                float2 uv2 = i.worldPos.xz * (_RippleScale * 0.7) - float2(t * 0.20, -t * 0.30);

                float w1 = sin(uv1.x + uv1.y * 1.2);
                float w2 = cos(uv2.x * 1.1 - uv2.y);
                float3 rippleNormal = normalize(float3((w1 + w2) * _RippleStrength, 1.0, (w1 - w2) * _RippleStrength));

                float3 normal = normalize(float3(i.worldNormal.xz + rippleNormal.xz * 0.5, i.worldNormal.y));
                normal = (dot(normal, viewDir) >= 0.0) ? normal : -normal;

                float3 halfDir = normalize(lightDir + viewDir);
                float NdotH = max(0.0, dot(normal, halfDir));
                float shininess = clamp(_Shininess, 4.0, 256.0);
                float spec = pow(NdotH, shininess);

                fixed3 specCol = (_SpecularColor.a > 0.01) ? _SpecularColor.rgb : fixed3(1.0, 1.0, 1.0);
                float3 specular = spec * specCol * _SpecularColor.a;

                float NdotV = max(0.0, dot(normal, viewDir));
                float fresnel = clamp(_FresnelBias + _FresnelScale * pow(1.0 - NdotV, clamp(_FresnelPower, 0.5, 6.0)), 0.0, 1.0);

                fixed3 deepCol = (_DeepColor.a > 0.01) ? _DeepColor.rgb : (baseCol.rgb * 0.45);
                fixed3 reflCol = (_ReflectionColor.a > 0.01) ? _ReflectionColor.rgb : (baseCol.rgb * 1.35);

                float NdotL = max(0.0, dot(normal, lightDir));
                float lightIntensity = 0.45 + 0.55 * NdotL;

                float3 shallowColor = baseCol.rgb * lightIntensity;
                float3 waterBody = lerp(deepCol * lightIntensity, shallowColor, 0.75);
                float3 litWater = lerp(waterBody, reflCol, fresnel * 0.60) + specular;

                float crestFactor = saturate((i.waveDispY - _FoamCrestThreshold) / max(0.01, 1.0 - _FoamCrestThreshold));
                float2 noiseUV = i.worldPos.xz * _FoamNoiseScale + _Time.y * _FoamNoiseSpeed * float2(0.2, 0.15);
                float noise = frac(sin(dot(noiseUV, float2(12.9898, 78.233))) * 43758.5453);
                float crestFoam = pow(crestFactor, 1.4) * _FoamCrestIntensity * saturate(noise + 0.4);

                float shoreFoam = 0.0;
                #if defined(_WATER_QUALITY_ULTRA) && (defined(SOFTPARTICLES_ON) || defined(_CAMERA_DEPTH_ON)) && !defined(SHADER_API_MOBILE) && !defined(SHADER_API_VULKAN) && !defined(SHADER_API_GLES3)
                float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)));
                float partZ = i.screenPos.w;
                float depthDiff = sceneZ - partZ;
                if (depthDiff > 0.0 && depthDiff < (_EdgeFoamThreshold * 20.0))
                {
                    float edgeT = saturate(1.0 - (depthDiff / (_EdgeFoamThreshold * 20.0)));
                    shoreFoam = edgeT * _EdgeFoamIntensity * saturate(noise + 0.5);
                }
                #endif

                fixed4 foamCol = (_FoamColor.a > 0.01) ? _FoamColor : ((_Foam.a > 0.01) ? _Foam : fixed4(0.92, 0.97, 1.0, 0.90));
                float totalFoam = saturate((crestFoam + shoreFoam) * _FoamIntensity);

                float3 finalRGB = lerp(litWater, foamCol.rgb, totalFoam);
                float alphaBase = max(baseCol.a, 0.65);
                float finalAlpha = saturate(alphaBase + fresnel * 0.25 + totalFoam * 0.40);

                fixed4 result = fixed4(finalRGB, finalAlpha);
                UNITY_APPLY_FOG(i.fogCoord, result);
                return result;
                #endif
            }
            ENDCG
        }
    }
    FallBack "Transparent/Diffuse"
}
