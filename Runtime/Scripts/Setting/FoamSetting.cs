using System;
using UnityEngine;

namespace WaterSystem.Data
{
    [Serializable]
    public class FoamSetting : WaterSettingItem
    {
        public bool foamEnable = true;
        public EFoamType foamType = EFoamType.RiverFoam;
        public Texture2D foamMap;
        public Texture2D noiseMap;
        public Color foamColor = Color.white;
        public float foamIntensity = 1f;
        public float shallowsHeight = 1f;
        public float foamParam5 = 1f;
        public float foamParam6 = 1f;
        public float foamParam7 = 1f;
        public float foamParam8 = 1f;
        public float foamParam9 = 1f;
        public float foamParam10 = 1f;
        public float foamParam1 = 1f;
        public float foamParam2 = 1f;
        public float foamParam3 = 1f;
        public float foamParam4 = 1f;
        public int seaFoamType = 1; // 0=default, 1=simple, 3=custom
        public AnimationCurve basicFoam;
        public Texture2D defaultFoamRamp; // a default foam ramp for the basic foam setting
        public Texture2D bakedDepthTex;

        // Foam curves
        public FoamSetting()
        {
            basicFoam = new AnimationCurve(new Keyframe[2]
            {
                new Keyframe(0.25f, 0f),
                new Keyframe(1f, 1f)
            });
        }

        public void SetMaterial(Material material)
        {
            switch (foamType)
            {
                case EFoamType.SeaFoam:
                    material.SetTexture(WaterDepthMap, bakedDepthTex);
                    material.SetVector(_FoamParam2, new Vector4(foamParam1, foamParam2));
                    break;
                case EFoamType.RiverFoam:
                    material.SetVector(_FoamParam2, new Vector4(foamParam1, foamParam2, 1f / foamParam3, foamParam4));
                    material.SetVector(_FoamParam3, new Vector4(foamParam7, foamParam8, foamParam9, foamParam10));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            material.SetVector(_FoamParam, new Vector4(foamIntensity, shallowsHeight, 1f / foamParam5, foamParam6));
            material.SetColor(_FoamColor, foamColor);
            if (foamEnable && foamIntensity > 0.01f && foamColor != Color.black)
            {
                switch (foamType)
                {
                    case EFoamType.SeaFoam:
                        material.SetTexture(_FoamMap, foamMap);
                        material.DisableKeyword("_Foam_River");
                        material.EnableKeyword("_Foam_Sea");
                        break;
                    case EFoamType.RiverFoam:
                        material.SetTexture(_FoamMap, noiseMap);
                        material.DisableKeyword("_Foam_Sea");
                        material.EnableKeyword("_Foam_River");
                        break;
                }
            }
            else
            {
                material.DisableKeyword("_Foam_Sea");
                material.DisableKeyword("_Foam_River");
            }
        }

        private static readonly int _FoamParam = Shader.PropertyToID("_FoamParam");
        private static readonly int _FoamParam2 = Shader.PropertyToID("_FoamParam2");
        private static readonly int _FoamParam3 = Shader.PropertyToID("_FoamParam3");
        private static readonly int _FoamColor = Shader.PropertyToID("_FoamColor");
        private static readonly int _FoamMap = Shader.PropertyToID("_FoamMap");
        private static readonly int WaterDepthMap = Shader.PropertyToID("_WaterDepthMap");
    }

    public enum EFoamType
    {
        SeaFoam,
        RiverFoam
    }
}