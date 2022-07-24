Shader "Hidden/Water/RippleHeight"
{
    Properties
    {
        _MainTex("MainTex", 2D) = "white" {}
        _PreRippleHeightTex("PreRippleHeightTex", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100

        Pass
        {
            Lighting Off
            Cull Off
            ZTest Always
            ZWrite Off

            HLSLPROGRAM
// Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "WaveRipple.hlsl"

            TEXTURE2D (_PreRippleHeightTex);
            SAMPLER (sampler_PreRippleHeightTex);
            TEXTURE2D (_MainTex);
            SAMPLER (sampler_MainTex);

            half4 _RippleLiquidParams;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.vertex = vertexInput.positionCS;
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half2 uv = input.uv;
                float cur = _RippleLiquidParams.x * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).r;
                float pre = _RippleLiquidParams.y * SAMPLE_TEXTURE2D(_PreRippleHeightTex, sampler_PreRippleHeightTex, uv).r;
                float round = _RippleLiquidParams.z * (
                    (SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(_RippleLiquidParams.w, 0)).r)
                    + (SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-_RippleLiquidParams.w, 0)).r)
                    + (SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0, _RippleLiquidParams.w)).r)
                    + (SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0, -_RippleLiquidParams.w)).r)
                );
                cur += (round + pre);
                cur *= 0.96;
                return cur;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}