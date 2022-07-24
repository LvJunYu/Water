﻿#ifndef WaveRipple
#define WaveRipple

inline float2 EncodeFloatRG(float v)
{
    float2 kEncodeMul = float2(1.0, 255.0);
    float kEncodeBit = 1.0 / 255.0;
    float2 enc = kEncodeMul * v;
    enc = frac(enc);
    enc.x -= enc.y * kEncodeBit;
    return enc;
}

inline float DecodeFloatRG(float2 enc)
{
    float2 kDecodeDot = float2(1.0, 1 / 255.0);
    return dot(enc, kDecodeDot);
}

float4 EncodeHeight(float height)
{
    float2 rg = EncodeFloatRG(height >= 0 ? height : 0);
    float2 ba = EncodeFloatRG(height < 0 ? -height : 0);

    return float4(rg, ba);
}

float DecodeHeight(float4 rgba)
{
    float d1 = DecodeFloatRG(rgba.rg);
    float d2 = DecodeFloatRG(rgba.ba);

    if (d1 >= d2)
        return d1;
    else
        return -d2;
}

#endif