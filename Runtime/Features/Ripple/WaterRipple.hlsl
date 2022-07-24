#ifndef WaterRipple
#define WaterRipple

half4 rippleData[15];
#include "WaterInput.hlsl"

float trochoids_approx(float v) 
{ 
	float A = 1.0;
	return -A + 2.0 * A * pow(1.0 - pow(0.5 + 0.5 * sin(v), A + 1.0), 1.0 / (A + 1.0));
}

float wave(float2 position, float2 origin, float time)
{
	float t = time * _RippleSpeed;
	if(t > _RippleLife) return 0;
	float y = 0.0;
	float2 dir = position - origin;
    float d = length(dir);
    #if _CustomWave
        float sample = SAMPLE_TEXTURE2D(_RippleMap, sampler_ScreenTextures_linear_clamp, float2(t - d * _RippleFrequency, 0)).a;
        y = sample * 2 - 1;
    #else
        float sample2 = SAMPLE_TEXTURE2D(_RippleMap, sampler_ScreenTextures_linear_clamp, float2(d / _RippleFactor3, t / _RippleLife)).r;
	    y = lerp(_RippleHeightMin, _RippleHeightMax, sample2);
	#endif

	float atten = 1.0 - smoothstep(_RippleLife * 0.5, _RippleLife, t);
	// atten *= saturate(time * 5);
	float disAtten = pow(saturate(1 - d / _RippleFactor3), _RippleFactor5);
	atten *= disAtten;
	return y * atten;
}

float allwave(float2 position, half noise)
{
    float height = 0;
    UNITY_LOOP
	for(uint i = 0; i < _RippleCount; i++)
	{
	    float4 data = rippleData[i];
	    height += wave(position, data.xy, data.z);
	}
	
	height = lerp(height, 0, noise * _RippleFactor2) * _RippleIntensity;
    return height;
}

float3 RippleNormal(float3 posWS, float3 normal)
{
    half noise = SAMPLE_TEXTURE2D(_RippleNoiseMap, sampler_ScreenTextures_linear_repeat, (posWS.xz + _Time.y * _RippleFactor4) * _RippleFactor1).r;
    float height = allwave(posWS.xz, noise);
	float2 deltaX = float2(0.01f, 0);
    float2 deltaY = float2(0, 0.01f);
    float heightX = allwave(posWS.xz + deltaX, noise);
    float heightY = allwave(posWS.xz + deltaY, noise);
	float3 ddxPos = float3(0.01f, heightX - height, 0);
	float3 ddyPos = float3(0, heightY - height, 0.01f);
    //float3 wavePos = float3(posWS.x, height, posWS.z);
	//float3 ddxPos = ddx(wavePos);
	//float3 ddyPos = ddy(wavePos);
    float3 rippleNormal = normalize(cross(ddyPos, ddxPos));
    //float rim = clamp(0.0, 2.0, 1.0 / (_RippleFactor5 * 2.0 + abs(height)));
	//rippleFac = saturate(pow(saturate(length(float2(ddxPos.y, ddyPos.y)) * 5 * _RippleFactor6), _RippleFactor7 * 8.0) * rim);
    return rippleNormal;
}

#endif