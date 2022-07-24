Shader "Hidden/Water/GaussianBlur"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
        }

        LOD 100

        HLSLINCLUDE

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // Defines /////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        #define URP_DECLARE_TEXTURE2D(textureName) \
            TEXTURE2D(textureName); SAMPLER(sampler##textureName)

        #define URP_DECLARE_TEXTURE2D_ST(textureName) \
            URP_DECLARE_TEXTURE2D(textureName); float4 textureName##_ST

        #define URP_DECLARE_TEXTURE2D_ST_TEXELSIZE(textureName) \
            URP_DECLARE_TEXTURE2D_ST(textureName); float4 textureName##_TexelSize

        #define URP_SAMPLE_TEXTURE2D(textureName, uv) \
            SAMPLE_TEXTURE2D(textureName, sampler##textureName, uv)

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // Structures //////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        struct GaussianBlurVertexInput
        {
            float4 vertex : POSITION;
            float2 uv     : TEXCOORD0;
        };

        struct GaussianBlurVertexToFragment
        {
            float4 vertex : SV_POSITION;
            float2 uv     : TEXCOORD0;
        };

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // Variables ///////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        URP_DECLARE_TEXTURE2D_ST_TEXELSIZE(_MainTex);

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // Utility Functions ///////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        // inline half4 GaussianBlur1D(float2 uv, float2 offset)
        // {
        //     // const int   gaussianRadius = 2;
        //     // const half3 gaussianSampleWeight = half3(0.20416369, 0.30400535, 0.09391280);
        //     // const half3 gaussianSampleOffset = half3(0.00000000, 1.40733337, 3.29421496);

        //     const int   gaussianRadius = 1;
        //     const half2 gaussianSampleWeight = half2(0.25137913, 0.37431043);
        //     const half2 gaussianSampleOffset = half2(0.00000000, 1.40733337);

        //     half4 sampledCenterPixel = URP_SAMPLE_TEXTURE2D(_MainTex, uv);
        //     half3 gaussianColorSum = sampledCenterPixel.rgb * gaussianSampleWeight[0];
        //     for (int i = 1; i <= gaussianRadius; ++i)
        //     {
        //         half2 sampleUVOffset = gaussianSampleOffset[i] * offset;
        //         half3 sampledPixelLeft = URP_SAMPLE_TEXTURE2D(_MainTex, uv - sampleUVOffset).rgb;
        //         half3 sampledPixelRight = URP_SAMPLE_TEXTURE2D(_MainTex, uv + sampleUVOffset).rgb;
        //         gaussianColorSum += (sampledPixelLeft + sampledPixelRight) * gaussianSampleWeight[i];
        //     }

        //     return half4(gaussianColorSum, sampledCenterPixel.a); 
        // }

        inline half4 GaussianBlur1D(float2 uv, float2 offset) 
        {
            const int   gaussianRadius = 1;
            const half2 gaussianSampleWeight = half2(0.36166450, 0.31916779);

            half4 sampledCenterPixel = URP_SAMPLE_TEXTURE2D(_MainTex, uv);
            half3 gaussianColorSum = sampledCenterPixel.rgb * gaussianSampleWeight[0];
            [unroll]
            for (int i = 1; i <= gaussianRadius; ++i)
            {
                half2 sampleUVOffset = offset * i;
                half3 sampledPixelLeft = URP_SAMPLE_TEXTURE2D(_MainTex, uv - sampleUVOffset).rgb;
                half3 sampledPixelRight = URP_SAMPLE_TEXTURE2D(_MainTex, uv + sampleUVOffset).rgb;
                gaussianColorSum += (sampledPixelLeft + sampledPixelRight) * gaussianSampleWeight[i];
            }

            return half4(gaussianColorSum, sampledCenterPixel.a);
        }

        GaussianBlurVertexToFragment GaussianBlurVert(GaussianBlurVertexInput i)
        {
            VertexPositionInputs positions = GetVertexPositionInputs(i.vertex.xyz);

            GaussianBlurVertexToFragment o;
            o.uv = i.uv;
            o.vertex = positions.positionCS; 
            return o;
        }

        half4 GaussianBlurFrag_Horizontal(GaussianBlurVertexToFragment i) : SV_Target
        {
            return GaussianBlur1D(i.uv, float2(_MainTex_TexelSize.x, 0.0));
        }

        half4 GaussianBlurFrag_Vertical(GaussianBlurVertexToFragment i) : SV_Target
        {
            return GaussianBlur1D(i.uv, float2(0.0, _MainTex_TexelSize.y));
        }

        ENDHLSL

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // Passes //////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        // Pass - Horizontal Gaussian Blue
        Pass
        {
            HLSLPROGRAM
            #pragma vertex   GaussianBlurVert
            #pragma fragment GaussianBlurFrag_Horizontal
            ENDHLSL
        }

        // Pass - Vertical Gaussian Blue
        Pass
        {
            HLSLPROGRAM
            #pragma vertex   GaussianBlurVert
            #pragma fragment GaussianBlurFrag_Vertical
            ENDHLSL
        }
    }
}
