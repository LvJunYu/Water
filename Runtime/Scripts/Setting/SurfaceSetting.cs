using System;
using UnityEngine;

namespace LYU.WaterSystem.Data
{
    [Serializable]
    public class SurfaceSetting : WaterSettingItem
    {
        public EBumpType bumpType = EBumpType.Bumpmap;
        public EScartterType scartterType = EScartterType.Detail;
        public Texture2D surfaceMap;

        public Texture2D flowMap;

        public Texture2D flowNormal;

        //public float flowMapTiling;
        public float flowMapScale;
        public float flowNormalSize;
        public float flowSpeed;
        public Texture2D foamMap;
        public float foamMetallic;
        public float foamSpecular;
        public float foamSmoothness;
        public Color foamColor;
        public float foamIntensity;

        public float bumpScale = 0.2f;
        public float surfaceSize = 1f;
        public Vector2 speed = new Vector2(1, 1);
        public float bumpScale2 = 0.2f;
        public float surfaceSize2 = 4f;
        public Vector2 speed2 = new Vector2(-2, -2);
        public float bumpScale3 = 0.2f;
        public float surfaceSize3 = 4f;
        public Vector2 speed3 = new Vector2(-2, -2);

        public float distort = 1f;
        public float edge = 1f;
        public float specularClamp = 1024;
        public float waterMaxVisibility = 20.0f;
        public float specularIntensity = 1f;
        public float specularRange = 1f;
        public Gradient absorptionRamp;
        public Gradient scatterRamp;

        public Boolean tripleNormalMap = false;
        [ColorUsage(true, false)] public Color shallowColor = new Color(1, 1, 1, 0);
        [ColorUsage(true, false)] public Color deepColor = Color.black;
        public Boolean additionColor = false;
        public Single additionRange = 0;
        [ColorUsage(true, false)] public Color additionColor1 = Color.black;
        [ColorUsage(true, false)] public Color additionColor2 = Color.black;

        [SerializeField] private Texture2D _rampTexture;
        private const int rampWidth = 128;

        public void GenerateColorRamp(FoamSetting foamSetting = null)
        {
            if (_rampTexture == null)
                _rampTexture = new Texture2D(rampWidth, 4, TextureFormat.ARGB32, false, false);
            if (absorptionRamp == null || scatterRamp == null)
                Setup();
            _rampTexture.wrapMode = TextureWrapMode.Clamp;
            Color[] cols = new Color[rampWidth * 4];
            for (int i = 0; i < rampWidth; i++)
            {
                cols[i] = absorptionRamp.Evaluate((float) i / rampWidth);
            }

            for (int i = 0; i < rampWidth; i++)
            {
                cols[i + rampWidth] = scatterRamp.Evaluate((float) i / rampWidth);
            }

            if (foamSetting != null && foamSetting.foamType == EFoamType.SeaFoam)
            {
                Texture2D defaultFoamRamp = foamSetting.defaultFoamRamp;
                for (int i = 0; i < rampWidth; i++)
                {
                    switch (foamSetting.seaFoamType)
                    {
                        case 0:
                            // default
                            cols[i + rampWidth * 2] =
                                foamSetting.defaultFoamRamp.GetPixelBilinear((float) i / rampWidth, 0.5f);
                            break;
                        case 1:
                            // simple
                            cols[i + rampWidth * 2] = defaultFoamRamp.GetPixelBilinear(
                                foamSetting.basicFoam.Evaluate((float) i / rampWidth), 0.5f);
                            break;
                        case 2:
                            // custom
                            cols[i + rampWidth * 2] = Color.black;
                            break;
                    }
                }
            }

            _rampTexture.SetPixels(cols);
            _rampTexture.Apply();
        }

        public void SetMaterial(Material material)
        {
            switch (bumpType)
            {
                case EBumpType.Bumpmap:
                    material.DisableKeyword("_FlowMap_Enable");
                    if (surfaceMap != null && (bumpScale != 0 || bumpScale2 != 0))
                        material.EnableKeyword("_BumpMap_Enable");
                    else
                        material.DisableKeyword("_BumpMap_Enable");
                    material.SetTexture(_SurfaceMap, surfaceMap);
                    material.SetVector(_SurfaceParam, new Vector4(surfaceSize, bumpScale, speed.x, speed.y));
                    material.SetVector(_SurfaceParam4, new Vector4(surfaceSize2, bumpScale2, speed2.x, speed2.y));
                    if (tripleNormalMap)
                    {
                        material.SetVector(_SurfaceParam5, new Vector4(surfaceSize3, bumpScale3, speed3.x, speed3.y));
                        material.EnableKeyword("_TRIPLE_NORMAL");
                    }
                    else
                    {
                        material.DisableKeyword("_TRIPLE_NORMAL");
                    }

                    break;
                case EBumpType.Flowmap:
                    material.DisableKeyword("_BumpMap_Enable");
                    if (flowMap != null)
                        material.EnableKeyword("_FlowMap_Enable");
                    else
                        material.DisableKeyword("_FlowMap_Enable");
                    material.SetTexture(_SurfaceMap, surfaceMap);
                    material.SetVector(_SurfaceParam, new Vector4(surfaceSize, bumpScale, speed.x, speed.y));

                    material.SetTexture(_FlowMap, flowMap);
                    material.SetTexture(_FlowNormal, flowNormal);
                    material.SetFloat(_FlowMapScale, flowMapScale);
                    material.SetFloat(_FlowNormalSize, flowNormalSize);
                    material.SetFloat(_FlowSpeed, flowSpeed);

                    material.SetTexture(_FoamMap, foamMap);
                    material.SetFloat(_FoamMetallic, foamMetallic);
                    material.SetFloat(_FoamSpecular, foamSpecular);
                    material.SetFloat(_FoamSmoothness, foamSmoothness);
                    material.SetFloat(_FoamIntensity, foamIntensity);
                    material.SetColor(_FoamColor, foamColor);
                    break;
            }

            switch (scartterType)
            {
                case EScartterType.Simple:
                    material.SetColor(_ShallowColor, shallowColor);
                    material.SetColor(_DeepColor, deepColor);
                    material.EnableKeyword("_SIMPLE_SCATTER");
                    break;
                case EScartterType.Detail:
                    material.DisableKeyword("_SIMPLE_SCATTER");
                    break;
            }

            material.SetTexture(AbsorptionScatteringRamp, _rampTexture);
            material.SetVector(_SurfaceParam2, new Vector4(edge, specularClamp, specularIntensity, distort));
            material.SetFloat(_SpecularRange, specularRange);

            if (additionColor)
            {
                material.SetFloat(_AdditionRange, additionRange);
                material.SetColor(_AdditionColor1, additionColor1);
                material.SetColor(_AdditionColor2, additionColor2);
                material.EnableKeyword("_ADDITION_COLOR");
            }
            else
            {
                material.DisableKeyword("_ADDITION_COLOR");
            }
        }

        public void Setup()
        {
            absorptionRamp = DefaultAbsorptionGrad();
            scatterRamp = DefaultScatterGrad();
        }

        Gradient DefaultAbsorptionGrad()
        {
            Gradient g = new Gradient();
            GradientColorKey[] gck = new GradientColorKey[5];
            GradientAlphaKey[] gak = new GradientAlphaKey[1];
            gak[0].alpha = 1;
            gak[0].time = 0;
            gck[0].color = Color.white;
            gck[0].time = 0f;
            gck[1].color = new Color(0.22f, 0.87f, 0.87f);
            gck[1].time = 0.082f;
            gck[2].color = new Color(0f, 0.47f, 0.49f);
            gck[2].time = 0.318f;
            gck[3].color = new Color(0f, 0.275f, 0.44f);
            gck[3].time = 0.665f;
            gck[4].color = Color.black;
            gck[4].time = 1f;
            g.SetKeys(gck, gak);
            return g;
        }

        Gradient DefaultScatterGrad()
        {
            Gradient g = new Gradient();
            GradientColorKey[] gck = new GradientColorKey[4];
            GradientAlphaKey[] gak = new GradientAlphaKey[1];
            gak[0].alpha = 1;
            gak[0].time = 0;
            gck[0].color = Color.black;
            gck[0].time = 0f;
            gck[1].color = new Color(0.08f, 0.41f, 0.34f);
            gck[1].time = 0.15f;
            gck[2].color = new Color(0.13f, 0.55f, 0.45f);
            gck[2].time = 0.42f;
            gck[3].color = new Color(0.21f, 0.62f, 0.6f);
            gck[3].time = 1f;
            g.SetKeys(gck, gak);
            return g;
        }

        private static readonly int AbsorptionScatteringRamp = Shader.PropertyToID("_AbsorptionScatteringRamp");

        private static readonly int _SurfaceMap = Shader.PropertyToID("_SurfaceMap");

        private static readonly int _FlowMap = Shader.PropertyToID("_FlowMap");

        //private static readonly int _FlowMapTiling = Shader.PropertyToID("_FlowMapTiling");
        private static readonly int _FlowMapScale = Shader.PropertyToID("_FlowMapScale");
        private static readonly int _FlowNormal = Shader.PropertyToID("_FlowNormal");
        private static readonly int _FlowNormalSize = Shader.PropertyToID("_FlowNormalSize");
        private static readonly int _FlowSpeed = Shader.PropertyToID("_FlowSpeed");

        private static readonly int _FoamMap = Shader.PropertyToID("_FoamMap2");
        private static readonly int _FoamMetallic = Shader.PropertyToID("_FoamMetallic");
        private static readonly int _FoamSpecular = Shader.PropertyToID("_FoamSpecular");
        private static readonly int _FoamSmoothness = Shader.PropertyToID("_FoamSmoothness");
        private static readonly int _FoamColor = Shader.PropertyToID("_FoamColor2");
        private static readonly int _FoamIntensity = Shader.PropertyToID("_FoamIntensity2");

        private static readonly int _SurfaceParam = Shader.PropertyToID("_SurfaceParam");
        private static readonly int _SurfaceParam2 = Shader.PropertyToID("_SurfaceParam2");
        private static readonly int _SurfaceParam4 = Shader.PropertyToID("_SurfaceParam4");
        private static readonly int _SurfaceParam5 = Shader.PropertyToID("_SurfaceParam5");
        private static readonly int _ShallowColor = Shader.PropertyToID("_ShallowColor");
        private static readonly int _DeepColor = Shader.PropertyToID("_DeepColor");
        private static readonly int _SpecularRange = Shader.PropertyToID("_HModifier");
        private static readonly int _AdditionRange = Shader.PropertyToID("_AdditionRange");
        private static readonly int _AdditionColor1 = Shader.PropertyToID("_AdditionColor1");
        private static readonly int _AdditionColor2 = Shader.PropertyToID("_AdditionColor2");
    }

    public enum EBumpType
    {
        Bumpmap,
        Flowmap
    }

    public enum EScartterType
    {
        Simple,
        Detail,
    }
}