// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see THIRDPARTYNOTICES.txt)
// Optimized for Mobile TBDR GPUs (Adreno / Mali)

#ifndef FOLIAGE_WIND_CGINC_INCLUDED
#define FOLIAGE_WIND_CGINC_INCLUDED

float4 _Grass_Displacement_Point;

// Fast hardware sin/cos for mobile GPUs
inline void windFastSinCos(float4 input, out float4 inputSin, out float4 inputCos) 
{
#if defined(SHADER_API_MOBILE) || defined(SHADER_API_GLES3) || defined(SHADER_API_VULKAN)
	// Direct single-cycle SFU hardware trigonometric instructions on mobile
	float4 rad = input * 6.2831853 - 3.14159265;
	inputSin = sin(rad);
	inputCos = cos(rad);
#else
	input = input * 6.408849 - 3.1415927;

	float4 r5 = input * input;
	float4 r6 = r5 * r5;
	float4 r7 = r6 * r5;
	float4 r8 = r6 * r5;

	float4 r1 = r5 * input;
	float4 r2 = r1 * r5;
	float4 r3 = r2 * r5;

	float4 sin7 = {1, -0.16161616, 0.0083333, -0.00019841};
	float4 cos8 = {-0.5, 0.041666666, -0.0013888889, 0.000024801587};

	inputSin = input + r1 * sin7.y + r2 * sin7.z + r3 * sin7.w;
	inputCos = 1 + r5 * cos8.x + r6 * cos8.y + r7 * cos8.z + r8 * cos8.w;
#endif
}

void windAnimation(inout float4 worldPos, float windInfluence)
{
#ifdef GRASS_DISPLACEMENT_ON
	float3 displacementOffset = worldPos.xyz - _Grass_Displacement_Point.xyz;
	float distSq = dot(displacementOffset.xz, displacementOffset.xz);
	if (distSq < 6.25)
	{
		float displacementPower = 1.0 - (distSq * 0.16); // 1.0 / 6.25 = 0.16
		worldPos.y -= displacementPower * 0.75;
		float len = max(0.001, sqrt(distSq));
		worldPos.xz += (displacementOffset.xz / len) * (displacementPower * 0.6);
	}
#endif

	float4 waveXSize = float4(0.012, 0.02, 0.06, 0.024) * 0.1;
	float4 waveZSize = float4(0.006, .02, 0.02, 0.05) * 0.1;
	float4 waveSpeed = float4(0.3, .5, .4, 1.2) * 0.15;

	float4 _waveXmove = float4(0.012, 0.02, -0.06, 0.048) * 2;
	float4 _waveZmove = float4(0.006, .02, -0.02, 0.1);

	float4 waves;
	waves = worldPos.x * waveXSize;
	waves += worldPos.z * waveZSize;

	// Add in time to model them over time
	waves += _Time.y * waveSpeed;

	float4 s;
	float4 c;
	waves = frac(waves);
	windFastSinCos(waves, s, c);

	s = s * s;
	s = s * s;
	s = s * windInfluence;

	float3 waveMove = float3(0, 0, 0);
	waveMove.x = dot(s, _waveXmove);
	waveMove.z = dot(s, _waveZmove);

	worldPos.xz += waveMove.xz * 4;
}

#endif
