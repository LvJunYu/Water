#ifndef WaterCaustics
#define WaterCaustics

float3 ReconstructWorldPos(half2 screenPos, float depth)
{
	#if defined(SHADER_API_GLCORE) || defined (SHADER_API_GLES) || defined (SHADER_API_GLES3)			// OpenGL平台 //
        depth = depth * 2 - 1;
    #endif
    float4 raw = mul(UNITY_MATRIX_I_VP, float4(screenPos * 2 - 1, depth, 1));
    float3 worldPos = raw.rgb / raw.a;
    return worldPos;
}

// Can be done per-vertex
float2 CausticUVs(float2 rawUV, float2 offset)
{
    //anim
    float2 uv = rawUV * _CausticsSize + float2(_Time.y, _Time.x) * 0.1;
    return uv + offset * 0.25;
}

half3 Caustics(float3 worldPos)
{
    float2 uv = worldPos.xz * 0.025 + _Time.x * 0.25;
    float waveOffset = SAMPLE_TEXTURE2D(_CausticMap, sampler_ScreenTextures_linear_repeat, uv).w - 0.5;
    float2 causticUV = CausticUVs(worldPos.xz, waveOffset);
    half upperMask = saturate(-worldPos.y + _WaterLevel + _CausticsOffset);
    half lowerMask = saturate((worldPos.y - _WaterLevel - _CausticsOffset) / _CausticsBlendDistance + _CausticsBlendDistance);
    float3 caustics = SAMPLE_TEXTURE2D_LOD(_CausticMap, sampler_ScreenTextures_linear_repeat, causticUV, abs(worldPos.y - _WaterLevel - _CausticsOffset) * 5 / _CausticsBlendDistance).bbb;
    caustics *= _CausticsIntensity;
#ifdef _DEBUG
    return caustics * min(upperMask, lowerMask);
#endif
    caustics *= min(upperMask, lowerMask) * 2;
    return caustics + 1;
}
    
half3 Caustics(float2 screenUV, float depth)
{
    float3 worldPos = ReconstructWorldPos(screenUV, depth);
    return Caustics(worldPos);
}

#endif // WaterCaustics