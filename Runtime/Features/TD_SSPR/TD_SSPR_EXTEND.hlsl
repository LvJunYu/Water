half3 SampleTDSSPR(half3 positionWS, half3 normalWS, half3 viewDirectionWS, float depth)
{
    half3 reflectionDirWS = normalize(reflect(-viewDirectionWS, normalWS));
    half3 worldReflectDirOffset = reflectionDirWS * (abs(viewDirectionWS).y * MarchParam.x * 100.0 + MarchParam.y) + positionWS;
    half4 reflectPositionCS = TransformWorldToHClip(worldReflectDirOffset);
    reflectPositionCS /= reflectPositionCS.w;

    float2 temp = reflectPositionCS.xy * reflectPositionCS.xy;
    reflectPositionCS.xy = reflectPositionCS.xy * 0.5 + 0.5;
    temp = max(1 - temp * temp, 0);

#if UNITY_UV_STARTS_AT_TOP
    reflectPositionCS.y = 1 - reflectPositionCS.y;
#endif
    float screenLerpEdge = temp.x * temp.y;
    float depth_scene = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, reflectPositionCS.xy), _ZBufferParams);
    float depth_ray = Linear01Depth(reflectPositionCS.z, _ZBufferParams);
    depth = Linear01Depth(depth, _ZBufferParams);
    half3 skyBoxCol = GlossyEnvironmentReflection(reflectionDirWS, 0, 1).xyz;
    if (depth_scene > 0.999999 || depth_scene < depth || depth_ray < depth)
    {
        return skyBoxCol;
    }
    else
    {
        half3 opaqueColor = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_ScreenTextures_linear_clamp, reflectPositionCS.xy).xyz;
        return lerp(skyBoxCol, opaqueColor, screenLerpEdge);
    }
}