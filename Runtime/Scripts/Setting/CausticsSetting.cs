using System;
using UnityEngine;

namespace LYU.WaterSystem.Data
{
    [Serializable]
    public class CausticsSetting : WaterSettingItem
    {
        public bool causticsEnable = true;
        public Texture2D causticsTexture;
        public float causticsIntensity = 1f;
        public float causticsSize = 0.5f;
        public float causticsOffset = 0.5f;
        public float causticsBlendDistance = 1f;

        public void SetMaterial(Material material)
        {
            material.SetTexture(_CausticMap, causticsTexture);
            material.SetVector(_CausticsParam1,
                new Vector4(causticsIntensity, causticsSize, causticsOffset, causticsBlendDistance));
            if (causticsEnable && causticsIntensity > 0.01f && causticsTexture != null)
                material.EnableKeyword("_Caustics_Enable");
            else
                material.DisableKeyword("_Caustics_Enable");
        }

        private static readonly int _CausticMap = Shader.PropertyToID("_CausticMap");
        private static readonly int _CausticsParam1 = Shader.PropertyToID("_CausticsParam1");
    }
}