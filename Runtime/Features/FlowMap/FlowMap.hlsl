TEXTURE2D(_FlowNormal);
TEXTURE2D(_FlowMap);
TEXTURE2D(_FoamMap2);
SAMPLER(sampler_FlowMap);

float3 NormalBlend_UnpackedRNM(float3 n1, float3 n2)
{
    n1 += float3(0, 0, 1);
    n2 *= float3(-1, -1, 1);

    return normalize(n1 * dot(n1, n2) / n1.z - n2);
}

half SampleFlowMap(float4 INuv, float2 vertexUV, out half3 detailBump, out half3 normalWS)
{
    float2 offset1 = float2(0.418, 0.355);
    float2 offset2 = float2(0.865, 0.148);
    float2 offset3 = float2(0.651, 0.752);
    float norTime = _Time.y * _BumpSpeed.x;

    float2 norUV0 = INuv.xy + norTime * float2(1, 1);
    float2 norUV1 = INuv.xy + offset1 + norTime * float2(-1, -1);
    float2 norUV2 = INuv.xy + offset2 + norTime * float2(-1, 1);
    float2 norUV3 = INuv.xy + offset3 + norTime * float2(1, -1);

    float3 nor1 = SAMPLE_TEXTURE2D(_SurfaceMap, sampler_ScreenTextures_linear_repeat, norUV0).xyz * 2 - 1;
    float3 nor2 = SAMPLE_TEXTURE2D(_SurfaceMap, sampler_ScreenTextures_linear_repeat, norUV1).xyz * 2 - 1;
    float3 nor3 = SAMPLE_TEXTURE2D(_SurfaceMap, sampler_ScreenTextures_linear_repeat, norUV2).xyz * 2 - 1;
    float3 nor4 = SAMPLE_TEXTURE2D(_SurfaceMap, sampler_ScreenTextures_linear_repeat, norUV3).xyz * 2 - 1;

    float3 nor = lerp(float3(0, 0, 1), (nor1 + nor2 + nor3 + nor4) * 0.25, _BumpScale);
    //return float4(nor, 1);
    float time = frac(_Time.y * _FlowSpeed);
    float time2 = frac(_Time.y * _FlowSpeed + 0.5);
    half3 flowmap = SAMPLE_TEXTURE2D(_FlowMap, sampler_FlowMap, vertexUV).xyz;
    //return float4(flowmap, 1);
    flowmap.xy = flowmap.xy * 2 - 1;
    flowmap.xy *= float2(1, -1);
    float2 uv1 = INuv.zw + time.xx * flowmap.xy;
    float2 uv2 = INuv.zw + time2.xx * flowmap.xy;

    half3 detailBump1 = SAMPLE_TEXTURE2D(_FlowNormal, sampler_ScreenTextures_linear_repeat, uv1).xyz * 2 - 1;
    half3 detailBump2 = SAMPLE_TEXTURE2D(_FlowNormal, sampler_ScreenTextures_linear_repeat, uv2).xyz * 2 - 1;
    half4 foamLerp1 = SAMPLE_TEXTURE2D(_FoamMap2, sampler_ScreenTextures_linear_repeat, uv1);
    half4 foamLerp2 = SAMPLE_TEXTURE2D(_FoamMap2, sampler_ScreenTextures_linear_repeat, uv2);
    float flowLerp = abs(time * 2 - 1);

    detailBump = lerp(detailBump1, detailBump2, flowLerp);
    detailBump = lerp(float3(0, 0, 1), detailBump, _FlowMapScale * length(flowmap.xy));

    detailBump = NormalBlend_UnpackedRNM(nor, detailBump);
    normalWS = half3(detailBump.x, 1, detailBump.y);

    return saturate(dot(lerp(foamLerp1, foamLerp2, flowLerp).xyz, float3(1, 1, 1)) * flowmap.z * _FoamIntensity2);
}