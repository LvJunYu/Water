Shader "Hidden/Water/CurRippleHeight"
{
    Properties
    {
        _MainTex("MainTex", 2D) = "white" {}
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
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "WaveRipple.hlsl"
            
            TEXTURE2D (_MainTex);
            SAMPLER (sampler_MainTex);

            #define MaxRippleCount 15
            half4 rippleData[MaxRippleCount];
            uint _RippleCount;
            float4 _RippleRange;
            float4 _RippleParam;
            #define _Center _RippleRange.xy
            #define _Size _RippleRange.zw
            #define _RippleIntensity _RippleParam.x
            #define _WaterLevel _RippleParam.y;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float3 posWS : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.vertex = vertexInput.positionCS;
                output.uv = input.uv;
                output.posWS.xz = (output.uv * 2 - 1) * _Size + _Center;
                output.posWS.y = _WaterLevel;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half2 uv = input.uv;
                float3 posWS = input.posWS;
                float height = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).r;
                
                // [unroll]
                for (uint i = 0; i < _RippleCount; i++)
                {
                    float4 data = rippleData[i];
                    float value = max(0, data.w - length(posWS - data.xyz)) * _RippleIntensity;
                    if(value > 0)
                    {
                        height = max(value, height);
                    }
                }
                
                return height;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}