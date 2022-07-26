#ifndef COMMON_UTILITIES_INCLUDED
#define COMMON_UTILITIES_INCLUDED

// remaps a value based on a in:min/max and out:min/max
// value		=		value to be remapped
// remap		=		x = min in, y = max in, z = min out, w = max out
float Remap(half value, half4 remap)
{
    return remap.z + (value - remap.x) * (remap.w - remap.z) / (remap.y - remap.x);
}

// Converts greyscale height to normal
// _tex			=		input texture(separate from a sampler)
// _sampler		=		the sampler to use
// _uv			=		uv coordinates
// _intensity	=		intensity of the effect
float3 HeightToNormal(Texture2D _tex, SamplerState _sampler, float2 _uv, half _intensity)
{
    float3 bumpSamples;
    bumpSamples.x = _tex.Sample(_sampler, _uv).x; // Sample center
    bumpSamples.y = _tex.Sample(_sampler, float2(_uv.x + _intensity / _ScreenParams.x, _uv.y)).x; // Sample U
    bumpSamples.z = _tex.Sample(_sampler, float2(_uv.x, _uv.y + _intensity / _ScreenParams.y)).x; // Sample V
    half dHdU = bumpSamples.z - bumpSamples.x; //bump U offset
    half dHdV = bumpSamples.y - bumpSamples.x; //bump V offset
    return float3(-dHdU, dHdV, 0.5); //return tangent normal
}

// Simple noise from thebookofshaders.com
// 2D Random
float2 random(float2 st)
{
    st = float2(dot(st, float2(127.1, 311.7)), dot(st, float2(269.5, 183.3)));
    return -1.0 + 2.0 * frac(sin(st) * 43758.5453123);
}

// 2D Noise based on Morgan McGuire @morgan3d
// https://www.shadertoy.com/view/4dS3Wd
float noise(float2 st)
{
    float2 i = floor(st);
    float2 f = frac(st);

    float2 u = f * f * (3.0 - 2.0 * f);

    return lerp(lerp(dot(random(i), f),
                     dot(random(i + float2(1.0, 0.0)), f - float2(1.0, 0.0)), u.x),
                lerp(dot(random(i + float2(0.0, 1.0)), f - float2(0.0, 1.0)),
                     dot(random(i + float2(1.0, 1.0)), f - float2(1.0, 1.0)), u.x), u.y);
}

//float noise2 (float2 st) {
//    float2 i = floor(st);
//    float2 f = frac(st);
//
//    float2 u = f*f*(3.0-2.0*f);
//    
//    float a = random(i);
//    float b = random(i + float2(1.0, 0.0));
//    float c = random(i + float2(0.0, 1.0));
//    float d = random(i + float2(1.0, 1.0));
//    
//	return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
//}

// https://blog.selfshadow.com/publications/blending-in-detail/
float3 blend_whiteout(float3 n1, float3 n2)
{
    float3 r = float3(n1.xy + n2.xy, n1.z * n2.z);
    return normalize(r);
}

float3 blend_whiteout(float3 n1, float3 n2, float3 n3)
{
    float3 r = float3(n1.xy + n2.xy + n3.xy, n1.z * n2.z * n3.z);
    return normalize(r);
}

float3 blend_rnm(float3 n1, float3 n2)
{
    n1 += float3(0, 0, 1);
    n2 *= float3(-1, -1, 1);

    return normalize(n1 * dot(n1, n2) - n2 * n1.z);
}

float3 blend_unity(float3 n1, float3 n2)
{
    float4 nf4 = n1.xyzz * float4(1, 1, 1, -1);
    float3 r;
    r.x = dot(nf4.zxx, n2.xyz);
    r.y = dot(nf4.yzy, n2.xyz);
    r.z = dot(nf4.xyw, -n2.xyz);
    return normalize(r);
}

half3 GetDetailNormal(float depth, WaterVertexOutput IN)
{
    half3 detailBump = 0;
    float detailScale = saturate(depth.x * 0.25);
    float3 detailBump1, detailBump2;
    detailBump1.xy = SAMPLE_TEXTURE2D(_SurfaceMap, sampler_ScreenTextures_linear_repeat, IN.uv.xy).xy * 2 - 1;
    detailBump2.xy = SAMPLE_TEXTURE2D(_SurfaceMap, sampler_ScreenTextures_linear_repeat, IN.uv.zw).xy * 2 - 1;
    detailBump1.xy *= detailScale * _BumpScale;
    detailBump2.xy *= detailScale * _BumpScale2;
    detailBump1.z = sqrt(1.0 - saturate(dot(detailBump1.xy, detailBump1.xy)));
    detailBump2.z = sqrt(1.0 - saturate(dot(detailBump2.xy, detailBump2.xy)));
   #ifdef _TRIPLE_NORMAL
        float3 detailBump3;
        detailBump3.xy = SAMPLE_TEXTURE2D(_SurfaceMap, sampler_ScreenTextures_linear_repeat, IN.uv2.xy).xy * 2 - 1;
        detailBump3.xy *= detailScale * _BumpScale3;
        detailBump3.z = sqrt(1.0 - saturate(dot(detailBump3.xy, detailBump3.xy)));
        detailBump = blend_whiteout(detailBump1, detailBump2, detailBump3);
    #else
        detailBump = blend_whiteout(detailBump1, detailBump2);
    #endif

    return detailBump;
}

#endif // COMMON_UTILITIES_INCLUDED
