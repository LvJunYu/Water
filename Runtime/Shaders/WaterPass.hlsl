#ifndef WaterPassInclude
#define WaterPassInclude

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "WaterInput.hlsl"
#include "CommonUtilities.hlsl"
#include "WaterLighting.hlsl"

#if _Foam_Sea | _Foam_River
    #define _Foam_Enable 1
    #include "Packages/water/Runtime/Features/Foam/WaterFoam.hlsl"
#endif

#if _Wave_Enable
    #include "Packages/water/Runtime/Features/Wave/GerstnerWaves.hlsl"
#endif

#if _Caustics_Enable
#include "Packages/water/Runtime/Features/Caustics/WaterCaustics.hlsl"
#endif

#if _Ripple_Normal
    #include "Packages/water/Runtime/Features/Ripple/WaterRipple.hlsl"
#endif

#ifdef ENABLE_FLOW_MAP
    #include "Packages/water/Runtime/Features/FlowMap/FlowMap.hlsl" 
#endif

struct WaterVertexInput
{
    float4 vertex : POSITION;
    float2 texcoord : TEXCOORD0;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct WaterVertexOutput
{
    float4 uv : TEXCOORD0;
    float4 posWSFog : TEXCOORD1;
    float3 normal : NORMAL;
    float4 posVSwaveHeight : TEXCOORD2;
    float4 viewDirNoise : TEXCOORD3;
    float4 screenPos : TEXCOORD4;
    float4 uv2 : TEXCOORD5;

    float4 clipPos : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

void WaveVertexOperations(WaterVertexInput v, inout WaterVertexOutput output)
{
    output.uv2.xy = v.texcoord;
    output.posWSFog.xyz = TransformObjectToWorld(v.vertex.xyz);
    output.normal = float3(0, 1, 0);
    float2 time = _Time.y * _BumpSpeed;
    float3 worldPos = output.posWSFog.xyz;
    #if _Wave_Enable
    output.viewDirNoise.w = ((noise((worldPos.xz * 0.5) + time) + noise((worldPos.xz) + time)) * 0.25 - 0.5) + 1;
    #endif

    #ifdef ENABLE_FLOW_MAP
    {
        output.uv.zw = worldPos.xz * 0.1 * _FlowNormalSize;
        output.uv.xy = worldPos.xz * 0.1 * _SurfaceSize;
    }
    #else
    {
        output.uv.xy = worldPos.xz * 0.1 * _SurfaceSize + time * 0.05 + (output.viewDirNoise.w * 0.1);
        float2 time2 = _Time.y * _BumpSpeed2;
        output.uv.zw = worldPos.xz * 0.1 * _SurfaceSize2 + time2 * 0.05 + (output.viewDirNoise.w * 0.1);

        #ifdef _TRIPLE_NORMAL
            float2 time3 = _Time.y * _BumpSpeed3;
            output.uv2.zw = worldPos.xz * 0.1 * _SurfaceSize3 + time3 * 0.05 + (output.viewDirNoise.w * 0.1);
        #endif
    }

    #endif

    half waterDepth = WaterTextureDepth(output.posWSFog.xyz) * 19.1 - 4.1;
    #if _Foam_Sea
    output.posWSFog.y += saturate((1-waterDepth) * 0.6 - 0.5) * _ShallowsHeight; //根据海底深度修正海面高低
    #endif

    //Gerstner here
    #if _Wave_Enable
	WaveStruct wave;
	SampleWaves(worldPos, saturate((waterDepth * 0.25)) + 0.1, wave, v.texcoord); //用垂直水深做mask，浅潭海浪小，深海海浪大
    output.normal = normalize(wave.normal.xzy);
    output.posWSFog.xyz += wave.position;
	output.posVSwaveHeight.w = wave.position.y / _MaxWaveHeight; // encode the normalized wave height into additional data
    #endif

    #if _Ripple_WaveEquation
	float2 rippleUv = (output.posWSFog.xz - _RippleCenter) / _RippleSize * 0.5 + 0.5;
	float rippleHeight = SAMPLE_TEXTURE2D_LOD(_RippleHeightTex, sampler_ScreenTextures_linear_clamp, rippleUv, 0).r;
	float mask = min(0.5, waterDepth * 0.25);
	output.posWSFog.y += clamp(rippleHeight * 0.5, -mask, mask);
    #endif

    // After waves
    output.clipPos = TransformWorldToHClip(output.posWSFog.xyz);
    output.screenPos = ComputeScreenPos(output.clipPos);
    output.posVSwaveHeight.xyz = TransformWorldToView(output.posWSFog.xyz);
    output.viewDirNoise.xyz = GetCameraPositionWS() - output.posWSFog.xyz;
    output.posWSFog.w = ComputeFogFactor(output.clipPos.z);

    // distance blend
    half distanceBlend = saturate(length((_WorldSpaceCameraPos.xz - output.posWSFog.xz) * 0.005) - 0.25);
    output.normal = lerp(output.normal, half3(0, 1, 0), distanceBlend); //越远越平，250米远是平的
}

WaterVertexOutput WaterVertex(WaterVertexInput v)
{
    WaterVertexOutput o = (WaterVertexOutput)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    WaveVertexOperations(v, o);
    return o;
}

half4 WaterFragment(WaterVertexOutput IN) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(IN);
    float3 worldPos = IN.posWSFog.xyz;
    #if _Ripple_Normal
    IN.normal = RippleNormal(IN.posWSFog.xyz, IN.normal);
    #elif _Ripple_WaveEquation
	float2 rippleUv = (IN.posWSFog.xz - _RippleCenter) / _RippleSize * 0.5 + 0.5;
	float3 rippleNormal = SAMPLE_TEXTURE2D(_RippleNormalTex, sampler_ScreenTextures_linear_clamp, rippleUv).rgb * 2 - 1;
	IN.normal = normalize(rippleNormal);
    #endif

    // Depth
    half3 screenUV = IN.screenPos.xyz / IN.screenPos.w; //screen UVs
    float rawD = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, screenUV.xy);
    float3 depth = WaterDepth(worldPos, rawD, IN.viewDirNoise.xyz, IN.posVSwaveHeight.xyz);
    // TODO - hardcoded shore depth UVs

    #if WEATHER_EFFECT
	    half f_occlusion = 0;
        half3 f_diffuseColor = half3(0,0,0);
        half3 f_specularColor = half3(0,0,0);
        half f_smoothness = 0;
        APPLY_RIPPLE(IN, IN.posWSFog.xyz, f_occlusion, IN.normal, f_diffuseColor, f_specularColor, f_smoothness)
    #endif

    // Shadow
    half shadow = 1;
    #if _Shadow_Enable | _ShadowJitter_Enable
    float3 shadowPos = IN.posWSFog.xyz;
    #if _ShadowJitter_Enable
    	half2 jitterUV = screenUV.xy * _ScreenParams.xy * _DitherPattern_TexelSize.xy;
        jitterUV += frac(_Time.zw * _BumpSpeed);
        float3 jitterTexture = SAMPLE_TEXTURE2D(_DitherPattern, sampler_ScreenTextures_linear_repeat, jitterUV).xyz * 2 - 1;
        jitterTexture.z = 0;
    	shadowPos += jitterTexture.xzy * _ShadowJitter;
    #endif
    shadow = MainLightRealtimeShadow(TransformWorldToShadowCoord(shadowPos));
    shadow = LerpWhiteTo(shadow, _ShadowIntensity);
    #endif

    // Light    
    Light mainLight = GetMainLight();
    half3 GI = SampleSH(IN.normal);

    // lightColor
    half3 lightColor = 1 * (shadow * mainLight.color + GI);

    float3 detailBump = 0;
    half foamLerp = 0;
    // Detail waves
    #if _BumpMap_Enable | ENABLE_FLOW_MAP
    #ifdef ENABLE_FLOW_MAP
        foamLerp = SampleFlowMap(IN.uv, IN.uv2, detailBump, IN.normal);
    #else

    float3 detailBump1, detailBump2;
    detailBump1.xy = SAMPLE_TEXTURE2D(_SurfaceMap, sampler_ScreenTextures_linear_repeat, IN.uv.zw).xy * 2 - 1;
    detailBump2.xy = SAMPLE_TEXTURE2D(_SurfaceMap, sampler_ScreenTextures_linear_repeat, IN.uv.xy).xy * 2 - 1;
    #ifdef _TRIPLE_NORMAL
    float3 detailBump3;
    detailBump3.xy = SAMPLE_TEXTURE2D(_SurfaceMap, sampler_ScreenTextures_linear_repeat, IN.uv2.zw).xy * 2 - 1;
    #endif
    float detailScale = saturate(depth.x * 0.25);
    detailBump1.xy *= detailScale * _BumpScale;
    detailBump2.xy *= detailScale * _BumpScale2;
    #ifdef _TRIPLE_NORMAL
    detailBump3.xy *= detailScale * _BumpScale3;
    detailBump3.z = sqrt(1.0 - saturate(dot(detailBump3.xy, detailBump3.xy)));
    #endif
    detailBump1.z = sqrt(1.0 - saturate(dot(detailBump1.xy, detailBump1.xy)));
    detailBump2.z = sqrt(1.0 - saturate(dot(detailBump2.xy, detailBump2.xy)));
    
    #ifdef _TRIPLE_NORMAL
    detailBump = blend_whiteout(detailBump1, detailBump2, detailBump3);
    #else
    detailBump = blend_whiteout(detailBump1, detailBump2);
    #endif

    IN.normal = blend_rnm(IN.normal.xzy, detailBump).xzy;
    // detailBump.xy = (detailBump1 + detailBump2 * 0.5) * saturate(depth.x * 0.25 + 0.25);
    // IN.normal += half3(detailBump.x, 0, detailBump.y) * _BumpScale;
    #endif
    IN.normal = normalize(IN.normal);
    #endif

    // Distortion 根据视线深度扰动uv，影响折射，模拟水流对水下物体的扰动效果
    half2 distortion = DistortionUVs(depth.x, IN.normal) * _Distortion;
    distortion = screenUV.xy + distortion; // * clamp(depth.x, 0, 5);
    float d = depth.x;
    depth.xz = AdjustedDepth(distortion, IN.viewDirNoise.xyz, IN.posVSwaveHeight.xyz);
    distortion = depth.x < 0 ? screenUV.xy : distortion;
    depth.x = depth.x < 0 ? d : depth.x;

    float2 foamUv = IN.uv.zw + detailBump.xy * 0.0025;
    #if _Foam_Sea
        half foamMask = SeaFoam(foamUv, depth.x, depth.y, IN.posVSwaveHeight.w, IN.viewDirNoise.w);
    #elif _Foam_River
        half foamMask = RiverFoam(foamUv, depth.x);
    #endif

    half3 foam = 0;
    #if _Foam_Enable
        foam = (foamMask * _FoamIntensity) * _FoamColor * (shadow * mainLight.color + GI);
    #endif

    float3 viewDir = SafeNormalize(IN.viewDirNoise.xyz);

    BRDFData brdfData = InitializeWaterBRDFData(foamLerp);
    half3 spec = DirectSpecular(brdfData, IN.normal, mainLight.direction, viewDir, _HModifier) * (_SpecularIntensity *
        shadow) * mainLight.color;

    #ifdef _ADDITIONAL_LIGHTS
        uint pixelLightCount = GetAdditionalLightsCount();
        for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
        {
            Light light = GetAdditionalLight(lightIndex, IN.posWSFog.xyz);
            spec += LightingPhysicallyBased(brdfData, light, IN.normal, viewDir);
            lightColor += light.distanceAttenuation * light.color;
        }
    #endif

    half3 reflection = SampleReflections(IN.posWSFog.xyz, IN.normal, IN.viewDirNoise.xyz, screenUV.xy, 0.0,
                                         IN.clipPos.z);
    half fresnelTerm = CalculateFresnelTerm(IN.normal, viewDir);
    reflection *= _ReflectIntensity * fresnelTerm;

    spec = clamp(reflection + spec, 0, _SpecularClamp);

    // Refraction
    half3 refraction = Refraction(distortion, depth.x);
    #if _Caustics_Enable
    refraction *= Caustics(screenUV.xy, rawD);
    #endif

    half depthMulti = 1 / _MaxDepth;
    half3 diffuse;
    #ifdef _SIMPLE_SCATTER
    {
        half3 shoalColor = lerp(refraction, refraction * _ShallowColor.rgb, saturate(depth.x * depthMulti));
        diffuse = lerp(_DeepColor.rgb, shoalColor, saturate(exp2(-depth.x * depthMulti)));
    }
    #else
    {
        refraction *= Absorption(depth.x * depthMulti);
        half3 sss = lightColor * Scattering(depth.x * depthMulti);
        half subsurfaceInstance = max(0, IN.posVSwaveHeight.w + _SubSurfaceScale);
        sss += SubSurfaceLighting(viewDir, mainLight.direction, subsurfaceInstance, shadow, mainLight.color);
        diffuse = refraction + sss;
    }
    
    #ifdef _ADDITION_COLOR
        diffuse += lerp(_AdditionColor1, _AdditionColor2, saturate(fresnelTerm * _AdditionRange));
    #endif
    
    #endif
    #if _Foam_Sea | _Foam_River
        //diffuse *= 1 - saturate(foam);
	    diffuse += foam; //lerp(refraction, color + reflection + foam, 1-saturate(1-depth.x * 25));
    #endif

    half3 color = diffuse + spec;
    
    //边缘过度
    color = lerp(refraction, color, 1 - saturate(1 - depth.x * _EdgeRange));

    color = MixFog(color, IN.posWSFog.w);

    return half4(color, 1);
}

#endif
