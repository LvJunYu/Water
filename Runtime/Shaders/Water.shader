Shader "LYU/Water/Water"
{
    Properties
    {
        [HideInInspector] _SurfaceMap("Surface Map", 2D) = "black" {}
        [HideInInspector] _DitherPattern ("Dithering Pattern", 2D) = "grey" {}
        [HideInInspector] _FoamMap("Foam Map", 2D) = "white" {}
        [HideInInspector] _SeaBedHeightMap("_SeaBedHeightMap", 2D) = "black" {}
        [HideInInspector] _AbsorptionScatteringRamp("_AbsorptionScatteringRamp", 2D) = "white" {}
        [HideInInspector] _CausticMap("_CausticMap", 2D) = "black" {}
        [HideInInspector] _RippleMap("_RippleMap", 2D) = "black" {}
        [HideInInspector] _RippleNoiseMap("_RippleNoiseMap", 2D) = "black" {}

        [HideInInspector] _WaterParam1("WaterParam1", Vector) = (0.0, 0.0, 0.0, 0.0)
        [HideInInspector] _WaterParam2("WaterParam2", Vector) = (0.0, 0.0, 0.0, 0.0)
        [HideInInspector] _SurfaceParam("SurfaceParam", Vector) = (0.0, 0.0, 0.0, 0.0)
        [HideInInspector] _SurfaceParam2("_SurfaceParam2", Vector) = (0.0, 0.0, 0.0, 0.0)
        [HideInInspector] _SurfaceParam3("_SurfaceParam3", Vector) = (0.0, 0.0, 0.0, 0.0)
        [HideInInspector] _SurfaceParam4("_SurfaceParam4", Vector) = (0.0, 0.0, 0.0, 0.0)
        [HideInInspector] _SubsurfaceParam("_SubsurfaceParam", Vector) = (0.0, 0.0, 0.0, 0.0)
        [HideInInspector] _SubSurfaceColor("_SubSurfaceColor", Color) = (0,1,1,1)
        [HideInInspector] _ReflectionParam("ReflectionParam", Vector) = (0.0, 0.0, 0.0, 0.0)
        [HideInInspector] _ShadowParam("ShadowParam", Vector) = (0.0, 0.0, 0.0, 0.0)
        [HideInInspector] _FoamParam("FoamParam", Vector) = (0.0, 0.0, 0.0, 0.0)
        [HideInInspector] _FoamParam2("FoamParam2", Vector) = (0.0, 0.0, 0.0, 0.0)
        [HideInInspector] _FoamParam3("FoamParam3", Vector) = (0.0, 0.0, 0.0, 0.0)
        [HideInInspector] _FoamColor("FoamColor", Color) = (1,1,1,1)
        [HideInInspector] _CausticsParam1("CausticsParam1", Vector) = (0.0, 0.0, 0.0, 0.0)
        [HideInInspector] _CausticsParam2("CausticsParam2", Vector) = (0.0, 0.0, 0.0, 0.0)
        [HideInInspector] _RippleParam("RippleParam", Vector) = (0.0, 0.0, 0.0, 0.0)
        [HideInInspector] _RippleParam2("RippleParam2", Vector) = (0.0, 0.0, 0.0, 0.0)
        [HideInInspector] _RippleParam3("RippleParam3", Vector) = (0.0, 0.0, 0.0, 0.0)
        [HideInInspector] _RippleParam4("RippleParam4", Vector) = (0.0, 0.0, 0.0, 0.0)
        [HideInInspector] _RippleCount("RippleCount", Int) = 3
        [HideInInspector] _WaveParam("WaveParam", Vector) = (0.0, 0.0, 0.0, 0.0)
        [HideInInspector] _RippleRange("_RippleRange", Vector) = (0.0, 0.0, 0.0, 0.0)
        
        [HideInInspector] _FlowMapScale("_FlowMapScale", Float) = 0
        [HideInInspector] _FlowNormalSize("_FlowNormalSize", Float) = 0
        [HideInInspector] _FlowSpeed("_FlowSpeed", Float) = 0

        [HideInInspector] _FoamMap2("_FoamMap2", 2D) = "white" {}
        [HideInInspector] _FoamMetallic("_FoamMetallic", Float) = 0
        [HideInInspector] _FoamSpecular("_FoamSpecular", Float) = 0
        [HideInInspector] _FoamSmoothness("_FoamSmoothness", Float) = 0
        [HideInInspector] _FoamIntensity("_FoamIntensity", Float) = 0
        [HideInInspector] _FoamColor2("_FoamColor2", Color) = (0, 0, 0, 0)
        [HideInInspector] MarchParam("MarchParam", Vector) = (0, 0, 0, 0)
        [HideInInspector] _SurfaceParam5("_SurfaceParam5", Vector) = (0, 0, 0, 0)
        [HideInInspector] _ShallowColor("_ShallowColor", Color) = (0, 0, 0, 0)
        [HideInInspector] _DeepColor("_DeepColor", Color) = (0, 0, 0, 0)
        [HideInInspector] _HModifier("_HModifier", Float) = 0
        [HideInInspector] _AdditionRange("_AdditionRange", Float) = 0
        [HideInInspector] _AdditionColor1("_AdditionColor1", Color) = (0, 0, 0, 0)
        [HideInInspector] _AdditionColor2("_AdditionColor2", Color) = (0, 0, 0, 0)

        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "Queue"="Transparent-100" "RenderPipeline" = "UniversalPipeline"
        }
        Blend[_SrcBlend][_DstBlend]
        ZWrite[_ZWrite]

        Pass
        {
            Name "WaterShading"

            Tags
            {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM
            #pragma prefer_hlslcc gles

            // -------------------------------------
            // Lightweight Pipeline keywords
            #pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

			// #pragma shader_feature _ USE_STRUCTURED_BUFFER
			#pragma shader_feature _ _REFLECTION_CUBEMAP _REFLECTION_PROBES _REFLECTION_PLANARREFLECTION _REFLECTION_SSPR _REFLECTION_TD_SSPR
			#pragma shader_feature _ _Foam_Sea _Foam_River
			#pragma shader_feature _ _Refraction_Dispersion_Enable
			#pragma shader_feature _ _Caustics_Enable _Caustics_Dispersion_Enable
			#pragma shader_feature _ _Shadow_Enable _ShadowJitter_Enable
			#pragma shader_feature _ _BumpMap_Enable _FlowMap_Enable
			#pragma shader_feature _ _Wave_Enable
			#pragma shader_feature _ _Ripple_Normal _Ripple_WaveEquation
			#pragma shader_feature _ _CustomWave
      
            #pragma shader_feature _ _TRIPLE_NORMAL
            #pragma shader_feature _ _SIMPLE_SCATTER
            #pragma shader_feature _ _ADDITION_COLOR

			//--------------------------------------
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
           
            #include "WaterPass.hlsl"
 
            #pragma vertex WaterVertex
            #pragma fragment WaterFragment
            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}