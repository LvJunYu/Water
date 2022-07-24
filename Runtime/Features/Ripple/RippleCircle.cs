using System.Collections.Generic;
using UnityEngine;

namespace WaterSystem.Data
{
    public class RippleCircle
    {
        private RippleSetting _setting;
        private Droplet[] _droplets;
        private Vector4[] _dataArray;
        private int _dropCount;
        private int _time;
        private int _frame;
        private bool _active; //运行时判断是否激活，优化性能
        private float _lastTrigger;
        private float _minHeight;
        private float _maxHeight;
        private Texture2D _rippleTexture;
        private Texture2D _rippleTexture2;

        private HashSet<RippleTrigger> _triggers = new HashSet<RippleTrigger>();

        public RippleCircle(RippleSetting setting)
        {
            _setting = setting;
        }

        public void Refresh()
        {
            _triggers.Clear();
            SetRippleTexture();
        }

        private void InitDroplets()
        {
            if (_droplets == null)
            {
                _droplets = new Droplet[RippleSetting.RippleCountLimit];
                for (var i = 0; i < _droplets.Length; i++)
                    _droplets[i] = new Droplet();
            }

            if (_dataArray == null)
            {
                _dataArray = new Vector4[RippleSetting.RippleCountLimit];
            }
        }

        private Vector4[] GetRippleData()
        {
            for (int i = 0; i < _setting.maxRippleCount; i++)
            {
                _dataArray[i] = _droplets[i].MakeShaderParameter();
            }

            return _dataArray;
        }

        private void Trigger(Vector3 pos)
        {
            InitDroplets();
            _dropCount = (_dropCount + 1) % _setting.maxRippleCount;
            _droplets[_dropCount].Start(pos, _setting.param6);
            _lastTrigger = _setting.lifeTime;
        }

        public void UpdateRipple(Material material)
        {
            if (_frame == Time.frameCount) return;
            _frame = Time.frameCount;
            if (_lastTrigger > 0f)
            {
                if (!_active)
                {
                    material.EnableKeyword("_Ripple_Normal");
                    _active = true;
                }
            }
            else
            {
                if (_active)
                {
                    material.DisableKeyword("_Ripple_Normal");
                    _active = false;
                }
            }

            InitDroplets();
            // Test();
            var deltaTime = Time.deltaTime;
            for (var i = 0; i < _setting.maxRippleCount; i++)
            {
                _droplets[i].Update(deltaTime);
            }

            if (_active)
            {
                _lastTrigger -= deltaTime;
                UpdateDropletProperty(material);
            }
        }

        private void Test()
        {
            _time++;
            var center = new Vector3(0, 0, 0);
            var delta = 0.2f;
            if (_time % (int) (200f) == 0)
                Trigger(center + new Vector3(Random.Range(-delta, delta), 0, Random.Range(-delta, delta)));
        }

        public void SetMaterial(Material material)
        {
            if (_active)
                material.EnableKeyword("_Ripple_Normal");
            material.SetInt(_RippleCount, _setting.maxRippleCount);
            material.SetTexture(_RippleNoiseMap, _setting.noiseMap);
            material.SetVector(_RippleParam,
                new Vector4(_setting.speed / 2f, _setting.frequency / 2f, _setting.intensity, _setting.lifeTime));
            material.SetVector(_RippleParam2,
                new Vector4(_setting.param1, _setting.param2, _setting.param3, _setting.param4));
            material.SetVector(_RippleParam3,
                new Vector4(_setting.param5 * _setting.param5, _setting.param6, _setting.param7, _setting.param8));
            switch (_setting.waveShape)
            {
                // case WaveShape.CapillaryWave:
                case RippleSetting.WaveShape.GravityWave:
                // case WaveShape.TrochoidCapillaryWave:
                case RippleSetting.WaveShape.TrochoidGravityWave:
                    material.SetTexture(_RippleMap, _rippleTexture2);
                    material.SetVector(_RippleParam4, new Vector4(_minHeight, _maxHeight));
                    material.DisableKeyword("_CustomWave");
                    break;
                case RippleSetting.WaveShape.CustomWave:
                    material.SetTexture(_RippleMap, _rippleTexture);
                    material.EnableKeyword("_CustomWave");
                    break;
            }
        }

        private void SetRippleTexture()
        {
            switch (_setting.waveShape)
            {
                // case WaveShape.CapillaryWave:
                case RippleSetting.WaveShape.GravityWave:
                // case WaveShape.TrochoidCapillaryWave:
                case RippleSetting.WaveShape.TrochoidGravityWave:
                    _rippleTexture2 = RippleGenerateTex.ProduceWaveTexture(_rippleTexture2, 128,
                        _setting.param3,
                        _setting.lifeTime,
                        _setting.frequency, _setting.waveShape, out _minHeight, out _maxHeight);
                    break;
                case RippleSetting.WaveShape.CustomWave:
                    _rippleTexture = RippleGenerateTex.ProduceCustomWaveTexture(_rippleTexture, 128, _setting.waveform);
                    break;
            }
        }

        private void UpdateDropletProperty(Material material)
        {
            material.SetVectorArray(_RippleData, GetRippleData());
        }

        public void Cleanup()
        {
            if (_droplets != null)
                foreach (var d in _droplets)
                    d.Stop();
        }

        private bool CheckContains(Vector3 pos)
        {
            return pos.y <= _setting._waterLevel;
        }

        public void CheckRipple(RippleTrigger trigger)
        {
            var pos = trigger.transform.position + trigger.Offset;
            if (CheckContains(pos))
            {
                if (!_triggers.Contains(trigger))
                {
                    _triggers.Add(trigger);
                    Trigger(pos);
                }
            }
            else
            {
                _triggers.Remove(trigger);
            }
        }

        public void RemoveTrigger(RippleTrigger trigger)
        {
            _triggers.Remove(trigger);
        }

        private static readonly int _RippleMap = Shader.PropertyToID("_RippleMap");
        private static readonly int _RippleNoiseMap = Shader.PropertyToID("_RippleNoiseMap");
        private static readonly int _RippleParam = Shader.PropertyToID("_RippleParam");
        private static readonly int _RippleParam2 = Shader.PropertyToID("_RippleParam2");
        private static readonly int _RippleParam3 = Shader.PropertyToID("_RippleParam3");
        private static readonly int _RippleParam4 = Shader.PropertyToID("_RippleParam4");
        private static readonly int _RippleData = Shader.PropertyToID("rippleData");
        private static readonly int _RippleCount = Shader.PropertyToID("_RippleCount");

        class Droplet
        {
            Vector3 position;
            float time;

            public Droplet()
            {
                time = 1000;
            }

            public void Start(Vector3 pos, float startTime = 0)
            {
                position = pos;
                time = startTime;
            }

            public void Stop()
            {
                time = 1000;
            }

            public void Update(float deltaTime)
            {
                time += deltaTime;
            }

            public Vector4 MakeShaderParameter()
            {
                return new Vector4(position.x, position.z, time, 0);
            }
        }
    }
}