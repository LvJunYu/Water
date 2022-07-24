using System;
using UnityEngine;

namespace WaterSystem.Data
{
    [Serializable]
    public class ReflectionSetting : WaterSettingItem
    {
        public bool reflectionEnable = true;
        public ReflectionType refType = ReflectionType.PlanarReflection;

        public PlanarReflections.PlanarReflectionSettings planarSettings =
            new PlanarReflections.PlanarReflectionSettings();

        public Cubemap cubemapTexture;
        public int fresnelPower = 10;
        public float reflectDistort = 1;
        public float reflectIntensity = 1;
        public PlanarReflections.ResolutionMulltiplier ssprResolution = PlanarReflections.ResolutionMulltiplier.Half;
        public Vector2 MarchParam = Vector2.zero;
        private PlanarReflections planarReflections;

        public void SetReflection(GameObject go, Material mat)
        {
            if (reflectionEnable)
                switch (refType)
                {
                    case ReflectionType.Cubemap:
                    case ReflectionType.ReflectionProbe:
                        break;
                    case ReflectionType.PlanarReflection:
                        planarReflections = go.GetComponent<PlanarReflections>();
                        if (planarReflections == null)
                            planarReflections = go.AddComponent<PlanarReflections>();
                        planarReflections.Settings = planarSettings;
                        planarReflections.waterData.material = mat;
                        planarReflections.waterData.waverLevel = go.transform.position.y;
                        planarReflections.waterData.name = go.name;
                        planarReflections.enabled = true;
                        break;
                    case ReflectionType.SSPR:
                        SSPlanarReflectionFeature.SetSSPREnable(true);
                        SSPlanarReflectionFeature.WaterLevel = go.transform.position.y;
                        break;
                    case ReflectionType.TD_SSPR:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            if (refType != ReflectionType.PlanarReflection || !reflectionEnable)
            {
                planarReflections = go.GetComponent<PlanarReflections>();
                if (planarReflections != null)
                    planarReflections.enabled = false;
            }

            if (refType != ReflectionType.SSPR || !reflectionEnable)
            {
                SSPlanarReflectionFeature.SetSSPREnable(false);
            }
        }

        public void SetMaterial(Material seaMaterial)
        {
            if (reflectionEnable && reflectIntensity > 0)
            {
                seaMaterial.SetVector(ReflectionParam, new Vector4(fresnelPower, reflectDistort, reflectIntensity));
                switch (refType)
                {
                    case ReflectionType.Cubemap:
                        seaMaterial.EnableKeyword("_REFLECTION_CUBEMAP");
                        seaMaterial.DisableKeyword("_REFLECTION_PROBES");
                        seaMaterial.DisableKeyword("_REFLECTION_PLANARREFLECTION");
                        seaMaterial.DisableKeyword("_REFLECTION_SSPR");
                        seaMaterial.DisableKeyword("_REFLECTION_TD_SSPR");
                        seaMaterial.SetTexture(CubemapTexture, cubemapTexture);
                        break;
                    case ReflectionType.ReflectionProbe:
                        seaMaterial.DisableKeyword("_REFLECTION_CUBEMAP");
                        seaMaterial.EnableKeyword("_REFLECTION_PROBES");
                        seaMaterial.DisableKeyword("_REFLECTION_PLANARREFLECTION");
                        seaMaterial.DisableKeyword("_REFLECTION_SSPR");
                        seaMaterial.DisableKeyword("_REFLECTION_TD_SSPR");
                        break;
                    case ReflectionType.PlanarReflection:
                        seaMaterial.DisableKeyword("_REFLECTION_CUBEMAP");
                        seaMaterial.DisableKeyword("_REFLECTION_PROBES");
                        seaMaterial.EnableKeyword("_REFLECTION_PLANARREFLECTION");
                        seaMaterial.DisableKeyword("_REFLECTION_SSPR");
                        seaMaterial.DisableKeyword("_REFLECTION_TD_SSPR");
                        break;
                    case ReflectionType.SSPR:
                        seaMaterial.DisableKeyword("_REFLECTION_CUBEMAP");
                        seaMaterial.DisableKeyword("_REFLECTION_PROBES");
                        seaMaterial.DisableKeyword("_REFLECTION_PLANARREFLECTION");
                        seaMaterial.EnableKeyword("_REFLECTION_SSPR");
                        seaMaterial.DisableKeyword("_REFLECTION_TD_SSPR");
                        SSPlanarReflectionFeature.Scale = PlanarReflections.GetScaleValue(ssprResolution);
                        break;
                    case ReflectionType.TD_SSPR:
                        seaMaterial.DisableKeyword("_REFLECTION_CUBEMAP");
                        seaMaterial.DisableKeyword("_REFLECTION_PROBES");
                        seaMaterial.DisableKeyword("_REFLECTION_PLANARREFLECTION");
                        seaMaterial.DisableKeyword("_REFLECTION_SSPR");
                        seaMaterial.EnableKeyword("_REFLECTION_TD_SSPR");
                        seaMaterial.SetVector(MarchParamID, MarchParam);
                        break;
                }
            }
            else
            {
                seaMaterial.DisableKeyword("_REFLECTION_CUBEMAP");
                seaMaterial.DisableKeyword("_REFLECTION_PROBES");
                seaMaterial.DisableKeyword("_REFLECTION_PLANARREFLECTION");
                seaMaterial.DisableKeyword("_REFLECTION_SSPR");
                seaMaterial.DisableKeyword("_REFLECTION_TD_SSPR");
            }
        }

        private static readonly int ReflectionParam = Shader.PropertyToID("_ReflectionParam");
        private static readonly int CubemapTexture = Shader.PropertyToID("_CubemapTexture");
        private static readonly int MarchParamID = Shader.PropertyToID("MarchParam");
    }

    [Serializable]
    public enum ReflectionType
    {
        Cubemap,
        ReflectionProbe,
        PlanarReflection,
        SSPR,
        TD_SSPR,
    }
}