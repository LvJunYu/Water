#ifndef GERSTNER_WAVES_INCLUDED
#define GERSTNER_WAVES_INCLUDED

uniform uint _WaveCount; // how many waves, set via the water component

struct Wave
{
    half amplitude;
    half direction;
    half wavelength;
    half speed;
};

//TODO: srp batch 不支持数组PerMaterial，如果有两个不同材质相同变体的水srp batch会导致海浪数据不正确
#define _MAX_WAVE_COUNT 20
#if defined(USE_STRUCTURED_BUFFER)
StructuredBuffer<Wave> _WaveDataBuffer;
#else
half4 waveData[_MAX_WAVE_COUNT]; // amplitude, direction, wavelength
#endif

struct WaveStruct
{
    float3 position;
    float3 normal;
};

float3 TransformVector3(float3 dir, float3 normalWS, float3 tangentWS, float3 bitangentWS)
{
    return dir.y * normalWS + dir.x * tangentWS + -dir.z * bitangentWS;
}

WaveStruct GerstnerWave(half2 pos, float waveCountMulti, half amplitude, half direction, half wavelength, half speed,
                        float2 uv)
{
    WaveStruct waveOut;
    float time = _Time.y * speed;
    const float pi2 = PI * 2;
    ////////////////////////////////wave value calculations//////////////////////////
    half3 wave = 0; // wave vector
    half frequency = 6.28318 / wavelength; // 2pi over wavelength(hardcoded)
    half wSpeed = sqrt(9.8 * frequency); // frequency of the wave based off wavelength
    half peak = _WaveSharpness; // peak value, 1 is the sharpest peaks
    half qi = peak / (amplitude * frequency * _WaveCount);
    direction = radians(direction); // convert the incoming degrees to radians, for directional waves
    half2 windDir = half2(sin(direction), cos(direction));
    half dir = dot(windDir, pos); // calculate a gradient along the wind direction
    half calc = dir * frequency + -time * wSpeed; // the wave calculation
    ////////////////////////////position output calculations/////////////////////////
    half cosCalc = cos(calc); // cosine version(used for horizontal undulation)
    half sinCalc = sin(calc); // sin version(used for vertical undulation)

    // calculate the offsets for the current point
    wave.xz = qi * amplitude * windDir.xy * cosCalc;
    wave.y = ((sinCalc * amplitude)) * waveCountMulti; // the height is divided by the number of waves

    ////////////////////////////normal output calculations/////////////////////////
    half wa = frequency * amplitude;
    // normal vector
    half3 n = half3(-(windDir.xy * wa * cosCalc),
                    1 - (qi * wa * sinCalc));

    ////////////////////////////////assign to output///////////////////////////////
    waveOut.position = wave * saturate(amplitude * 10000);
    waveOut.normal = (n * waveCountMulti);
    //waveOut.normal = float3(windDir, 0) * waveCountMulti;
    return waveOut;
}

inline void SampleWaves(float3 position, half opacity, out WaveStruct waveOut, float2 uv)
{
    half2 pos = position.xz;
    waveOut.position = 0;
    waveOut.normal = 0;
    half waveCountMulti = 1.0 / _WaveCount;
    half3 opacityMask = saturate(half3(3, 1, 3) * opacity);

    UNITY_LOOP
    for (uint i = 0; i < _WaveCount; i++)
    {
        #if defined(USE_STRUCTURED_BUFFER)
		Wave w = _WaveDataBuffer[i];
        #else
        Wave w;
        w.amplitude = waveData[i].x;
        w.direction = waveData[i].y;
        w.wavelength = waveData[i].z;
        w.speed = waveData[i].w;
        #endif
        WaveStruct wave = GerstnerWave(pos,
                                       waveCountMulti,
                                       w.amplitude,
                                       w.direction,
                                       w.wavelength,
                                       w.speed,
                                       uv); // calculate the wave

        waveOut.position += wave.position; // add the position
        waveOut.normal += wave.normal; // add the normal
    }
    waveOut.position *= opacityMask;
}

#endif // GERSTNER_WAVES_INCLUDED
