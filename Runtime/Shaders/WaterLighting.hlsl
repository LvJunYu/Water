#ifndef WATER_LIGHTING_INCLUDED
#define WATER_LIGHTING_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

half3 Scattering(float depth)
{
    return SAMPLE_TEXTURE2D(_AbsorptionScatteringRamp, sampler_ScreenTextures_linear_clamp, half2(depth, 0.375)).rgb;
}

half3 Absorption(float depth)
{
    return SAMPLE_TEXTURE2D(_AbsorptionScatteringRamp, sampler_ScreenTextures_linear_clamp, half2(depth, 0.0)).rgb;
}

float2 AdjustedDepth(float rawD, float3 viewDir, float3 posVS)
{
    float d = LinearEyeDepth(rawD, _ZBufferParams);
    float depthPos = d * length(posVS / posVS.z); //海底距离相机的距离
    float waterPos = length(viewDir); //水面距离相机得距离
    return float2(depthPos - waterPos, (rawD * -_ProjectionParams.x) + (1 - UNITY_REVERSED_Z));
}

float2 AdjustedDepth(float2 uvs, float3 viewDir, float3 posVS)
{
    float rawD = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uvs);
    return AdjustedDepth(rawD, viewDir, posVS);
}

float WaterTextureDepth(float3 posWS)
{
    half2 depthUv = (posWS.xz - _WaterCenterPos.xz) * _DepthTexTiling * 0.5 + 0.5;
    return (1 - SAMPLE_TEXTURE2D_LOD(_SeaBedHeightMap, sampler_ScreenTextures_linear_clamp, depthUv, 1).r);
}

float3 WaterDepth(float3 posWS, float rawD, float3 viewDir, float3 posVS)
{
    float3 outDepth = 0;
    outDepth.xz = AdjustedDepth(rawD, viewDir, posVS);
    float wd = WaterTextureDepth(posWS) * 19.1;
    outDepth.y = (wd - 3.5) + posWS.y - _WaterLevel;
    return outDepth;
}

half3 Refraction(half2 distortion, float depth)
{
    half3 refrac = SAMPLE_TEXTURE2D_LOD(_CameraOpaqueTexture, sampler_ScreenTextures_linear_clamp, distortion,
                                        depth * 0.25).rgb;
    return refrac;
}

half2 DistortionUVs(half depth, float3 normalWS)
{
    half3 viewNormal = mul((float3x3)GetWorldToHClipMatrix(), -normalWS).xyz;

    return viewNormal.xz * saturate((depth) * 0.1) * 0.05;
}

half CalculateFresnelTerm(half3 normalWS, half3 viewDirectionWS)
{
    return pow(1.0 - saturate(dot(normalWS, viewDirectionWS)), _FresnelPower); //fresnel TODO - find a better place
}

half4 VertexLightingAndFog(half3 normalWS, half3 posWS, half3 clipPos)
{
    half3 vertexLight = VertexLighting(posWS, normalWS);
    half fogFactor = ComputeFogFactor(clipPos.z);
    return half4(fogFactor, vertexLight);
}

half3 SubSurfaceLighting(float3 viewDir, float3 lightDir, float sssIndensity, float shadow, half3 lightColor)
{
    float v = abs(viewDir.y);
    half towardsSun = pow(max(0., dot(lightDir, -viewDir)), _SubSurfaceSunFallOff);
    half subsurface = (_SubSurfaceBase + _SubSurfaceSun * towardsSun) * (1.0 - v) * sssIndensity * shadow;
    return subsurface * _SubSurfaceColor.rgb * lightColor;
}

half3 SampleReflections(half3 positionWS, half3 normalWS, half3 viewDirectionWS, half2 screenUV, half roughness,
                        float depth)
{
    half3 reflection = 0;
    half3 distortNormal = lerp(half3(0, 1, 0), normalWS, _ReflectionParam.y);
    half3 reflectVector = reflect(-viewDirectionWS, distortNormal);
    #if _REFLECTION_CUBEMAP
    reflection = SAMPLE_TEXTURECUBE(_CubemapTexture, sampler_CubemapTexture, reflectVector).rgb;
    #elif _REFLECTION_PROBES
    reflection = GlossyEnvironmentReflection(reflectVector, 0, 1);
    #elif _REFLECTION_PLANARREFLECTION
    half2 reflectionUV = screenUV + normalWS.zx * half2(0.1, 0.3) * _RelectDistort;
    reflection += SAMPLE_TEXTURE2D_LOD(_PlanarReflectionTexture, sampler_ScreenTextures_linear_clamp, reflectionUV, 6 * roughness).rgb;//planar reflection
    #elif _REFLECTION_SSPR
    half2 reflectionUV = screenUV + normalWS.zx * half2(0.1, 0.3) * _RelectDistort;
    half4 sample = SAMPLE_TEXTURE2D(_SSPlanarReflectionTexture, sampler_ScreenTextures_linear_clamp, reflectionUV);//planar reflection
	sample.rgb += GlossyEnvironmentReflection(reflectVector, 0, 1) * (1 - sample.a);
	// if(all(sample == 0))
    // {
    //        sample = GlossyEnvironmentReflection(reflectVector, 0, 1);
	// }
	reflection += sample.rgb;
    #elif _REFLECTION_TD_SSPR
        reflection += SampleTDSSPR(positionWS, normalWS, viewDirectionWS, depth);
    #else
    return 0;
    #endif

    return reflection;
}

half3 DirectSpecular(BRDFData brdfData, half3 normalWS, half3 lightDirectionWS, half3 viewDirectionWS, float HModifier)
{
    float3 halfDir = SafeNormalize(float3(lightDirectionWS)+float3(viewDirectionWS) + float3(0, HModifier, 0));
    float NoH = saturate(dot(normalWS, halfDir));

    half LoH = saturate(dot(lightDirectionWS, halfDir));

    // GGX Distribution multiplied by combined approximation of Visibility and Fresnel
    // BRDFspec = (D * V * F) / 4.0
    // D = roughness^2 / ( NoH^2 * (roughness^2 - 1) + 1 )^2
    // V * F = 1.0 / ( LoH^2 * (roughness + 0.5) )
    // See "Optimizing PBR for Mobile" from Siggraph 2015 moving mobile graphics course
    // https://community.arm.com/events/1155

    // Final BRDFspec = roughness^2 / ( NoH^2 * (roughness^2 - 1) + 1 )^2 * (LoH^2 * (roughness + 0.5) * 4.0)
    // We further optimize a few light invariant terms
    // brdfData.normalizationTerm = (roughness + 0.5) * 4.0 rewritten as roughness * 4.0 + 2.0 to a fit a MAD.
    float d = NoH * NoH * brdfData.roughness2MinusOne + 1.00001f;

    half LoH2 = LoH * LoH;
    half specularTerm = brdfData.roughness2 / ((d * d) * max(0.1h, LoH2) * brdfData.normalizationTerm);

    // On platforms where half actually means something, the denominator has a risk of overflow
    // clamp below was added specifically to "fix" that, but dx compiler (we convert bytecode to metal/gles)
    // sees that specularTerm have only non-negative terms, so it skips max(0,..) in clamp (leaving only min(100,...))
#if defined (SHADER_API_MOBILE) || defined (SHADER_API_SWITCH)
    specularTerm = specularTerm - HALF_MIN;
    specularTerm = clamp(specularTerm, 0.0, 100.0); // Prevent FP16 overflow on mobiles
#endif
    half3 specularColor = specularTerm * brdfData.specular;

    //DEBUG_ASSIGN_VECTOR4(Direct_Specular, float4(specularColor, 1));
    return specularColor;
}

BRDFData InitializeWaterBRDFData(half foamLerp)
{
    half3 albedo = 0;
    half metallic = 0;
    half3 specular = 1;
    half smoothness = 0.9;
    half alpha = 1;
    #ifdef _FlowMap_Enable
        albedo = lerp(albedo, _FoamColor2.xyz, foamLerp);
        metallic = lerp(metallic, _FoamMetallic, foamLerp);
        specular = lerp(specular, _FoamSpecular.xxx, foamLerp);
        smoothness = lerp(smoothness, _FoamSmoothness, foamLerp);
    #endif
    BRDFData outBRDFData;
    InitializeBRDFData(albedo, metallic, specular, smoothness, alpha, outBRDFData);
    return outBRDFData;
}

#endif // WATER_LIGHTING_INCLUDED
