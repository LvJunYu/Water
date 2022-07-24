Shader "Hidden/Water/RippleNormal"
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
            float4 _MainTex_TexelSize;
            float4 _RippleRange;

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
                float cur = (SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).r);
                float right = (SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(_MainTex_TexelSize.x, 0.0)).r);
                float left = (SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-_MainTex_TexelSize.x, 0.0)).r);
                float top = (SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0.0, _MainTex_TexelSize.y)).r);
                float down = (SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0.0, -_MainTex_TexelSize.y)).r);
                float3 ddxPos = float3(_MainTex_TexelSize.x * _RippleRange.z * 4, right - left, 0);
                float3 ddyPos = float3(0, top - down, _MainTex_TexelSize.y * _RippleRange.w * 4);

                float3 rippleNormal = normalize(cross(ddyPos, ddxPos));
                return float4(rippleNormal * 0.5 + 0.5, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}