using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace WaterSystem.Data
{
    [Serializable]
    public class RippleSetting : WaterSettingItem
    {
        public const int RippleCountLimit = 15;
        public RippleType rippleType = RippleType.Circle;
        public WaveShape waveShape = WaveShape.CustomWave;
        public bool rippleEnable;
        public float intensity = 0.5f;
        public float speed = 1f;
        public float frequency = 1f;
        public float lifeTime = 3f;
        public float param1 = 0.3f;
        public float param2 = 1f;
        public float param3 = 2f;
        public float param4 = 1f;
        public float param5 = 2f;
        public float param6;
        public float param7 = 1f;
        public float param8;

        public int maxRippleCount = 3;
        public Texture2D noiseMap;
        [NonSerialized] public float _waterLevel;

        private RippleWaveEquation _waveEquation;
        private RippleCircle _circleRipple;

        public AnimationCurve waveform = new AnimationCurve(
            new Keyframe(0.00f, 0.50f, 0, 0),
            new Keyframe(0.05f, 1.00f, 0, 0),
            new Keyframe(0.15f, 0.10f, 0, 0),
            new Keyframe(0.25f, 0.80f, 0, 0),
            new Keyframe(0.35f, 0.30f, 0, 0),
            new Keyframe(0.45f, 0.60f, 0, 0),
            new Keyframe(0.55f, 0.40f, 0, 0),
            new Keyframe(0.65f, 0.55f, 0, 0),
            new Keyframe(0.75f, 0.46f, 0, 0),
            new Keyframe(0.85f, 0.52f, 0, 0),
            new Keyframe(0.99f, 0.50f, 0, 0)
        );

        public Shader rippleCurHeightShader;
        public Shader rippleNormalShader;
        public Shader rippleHeightShader;

        public Vector2 center;
        public int rippleRange = 50;
        public int precision = 10;
        public float viscosity = 0.15f;
        public float velocity = 0.5f;

        public RippleSetting()
        {
            _waveEquation = new RippleWaveEquation(this);
            _circleRipple = new RippleCircle(this);
        }

        public enum WaveShape
        {
            // CapillaryWave = 0,
            GravityWave = 1,

            // TrochoidCapillaryWave = 2,
            TrochoidGravityWave = 3,
            CustomWave
        };

        public enum RippleType
        {
            Circle,
            WaveEquation
        }

        public void Refresh(Water water)
        {
            RenderPipelineManager.beginFrameRendering -= _waveEquation.RenderPipelineManagerOnBeginFrameRendering;
            if (!CheckEnable()) return;
            rippleHandlers.Add(this);
            _waterLevel = water.transform.position.y;
            switch (rippleType)
            {
                case RippleType.Circle:
                    _circleRipple.Refresh();
                    break;
                case RippleType.WaveEquation:
                    InitShader();
                    _waveEquation.Refresh();
                    break;
            }
        }

        private void InitShader()
        {
            if (rippleHeightShader == null)
                rippleHeightShader = Shader.Find("Hidden/Water/RippleHeight");
            if (rippleCurHeightShader == null)
                rippleCurHeightShader = Shader.Find("Hidden/Water/CurRippleHeight");
            if (rippleNormalShader == null)
                rippleNormalShader = Shader.Find("Hidden/Water/RippleNormal");
        }

        public void SetMaterial(Material material)
        {
            if (CheckEnable())
            {
                switch (rippleType)
                {
                    case RippleType.Circle:
                        material.DisableKeyword("_Ripple_WaveEquation");
                        _circleRipple.SetMaterial(material);
                        break;
                    case RippleType.WaveEquation:
                        material.DisableKeyword("_Ripple_Normal");
                        _waveEquation.SetMaterial(material);
                        break;
                }
            }
            else
            {
                material.DisableKeyword("_Ripple_Normal");
                material.DisableKeyword("_Ripple_WaveEquation");
            }
        }

        public bool CheckEnable()
        {
            return rippleEnable && intensity > 0.01f;
        }

        public void Cleanup()
        {
            rippleHandlers.Remove(this);
            _circleRipple.Cleanup();
            _waveEquation.Cleanup();
        }

        public void UpdateRipple(Material material)
        {
            if (!CheckEnable()) return;
            switch (rippleType)
            {
                case RippleType.Circle:
                    _circleRipple.UpdateRipple(material);
                    break;
                case RippleType.WaveEquation:
                    break;
            }
        }

        public void CheckRippleTrigger(RippleTrigger trigger)
        {
            if (!CheckEnable()) return;
            switch (rippleType)
            {
                case RippleType.Circle:
                    _circleRipple.CheckRipple(trigger);
                    break;
                case RippleType.WaveEquation:
                    _waveEquation.CheckRipple(trigger);
                    break;
            }
        }

        public void RemoveTrigger(RippleTrigger trigger)
        {
            if (!CheckEnable()) return;
            switch (rippleType)
            {
                case RippleType.Circle:
                    _circleRipple.RemoveTrigger(trigger);
                    break;
                case RippleType.WaveEquation:
                    _waveEquation.RemoveTrigger(trigger);
                    break;
            }
        }

        public static void UpdateRippleTrigger(RippleTrigger trigger)
        {
            foreach (var rippleHandler in rippleHandlers)
            {
                rippleHandler?.CheckRippleTrigger(trigger);
            }
        }

        public static void ClearUpTrigger(RippleTrigger trigger)
        {
            foreach (var rippleHandler in rippleHandlers)
            {
                rippleHandler?.RemoveTrigger(trigger);
            }
        }

        static HashSet<RippleSetting> rippleHandlers = new HashSet<RippleSetting>();
    }
}