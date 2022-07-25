using System;
using UnityEngine;

namespace LYU.WaterSystem.Data
{
    [Serializable]
    public class ShadowSetting : WaterSettingItem
    {
        public bool shadowEnable = true;
        public Texture2D ditherTexture;
        public float shadowIntensity = 1f;
        public float shadowJitter;

        public void SetMaterial(Material material)
        {
            material.SetVector(_ShadowParam, new Vector4(shadowIntensity, shadowJitter));
            material.SetTexture(_DitherPattern, ditherTexture);
            if (shadowEnable && shadowIntensity > 0.01f)
            {
                if (shadowJitter > 0.01f && ditherTexture != null)
                {
                    material.DisableKeyword("_Shadow_Enable");
                    material.EnableKeyword("_ShadowJitter_Enable");
                }
                else
                {
                    material.DisableKeyword("_ShadowJitter_Enable");
                    material.EnableKeyword("_Shadow_Enable");
                }
            }
            else
            {
                material.DisableKeyword("_Shadow_Enable");
                material.DisableKeyword("_ShadowJitter_Enable");
            }
        }

        private static readonly int _DitherPattern = Shader.PropertyToID("_DitherPattern");
        private static readonly int _ShadowParam = Shader.PropertyToID("_ShadowParam");
    }
}