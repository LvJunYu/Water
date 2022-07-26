#ifndef WaterInputInclude
#define WaterInputInclude

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

CBUFFER_START(UnityPerMaterial)
float4 _DitherPattern_TexelSize;
float4 _WaterParam1;
half4 _WaterParam2;
half4 _SurfaceParam;
half4 _SurfaceParam2;
half4 _SubsurfaceParam;
half4 _SurfaceParam4;
half4 _SurfaceParam5;
half4 _ReflectionParam;
half4 _ShadowParam;
half4 _FoamParam;
half4 _FoamParam2;
half4 _FoamParam3;
half4 _FoamColor;
half4 _CausticsParam1;

half4 _RippleParam;
half4 _RippleParam2;
half4 _RippleParam3;
half4 _RippleParam4;
half4 _RippleRange;

half4 _WaveParam;

half4 _SubSurfaceColor;
uint _RippleCount;

//flow map
half _FlowMapScale;
half _FlowNormalSize;
half _FlowSpeed;
half _FoamMetallic;
half _FoamSpecular;
half _FoamSmoothness;
half _FoamIntensity2;
half4 _FoamColor2;
half4 _ShallowColor;
half4 _DeepColor;
half _HModifier;
half _AdditionRange;
half4 _AdditionColor1;
half4 _AdditionColor2;

half2 MarchParam;
CBUFFER_END

#define _SubSurfaceSunFallOff _SubsurfaceParam.x
#define _SubSurfaceBase _SubsurfaceParam.y
#define _SubSurfaceSun _SubsurfaceParam.z
#define _SubSurfaceScale _SubsurfaceParam.w

#define _WaveSharpness _WaveParam.y

#define _WaterCenterPos _WaterParam1.xyz
#define _WaterLevel _WaterParam1.y
#define _WaterMaxVerticalHeight _WaterParam1.w

#define _MaxViewDepth _WaterParam2.x
#define _MaxWaveHeight _WaterParam2.y
#define _HeighMapCameraAboveWaterHeight _WaterParam2.z
#define _HeighMapSizeInverse 1.0 / _WaterParam2.w

#define _SurfaceSize _SurfaceParam.x
#define _BumpScale _SurfaceParam.y
#define _BumpSpeed _SurfaceParam.zw
#define _SurfaceSize2 _SurfaceParam4.x
#define _BumpScale2 _SurfaceParam4.y
#define _BumpSpeed2 _SurfaceParam4.zw
#define _SurfaceSize3 _SurfaceParam5.x
#define _BumpScale3 _SurfaceParam5.y
#define _BumpSpeed3 _SurfaceParam5.zw

#define _EdgeRange _SurfaceParam2.x
#define _SpecularClamp _SurfaceParam2.y
#define _SpecularIntensity _SurfaceParam2.z
#define _Distortion _SurfaceParam2.w

#define _FresnelPower _ReflectionParam.x
#define _RelectDistort _ReflectionParam.y;
#define _ReflectIntensity _ReflectionParam.z

#define _ShadowIntensity _ShadowParam.x
#define _ShadowJitter _ShadowParam.y

#define _FoamIntensity _FoamParam.x
#define _ShallowsHeight _FoamParam.y
#define _FoamFactor1 _FoamParam2.x
#define _FoamFactor2 _FoamParam2.y
#define _FoamFactor3 _FoamParam2.z
#define _FoamFactor4 _FoamParam2.w
#define _FoamFactor5 _FoamParam.z
#define _FoamFactor6 _FoamParam.w
#define _FoamFactor7 _FoamParam3.x
#define _FoamFactor8 _FoamParam3.y
#define _FoamFactor9 _FoamParam3.z
#define _FoamFactor10 _FoamParam3.w

#define _CausticsIntensity _CausticsParam1.x
#define _CausticsSize _CausticsParam1.y
#define _CausticsOffset _CausticsParam1.z
#define _CausticsBlendDistance _CausticsParam1.w

#define _RippleSpeed _RippleParam.x
#define _RippleFrequency _RippleParam.y
#define _RippleIntensity _RippleParam.z
#define _RippleLife _RippleParam.w
#define _RippleFactor1 _RippleParam2.x
#define _RippleFactor2 _RippleParam2.y
#define _RippleFactor3 _RippleParam2.z
#define _RippleFactor4 _RippleParam2.w
#define _RippleFactor5 _RippleParam3.x
#define _RippleFactor6 _RippleParam3.y
#define _RippleFactor7 _RippleParam3.z
#define _RippleFactor8 _RippleParam3.w
#define _RippleHeightMin _RippleParam4.x
#define _RippleHeightMax _RippleParam4.y
#define _RippleCenter _RippleRange.xy
#define _RippleSize _RippleRange.zw

SAMPLER(sampler_ScreenTextures_linear_clamp);
SAMPLER(sampler_ScreenTextures_linear_repeat);

#if defined(_REFLECTION_PLANARREFLECTION)
TEXTURE2D(_PlanarReflectionTexture);
#elif defined(_REFLECTION_CUBEMAP)
TEXTURECUBE(_CubemapTexture);
SAMPLER(sampler_CubemapTexture);
#elif defined(_REFLECTION_SSPR)
TEXTURE2D(_SSPlanarReflectionTexture);
#endif

TEXTURE2D_FLOAT(_CameraDepthTexture);
SAMPLER(sampler_CameraDepthTexture);

TEXTURE2D(_CameraOpaqueTexture);
TEXTURE2D(_SeaBedHeightMap);
TEXTURE2D(_AbsorptionScatteringRamp);
TEXTURE2D(_SurfaceMap);
TEXTURE2D(_FoamMap);
TEXTURE2D(_DitherPattern);
TEXTURE2D(_CausticMap);
TEXTURE2D(_RippleMap);
TEXTURE2D(_RippleNoiseMap);
TEXTURE2D(_RippleHeightTex);
TEXTURE2D(_RippleNormalTex);

#endif