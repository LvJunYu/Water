using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LYU.WaterSystem.Data
{
    [Serializable]
    public class WaveSetting : WaterSettingItem
    {
        public const int MaxWaveCount = 20;
        public bool waveEnable = true;
        public float waveSpeed = 1f;
        public float speedRandom = 0f;

        public float sharpness = 1.2f;
        public List<Wave> _waves = new List<Wave>();
        public bool _customWaves;
        public int randomSeed = 3234;
        public BasicWaves _basicWaveSettings = new BasicWaves(1.5f, 45.0f, 5.0f);

        [SerializeField] public Wave[] _waveArray;
        [SerializeField] private ComputeBuffer _waveBuffer;
        private bool useComputeBuffer;
        public float _maxWaveHeight;

        public bool subSurfaceEnable;
        public Color subSurfaceColor = Color.cyan;
        public float subSurfaceSunFallOff = 5f;
        public float subSurfaceBase = 0.3f;
        public float subSurfaceSun = 3f;
        public float subSurfaceScale = 0.6f;
        public float testUv = 1f;

        public void SetMaterial(Material material)
        {
            if (waveEnable)
            {
                material.SetVector(WaveParam, new Vector4(0, sharpness, 0, 0));
                material.EnableKeyword("_Wave_Enable");

                if (_waveArray != null)
                    material.SetInt(WaveCount, _waveArray.Length);
                useComputeBuffer = false;
                if (useComputeBuffer)
                {
                    material.EnableKeyword("USE_STRUCTURED_BUFFER");
                    if (_waveBuffer == null)
                        _waveBuffer = new ComputeBuffer(10, sizeof(float) * 6);
                    _waveBuffer.SetData(_waveArray);
                    material.SetBuffer(WaveDataBuffer, _waveBuffer);
                }
                else
                {
                    material.DisableKeyword("USE_STRUCTURED_BUFFER");
                    material.SetVectorArray(WaveData, GetWaveData());
                }
            }
            else
            {
                material.DisableKeyword("_Wave_Enable");
            }

            if (waveEnable && subSurfaceEnable)
            {
                material.SetColor(_SubSurfaceColor, subSurfaceColor);
                material.SetVector(_SubsurfaceParam,
                    new Vector4(subSurfaceSunFallOff, subSurfaceBase, subSurfaceSun, subSurfaceScale));
            }
            else
                material.SetVector(_SubsurfaceParam, new Vector4(1, 0, 0));
        }

        public void SetWaves(Water water)
        {
            if (!waveEnable)
            {
                _maxWaveHeight = 0f;
                return;
            }

            _customWaves = false;
            SetupWaves(_customWaves);

            _maxWaveHeight = 0f;
            foreach (Wave w in _waveArray)
            {
                _maxWaveHeight += w.amplitude;
            }

            _maxWaveHeight /= _waveArray.Length;

            //CPU side
            if (!GerstnerWavesJobs.init && Application.isPlaying)
                GerstnerWavesJobs.Init(water);
        }

        public void Cleanup()
        {
            _waveBuffer?.Dispose();
        }

        private void SetupWaves(bool custom)
        {
            if (!custom)
            {
                //create basic waves based off basic wave settings
                Random.State backupSeed = Random.state;
                Random.InitState(randomSeed);
                BasicWaves basicWaves = _basicWaveSettings;
                float a = basicWaves.amplitude;
                float d = basicWaves.direction;
                float l = basicWaves.wavelength;
                int numWave = basicWaves.numWaves;
                _waveArray = new Wave[numWave];
                float r = 1f / numWave;
                for (int i = 0; i < numWave; i++)
                {
                    float p = Mathf.Lerp(0.5f, 1.5f, i * r);
                    float amp = a * p * Random.Range(0.8f, 1.2f);
                    float dir = d + Random.Range(-45f, 45f);
                    float len = l * p * Random.Range(0.6f, 1.4f);
                    float speed = waveSpeed * Random.Range(1 - speedRandom, 1 + speedRandom);
                    _waveArray[i] = new Wave(amp, dir, len, speed);
                    Random.InitState(randomSeed + i + 1);
                }

                Random.state = backupSeed;
            }
            else
            {
                _waveArray = _waves.ToArray();
            }
        }

        private Vector4[] GetWaveData()
        {
            if (_waveArray == null) return null;
            Vector4[] waveData = new Vector4[MaxWaveCount];
            for (int i = 0; i < _waveArray.Length; i++)
            {
                waveData[i] = new Vector4(_waveArray[i].amplitude, _waveArray[i].direction, _waveArray[i].wavelength,
                    _waveArray[i].speed);
            }

            return waveData;
        }

        private static readonly int WaveCount = Shader.PropertyToID("_WaveCount");
        private static readonly int WaveDataBuffer = Shader.PropertyToID("_WaveDataBuffer");
        private static readonly int WaveData = Shader.PropertyToID("waveData");
        private static readonly int WaveParam = Shader.PropertyToID("_WaveParam");

        private static readonly int _SubsurfaceParam = Shader.PropertyToID("_SubsurfaceParam");
        private static readonly int _SubSurfaceColor = Shader.PropertyToID("_SubSurfaceColor");
    }

    [Serializable]
    public struct Wave
    {
        public float amplitude; // height of the wave in units(m)
        public float direction; // direction the wave travels in degrees from Z+
        public float wavelength; // distance between crest>crest
        public float speed;

        public Wave(float amp, float dir, float length, float waveSpeed)
        {
            amplitude = amp;
            speed = waveSpeed;
            direction = dir;
            wavelength = length;
        }
    }

    [Serializable]
    public class BasicWaves
    {
        public int numWaves;
        public float amplitude;
        public float direction;
        public float wavelength;

        public BasicWaves(float amp, float dir, float len)
        {
            numWaves = 6;
            amplitude = amp;
            direction = dir;
            wavelength = len;
        }
    }
}