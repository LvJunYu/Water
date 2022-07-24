#ifndef WATER_FOAM_INCLUDED
#define WATER_FOAM_INCLUDED

half JacobianFoam(float2 foamUv, float noise, float2 offset)
{
    float2 dDdx = -ddx(offset) * 0.5;
    float2 dDdy = -ddy(offset) * 0.5;
    float jacobian = (1 + dDdx.x) * (1 + dDdy.y) - dDdx.y * dDdy.x;
    jacobian = max(1 - jacobian, 0);
    //return jacobian * 10;
    float foamMask = jacobian * 10;
    half3 foamBlend = SAMPLE_TEXTURE2D(_AbsorptionScatteringRamp, sampler_ScreenTextures_linear_clamp,
                                       half2(foamMask, 0.66)).rgb; //对ramp采样
    half3 foamMap = SAMPLE_TEXTURE2D(_FoamMap, sampler_ScreenTextures_linear_repeat, foamUv).rgb;
    //r=thick, g=medium, b=light
    foamMask = dot(foamMap, foamBlend); // 混合泡沫贴图
    return foamMask * 2;
}

half SeaFoam(float2 foamUv, float depth, float height, float waveHeight, float noise)
{
    half3 foamMap = SAMPLE_TEXTURE2D(_FoamMap, sampler_ScreenTextures_linear_repeat, foamUv).rgb;
    //r=thick, g=medium, b=light
    half shoreMask = pow(saturate((1 - height + 9) * 0.1), 6); //水垂直深度浅的地方有泡沫
    half foamMask = waveHeight; //海浪normolize高度，模拟浪尖泡沫
    // 根据视角深度对泡沫的影响，并用sin使泡沫往岸边滚动
    half shoreWave = (sin(_Time.z + (height * 10) + noise) * 0.5 + 0.5) * saturate((1 - depth) + 1);
    foamMask = max(max((foamMask + shoreMask) - noise * 0.25, 0), shoreWave); // 取上门两个最大值
    half3 foamBlend = SAMPLE_TEXTURE2D(_AbsorptionScatteringRamp, sampler_ScreenTextures_linear_clamp,
                                       half2(foamMask, 0.66)).rgb; //对ramp采样
    foamMask = length(foamMap * foamBlend); // 混合泡沫贴图
    half foam = foamMask;
    foam *= saturate(depth * 5);
    return foam;
}

half RiverFoam(float2 foamUv, float depth)
{
    half4 foamMap = SAMPLE_TEXTURE2D(_FoamMap, sampler_ScreenTextures_linear_repeat, foamUv * _FoamFactor2 * 5);
    half edgeMask = 1 - saturate(depth * _FoamFactor5 * 5);
    half min = lerp(1 - _FoamFactor4, 0, edgeMask); //靠近岸边变粗
    half noise = lerp(foamMap.r * _FoamFactor7 * 20, 0, edgeMask); // 靠近岸边扰动下降
    half mask = sin((_Time.z * _FoamFactor8 * 5) + (depth * _FoamFactor1 * 10) + noise) * 0.5 + 0.5; //泡沫速度和频率
    mask = smoothstep(min, 1, mask); //泡沫粗细
    mask *= 1 - saturate(depth * _FoamFactor3); //泡沫范围

    half edge = _FoamFactor6; //岸边
    if (edgeMask > _FoamFactor10)
    {
        mask = edge;
    }
    //mask = lerp(mask, edge,  edgeMask);
    return mask;
}

#endif
