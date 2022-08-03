using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LYU.WaterSystem.Data
{
    public class RippleWaveEquation
    {
        private RippleSetting _setting;
        private RenderTexture _heightTex;
        private RenderTexture _rt1;
        private RenderTexture _rt2;
        private RenderTexture _normalTex;
        private Vector4[] _dataArray;
        private HashSet<RippleTrigger> _triggers = new HashSet<RippleTrigger>();
        private Material _rippleHeightMat;
        private Material _rippleCurHeightMat;
        private Material _rippleNormalMat;

        private RenderTexture _pre;
        private RenderTexture _cur;

        public RippleWaveEquation(RippleSetting setting)
        {
            _setting = setting;
        }

        public void RenderPipelineManagerOnBeginFrameRendering(ScriptableRenderContext context, Camera[] cameras)
        {
            CommandBuffer cmd = CommandBufferPool.Get("RippleHeight");
            cmd.Clear();
            //cur
            _rippleCurHeightMat.SetInt(_RippleCount, GetWaveRippleData());
            _rippleCurHeightMat.SetVectorArray(_RippleData, _dataArray);
            cmd.Blit(_heightTex, _cur, _rippleCurHeightMat);
            //height
            _rippleHeightMat.SetTexture(PreRippleHeightTex, _pre);
            cmd.Blit(_cur, _heightTex, _rippleHeightMat);
            //normal
            cmd.Blit(_heightTex, _normalTex, _rippleNormalMat);
            //switch
            var temp = _cur;
            _cur = _pre;
            _pre = temp;
            // cmd.Blit(_cur, _pre);
            cmd.SetGlobalTexture(RippleHeightTex, _heightTex);
            cmd.SetGlobalTexture(RippleNormalTex, _normalTex);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public void Refresh()
        {
            _triggers.Clear();
            CalculateTextureSize();
            InitMaterials();
            InitTextures(_texSize);
            _pre = _rt1;
            _cur = _rt2;
            RenderPipelineManager.beginFrameRendering -= RenderPipelineManagerOnBeginFrameRendering;
            RenderPipelineManager.beginFrameRendering += RenderPipelineManagerOnBeginFrameRendering;
        }

        public void Cleanup()
        {
            RenderPipelineManager.beginFrameRendering -= RenderPipelineManagerOnBeginFrameRendering;
            ClearRippleTextures();
        }

        public void SetMaterial(Material material)
        {
            material.EnableKeyword("_Ripple_WaveEquation");
            _rippleHeightMat.SetVector(RippleLiquidParams,
                GetLiquidParams(_setting.velocity, _setting.viscosity, 0.1f * _setting.precision / _texSize,
                    Time.fixedDeltaTime));
            _rippleCurHeightMat.SetVector(_RippleParam, new Vector4(_setting.intensity, _setting._waterLevel));
            var rippleRange = new Vector4(_setting.center.x, _setting.center.y, _setting.rippleRange,
                _setting.rippleRange);
            _rippleCurHeightMat.SetVector(RippleRange, rippleRange);
            _rippleNormalMat.SetVector(RippleRange, rippleRange);
            material.SetVector(RippleRange, rippleRange);
        }

        public void CheckRipple(RippleTrigger trigger)
        {
            bool samePos = trigger.CheckSamePos();
            bool inRange = Mathf.Abs(trigger.transform.position.y - _setting._waterLevel) < trigger.radius;
            if (!samePos && inRange)
                _triggers.Add(trigger);
            else
                _triggers.Remove(trigger);
        }

        public void RemoveTrigger(RippleTrigger trigger)
        {
            _triggers.Remove(trigger);
        }

        private Vector4 GetLiquidParams(float speed, float damp, float deltaSpace, float deltaTime)
        {
            float maxvelocity = deltaSpace / (2 * deltaTime) * Mathf.Sqrt(damp * deltaTime + 2);
            float velocity = maxvelocity * speed;
            float viscositySq = damp * damp;
            float velocitySq = velocity * velocity;
            float deltaSizeSq = deltaSpace * deltaSpace;
            float dt = Mathf.Sqrt(viscositySq + 32 * velocitySq / (deltaSizeSq));
            float dtden = 8 * velocitySq / (deltaSizeSq);
            float maxT = (damp + dt) / dtden;
            float maxT2 = (damp - dt) / dtden;
            if (maxT2 > 0 && maxT2 < maxT)
                maxT = maxT2;
            if (maxT < deltaTime)
                Debug.LogError("粘度系数不符合要求");

            // float fac = velocitySq * deltaTime * deltaTime / deltaSizeSq;
            float i = damp * deltaTime - 2;
            float j = damp * deltaTime + 2;

            // float k1 = (4 - 8 * fac) / (j);
            float k1 = 4 / j - speed * speed * 2f;
            float k2 = i / j;
            // float k3 = 2 * fac / j;
            float k3 = speed * speed / 2f;

            return new Vector4(k1, k2, k3, deltaSpace);
        }

        private int GetWaveRippleData()
        {
            if (_dataArray == null || _dataArray.Length == 0)
                _dataArray = new Vector4[RippleSetting.RippleCountLimit];
            var count = 0;
            for (var i = 0; i < _setting.maxRippleCount; i++)
            {
                _dataArray[i] = Vector4.zero;
            }

            foreach (var trigger in _triggers)
            {
                if (count >= _setting.maxRippleCount) break;
                if (trigger.radius < 0.0001f) continue;
                var pos = trigger.Offset + trigger.transform.position;
                _dataArray[count] = new Vector4(pos.x, pos.y, pos.z, trigger.radius);
                count++;
            }

            count = Test(count);
            return count;
        }

        private void CalculateTextureSize()
        {
            _texSize = _setting.rippleRange * _setting.precision * 2;
            if (_texSize > 2048)
            {
                _texSize = 2048;
                Debug.LogWarning($"贴图尺寸过大，建议降低精度或减少涟漪区域。");
            }
        }

        private void InitMaterials()
        {
            if (_rippleHeightMat == null)
                _rippleHeightMat = new Material(_setting.rippleHeightShader);
            if (_rippleCurHeightMat == null)
                _rippleCurHeightMat = new Material(_setting.rippleCurHeightShader);
            if (_rippleNormalMat == null)
                _rippleNormalMat = new Material(_setting.rippleNormalShader);
        }

        private void InitTextures(int texSize)
        {
            _heightTex = GetTexture(texSize, texSize, RenderTextureFormat.RFloat, _heightTex);
            _rt1 = GetTexture(texSize, texSize, RenderTextureFormat.RFloat, _rt1);
            _rt2 = GetTexture(texSize, texSize, RenderTextureFormat.RFloat, _rt2);
            _normalTex = GetTexture(texSize, texSize, RenderTextureFormat.ARGB32, _normalTex);
        }

        private void ClearRippleTextures()
        {
            if (_heightTex != null)
            {
                _heightTex.Release();
                _heightTex = null;
            }

            if (_rt1 != null)
            {
                _rt1.Release();
                _rt1 = null;
            }

            if (_rt2 != null)
            {
                _rt2.Release();
                _rt2 = null;
            }
        }

        private RenderTexture GetTexture(int width, int height, RenderTextureFormat format, RenderTexture rt)
        {
            if (rt == null)
            {
                rt = CreateRenderTexture(width, height, format);
            }
            else
            {
                if (rt.width != width || rt.height != height)
                {
                    rt.Release();
                    rt = CreateRenderTexture(width, height, format);
                }
            }

            return rt;
        }

        private RenderTexture CreateRenderTexture(int width, int height, RenderTextureFormat format)
        {
            var rt = new RenderTexture(width, height, 0, format);
            // reflectTex.enableRandomWrite = true;
            // reflectTex.name = "RippleHeightTexture";
            // reflectTex.filterMode = FilterMode.Point;
            rt.Create();
            return rt;
        }

        private static readonly int PreRippleHeightTex = Shader.PropertyToID("_PreRippleHeightTex");
        private static readonly int RippleHeightTex = Shader.PropertyToID("_RippleHeightTex");
        private static readonly int RippleNormalTex = Shader.PropertyToID("_RippleNormalTex");
        private static readonly int _RippleCount = Shader.PropertyToID("_RippleCount");
        private static readonly int _RippleData = Shader.PropertyToID("rippleData");
        private static readonly int RippleRange = Shader.PropertyToID("_RippleRange");
        private static readonly int _RippleParam = Shader.PropertyToID("_RippleParam");
        private static readonly int RippleLiquidParams = Shader.PropertyToID("_RippleLiquidParams");

        // private int _testCount = 1;
        // private int _frameInternal = 2;
        // private float _testRange = 50;
        // private float _maxRadius = 1f;
        // private float _minRadius = 0.1f;
        private int _texSize = 512;

        private int Test(int count)
        {
            if (Time.frameCount % _setting.frameInternal != 0) return count;
            for (var i = 0; i < _setting.raindropCount; i++)
            {
                if (count >= _setting.maxRippleCount) break;
                _dataArray[count] = new Vector4(
                    Random.Range(_setting.center.x - _setting.testRange, _setting.center.x + _setting.testRange),
                    Random.Range(_setting._waterLevel - _setting.testDepth, _setting._waterLevel + _setting.testDepth),
                    Random.Range(_setting.center.y - _setting.testRange, _setting.center.y + _setting.testRange),
                    Random.Range(_setting.minRadius, _setting.maxRadius));
                count++;
            }

            return count;
        }
    }
}