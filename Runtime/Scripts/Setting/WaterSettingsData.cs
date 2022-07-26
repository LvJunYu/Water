using UnityEngine;

namespace LYU.WaterSystem.Data
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "SeaSettingsData", menuName = "WaterSystem/SeaSettings", order = 0)]
    public class WaterSettingsData : ScriptableObject
    {
        public SurfaceSetting surfaceSetting = new SurfaceSetting();
        public ReflectionSetting reflectionSetting = new ReflectionSetting();
        public ShadowSetting shadowSetting = new ShadowSetting();
        public FoamSetting foamSetting = new FoamSetting();
        public WaveSetting waveSetting = new WaveSetting();
        public CausticsSetting causticsSetting = new CausticsSetting();
        public RippleSetting rippleSetting = new RippleSetting();

        public void SetMaterial(Material material)
        {
            reflectionSetting.SetMaterial(material);
            shadowSetting.SetMaterial(material);
            surfaceSetting.SetMaterial(material);
            foamSetting.SetMaterial(material);
            waveSetting.SetMaterial(material);
            causticsSetting.SetMaterial(material);
            rippleSetting.SetMaterial(material);
        }

        public void Refresh(Water water)
        {
            reflectionSetting.SetReflection(water.gameObject, water.waterMaterial);
            surfaceSetting.GenerateColorRamp(foamSetting);
            waveSetting.SetWaves(water);
            rippleSetting.Refresh(water);
        }

        public void Cleanup()
        {
            waveSetting.Cleanup();
            rippleSetting.Cleanup();
        }

        public void UpdateSetting(Material material)
        {
            rippleSetting.UpdateRipple(material);
        }
    }
}