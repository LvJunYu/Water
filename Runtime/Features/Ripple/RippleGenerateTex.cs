using System;
using UnityEngine;
using WaterSystem.Data;

public class RippleGenerateTex
{
    static float trochoids_approx(float v)
    {
        float A = 1.0f;
        return -1.0f + 2.0f * A *
            Mathf.Pow(1.0f - Mathf.Pow(0.5f + 0.5f * Mathf.Sin(2 * v), A + 1.0f), 1.0f / (A + 1.0f));
    }

    static float wavecore(float x, float k, float c, float t, RippleSetting.WaveShape waveShape)
    {
        t *= 2;
        float xx;
        switch (waveShape)
        {
            // case RippleSetting.WaveShape.CapillaryWave:
            //     xx = x - c * t;
            //     return Mathf.Sin(k * xx) * Mathf.Exp(-xx * xx);
            case RippleSetting.WaveShape.GravityWave:
                t *= 8;
                xx = x - 1.0f / c * t;
                return Mathf.Sin(k * xx) * Mathf.Exp(-xx * xx);
            // case RippleSetting.WaveShape.TrochoidCapillaryWave:
            //     xx = x - c * t;
            //     return trochoids_approx(k * xx) * Mathf.Exp(-xx * xx);
            case RippleSetting.WaveShape.TrochoidGravityWave:
                t *= 4;
                xx = x - 1.0f / c * t;
                return trochoids_approx(k * xx) * Mathf.Exp(-xx * xx);
            default:
                throw new ArgumentOutOfRangeException(nameof(waveShape), waveShape, null);
        }
    }

    static float NormalizationWave(float min, float max, float value)
    {
        float result = (value - min) / (max - min);
        return result;
    }

    struct WaveTextureData
    {
        public int size;
        public float maxDis;
        public float maxTime;
        public float frequency;
        public RippleSetting.WaveShape waveShape;
        public float min;
        public float max;
        public Texture2D texture;

        public bool CheckSame(int texSize, float f, float maxTime1, float frequency1,
            RippleSetting.WaveShape waveShape1, Texture2D tex)
        {
            return (size == texSize && f == maxDis && maxTime1 == maxTime && frequency1 == frequency &&
                    waveShape1 == waveShape && tex == texture);
        }

        public void SetData(int texSize, float f, float maxTime1, float frequency1, RippleSetting.WaveShape waveShape1,
            float min1, float max1, Texture2D tex)
        {
            size = texSize;
            maxDis = f;
            maxTime = maxTime1;
            frequency = frequency1;
            waveShape = waveShape1;
            min = min1;
            max = max1;
            texture = tex;
        }
    }

    static WaveTextureData lastTexData;

    public static Texture2D ProduceWaveTexture(Texture2D waveTexture, int texSize, float maxDis, float maxTime,
        float frequency, RippleSetting.WaveShape waveShape, out float min, out float max)
    {
        if (lastTexData.CheckSame(texSize, maxDis, maxTime, frequency, waveShape, waveTexture))
        {
            min = lastTexData.min;
            max = lastTexData.max;
            return lastTexData.texture;
        }

        float maxValue = 0;
        float minValue = 0;
        if (waveTexture == null || texSize != waveTexture.width)
            waveTexture = new Texture2D(texSize, texSize);
        var colors = new Color[texSize * texSize];
        float deltaDis = maxDis / (texSize - 1);
        float deltaTime = maxTime / (texSize - 1);
        for (int i = 0; i < texSize; i++)
        {
            var d = deltaDis * i;
            var x = d * frequency * 2.5f;
            for (int j = 0; j < texSize; j++)
            {
                var t = deltaTime * j;
                float value = 0;
                for (int k = 1; k < 10; k++)
                {
                    value += wavecore(x, k, Mathf.Sqrt(k), t, waveShape) / k +
                             wavecore(-x, k, Mathf.Sqrt(k), t, waveShape) / k;
                }

                if (value > maxValue) maxValue = value;
                if (value < minValue) minValue = value;
            }
        }

        min = minValue;
        max = maxValue;
        // Debug.Log($"ripple height min: {minValue}, max: {maxValue}");
        int index = 0;
        for (int j = 0; j < texSize; j++)
        {
            var t = deltaTime * j;
            for (int i = 0; i < texSize; i++)
            {
                var d = deltaDis * i;
                var x = d * frequency * 2.5f;
                float value = 0;
                for (int k = 1; k < 10; k++)
                {
                    value += wavecore(x, k, Mathf.Sqrt(k), t, waveShape) / k +
                             wavecore(-x, k, Mathf.Sqrt(k), t, waveShape) / k;
                }

                value = NormalizationWave(minValue, maxValue, value);
                colors[index++] = new Color(value, 0, 0);
            }
        }

        waveTexture.SetPixels(colors);
        waveTexture.Apply();
        lastTexData.SetData(texSize, maxDis, maxTime, frequency, waveShape, min, max, waveTexture);
        return waveTexture;
    }

    public static Texture2D ProduceCustomWaveTexture(Texture2D rippleTexture, int size, AnimationCurve waveform)
    {
        if (rippleTexture == null || rippleTexture.width != size)
        {
            rippleTexture = new Texture2D(128, 1, TextureFormat.Alpha8, false);
            rippleTexture.wrapMode = TextureWrapMode.Clamp;
            rippleTexture.filterMode = FilterMode.Bilinear;
        }

        var colors = new Color[rippleTexture.width];
        for (var i = 0; i < colors.Length; i++)
        {
            var x = 1.0f / rippleTexture.width * i;
            colors[i].a = waveform.Evaluate(x);
        }

        rippleTexture.SetPixels(colors);
        rippleTexture.Apply();
        return rippleTexture;
    }
}