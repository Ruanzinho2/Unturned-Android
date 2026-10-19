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

                float4 steepness = clamp(_GSteepness, 0.0, 1.0);
                float4 QA = _GAmplitude * steepness;
                // Bound total wave steepness to 0.75 to eliminate self-intersecting loops and pinching
                float sumSteepness = dot(QA, freq);
                if (sumSteepness > 0.75)
                {
                    QA *= (0.75 / sumSteepness);
                }

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

                float2 steepness = clamp(_GSteepness.xy, 0.0, 1.0);
                float2 QA = _GAmplitude.xy * steepness;
                float sumSteepness2 = dot(QA, freq);
                if (sumSteepness2 > 0.75)
                {
                    QA *= (0.75 / sumSteepness2);
                }

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
                #if defined(_WATER_QUALITY_LOW)
                // Low quality: completely flat plane, zero trigonometry in vertex shader
                waveDisp = float3(0.0, 0.0, 0.0);
                #elif defined(_WATER_QUALITY_MEDIUM)
                // Medium quality: Smooth directional ocean swell (avoids 64m mesh aliasing and product checkerboards)
                float swell = (sin(worldPos.x * 0.032 + worldPos.z * 0.018 + _Time.y * 0.80) + cos(worldPos.x * 0.018 - worldPos.z * 0.026 + _Time.y * 0.60)) * (_GerstnerIntensity * 0.20);
                waveDisp = float3(0.0, swell, 0.0);
                #else
                // High / Ultra: Smooth dual-harmonic ocean swell (80m-180m wavelengths)
                float sw1 = sin(worldPos.x * 0.035 + worldPos.z * 0.020 + _Time.y * 0.85);
                float sw2 = cos(worldPos.x * 0.022 - worldPos.z * 0.038 + _Time.y * 0.65);
                float swell = (sw1 + sw2) * (_GerstnerIntensity * 0.22);
                waveDisp = float3(0.0, swell, 0.0);
                #endif

                worldPos.xyz += waveDisp;
                o.worldPos = worldPos.xyz;
                o.worldNormal = float3(0.0, 1.0, 0.0);
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
                // LevelLighting passes sun.forward (points downward into ground).
                // For lighting calculations, lightDir must point UPWARDS towards the sky/sun!
                if (lightDir.y < 0.0)
                {
                    lightDir = -lightDir;
                }
                if (dot(lightDir, lightDir) < 0.01)
                {
                    lightDir = normalize(float3(0.35, 0.85, -0.25));
                }

                fixed4 baseCol = (_BaseColor.a > 0.01) ? _BaseColor : _Color;
                if (baseCol.a < 0.01) baseCol.a = 0.65;

                #if defined(_WATER_QUALITY_LOW)
                // ==================== LOW QUALITY ====================
                // Zero noise sampling, flat mirror, direct Blinn-Phong lighting
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
                // Directional wind wave trains per-pixel, cubemap reflection, organic specular column
                float2 p = i.worldPos.xz * (_RippleScale * 0.12);
                float t = _Time.y * _RippleSpeed;

                // 4 directional waves within wind cone (eliminates orthogonal checkerboards and circular domes)
                float4 dotP = float4(
                    p.x * 0.978 + p.y * 0.208,  // 12 deg
                    p.x * 0.848 + p.y * 0.530,  // 32 deg
                    p.x * 0.985 - p.y * 0.174,  // -10 deg
                    p.x * 0.669 + p.y * 0.743   // 48 deg
                );
                float4 freq = float4(1.0, 1.47, 2.13, 2.89);
                float4 speed = float4(1.15, -0.95, 1.35, -1.10) * t;
                float4 phase = dotP * freq + speed;
                float4 cosP = cos(phase);

                float4 amp = float4(0.26, 0.17, 0.11, 0.06) * (_RippleStrength * 0.22);
                float dNx = dot(amp * freq, float4( 0.978 * cosP.x,  0.848 * cosP.y,  0.985 * cosP.z,  0.669 * cosP.w));
                float dNz = dot(amp * freq, float4( 0.208 * cosP.x,  0.530 * cosP.y, -0.174 * cosP.z,  0.743 * cosP.w));

                float3 normal = normalize(float3(-dNx, 1.0, -dNz));
                if (dot(normal, viewDir) < 0.0)
                {
                    normal = -normal;
                }

                float NdotL = max(0.0, dot(normal, lightDir));
                float3 halfDir = normalize(lightDir + viewDir);
                float NdotH = max(0.0, dot(normal, halfDir));
                float spec = pow(NdotH, clamp(_Shininess, 20.0, 64.0)) * saturate(NdotL * 2.2);
                spec = spec * spec; // Natural quadratic specular falloff without circular hot spots
                fixed3 specCol = (_SpecularColor.a > 0.01) ? _SpecularColor.rgb : fixed3(1.0, 1.0, 1.0);
                float3 specular = spec * specCol * (_SpecularColor.a * 0.45);

                float NdotV = max(0.0, dot(normal, viewDir));
                float oneMinusV = 1.0 - NdotV;
                float fresnel = clamp(_FresnelBias + _FresnelScale * (oneMinusV * oneMinusV), 0.0, 1.0);

                fixed3 deepCol = (_DeepColor.a > 0.01) ? _DeepColor.rgb : (baseCol.rgb * 0.45);
                fixed3 reflCol = (_ReflectionColor.a > 0.01) ? _ReflectionColor.rgb : (baseCol.rgb * 1.35);

                // Sample cubemap reflection for sky / reflection probes (adapted from AleToonURP math)
                float3 reflDir = reflect(-viewDir, normal);
                half4 skyData = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflDir, 2.0);
                half3 skyColor = DecodeHDR(skyData, unity_SpecCube0_HDR);
                float skyBright = max(skyColor.r, max(skyColor.g, skyColor.b));
                if (skyBright > 0.05)
                {
                    reflCol = lerp(reflCol, reflCol * skyColor, 0.50);
                }

                float lightIntensity = 0.50 + 0.50 * NdotL;
                float3 shallowColor = baseCol.rgb * lightIntensity;
                float3 waterBody = lerp(deepCol * lightIntensity, shallowColor, 0.70);
                float3 litWater = lerp(waterBody, reflCol, fresnel * 0.45) + specular;

                float alphaBase = max(baseCol.a, 0.65);
                float finalAlpha = saturate(alphaBase + fresnel * 0.20);

                fixed4 result = fixed4(litWater, finalAlpha);
                UNITY_APPLY_FOG(i.fogCoord, result);
                return result;

                #else
                // ==================== HIGH & ULTRA QUALITY ====================
                // Directional analytical waves + capillary ripples + cubemap reflection (zero foam circles)
                float2 p = i.worldPos.xz * (_RippleScale * 0.12);
                float t = _Time.y * _RippleSpeed;

                // 4 directional waves within wind cone (eliminates orthogonal checkerboards and circular domes)
                float4 dotP = float4(
                    p.x * 0.978 + p.y * 0.208,  // 12 deg
                    p.x * 0.848 + p.y * 0.530,  // 32 deg
                    p.x * 0.985 - p.y * 0.174,  // -10 deg
                    p.x * 0.669 + p.y * 0.743   // 48 deg
                );
                float4 freq = float4(1.0, 1.47, 2.13, 2.89);
                float4 speed = float4(1.15, -0.95, 1.35, -1.10) * t;
                float4 phase = dotP * freq + speed;
                float4 cosP = cos(phase);

                float4 amp = float4(0.28, 0.18, 0.12, 0.07) * (_RippleStrength * 0.24);
                float dNx = dot(amp * freq, float4( 0.978 * cosP.x,  0.848 * cosP.y,  0.985 * cosP.z,  0.669 * cosP.w));
                float dNz = dot(amp * freq, float4( 0.208 * cosP.x,  0.530 * cosP.y, -0.174 * cosP.z,  0.743 * cosP.w));

                // Micro capillary ripples along wind flow
                float2 capUV = i.worldPos.xz * (_RippleScale * 0.45) + float2(t * 0.40, t * 0.15);
                float capW1 = sin(capUV.x * 1.6 + capUV.y * 0.7);
                float capW2 = cos(capUV.x * 0.9 - capUV.y * 1.4);
                float3 fineOffset = float3(-capW1 * (_RippleStrength * 0.04), 0.0, -capW2 * (_RippleStrength * 0.04));

                float3 normal = normalize(float3(-dNx, 1.0, -dNz) + fineOffset);
                if (dot(normal, viewDir) < 0.0)
                {
                    normal = -normal;
                }

                float NdotL = max(0.0, dot(normal, lightDir));
                float3 halfDir = normalize(lightDir + viewDir);
                float NdotH = max(0.0, dot(normal, halfDir));
                float shininess = clamp(_Shininess, 24.0, 72.0);
                float spec = pow(NdotH, shininess) * saturate(NdotL * 2.2);
                spec = spec * spec; // Natural quadratic specular falloff without circular hot spots

                fixed3 specCol = (_SpecularColor.a > 0.01) ? _SpecularColor.rgb : fixed3(1.0, 1.0, 1.0);
                float3 specular = spec * specCol * (_SpecularColor.a * 0.50);

                float NdotV = max(0.0, dot(normal, viewDir));
                float fresnel = clamp(_FresnelBias + _FresnelScale * pow(1.0 - NdotV, clamp(_FresnelPower, 0.5, 6.0)), 0.0, 1.0);

                fixed3 deepCol = (_DeepColor.a > 0.01) ? _DeepColor.rgb : (baseCol.rgb * 0.45);
                fixed3 reflCol = (_ReflectionColor.a > 0.01) ? _ReflectionColor.rgb : (baseCol.rgb * 1.35);

                // Sample cubemap reflection for sky / reflection probes (adapted from AleToonURP math)
                float3 reflDir = reflect(-viewDir, normal);
                half4 skyData = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflDir, 1.5);
                half3 skyColor = DecodeHDR(skyData, unity_SpecCube0_HDR);
                float skyBright = max(skyColor.r, max(skyColor.g, skyColor.b));
                if (skyBright > 0.05)
                {
                    reflCol = lerp(reflCol, reflCol * skyColor, 0.60);
                }

                float lightIntensity = 0.50 + 0.50 * NdotL;
                float3 shallowColor = baseCol.rgb * lightIntensity;
                float3 waterBody = lerp(deepCol * lightIntensity, shallowColor, 0.70);
                float3 litWater = lerp(waterBody, reflCol, fresnel * 0.50) + specular;

                float alphaBase = max(baseCol.a, 0.65);
                float finalAlpha = saturate(alphaBase + fresnel * 0.25);

                fixed4 result = fixed4(litWater, finalAlpha);
                UNITY_APPLY_FOG(i.fogCoord, result);
                return result;
                #endif
            }
            ENDCG
        }
    }
    FallBack "Transparent/Diffuse"
}
