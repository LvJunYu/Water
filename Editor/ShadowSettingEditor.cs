using UnityEditor;
using UnityEngine;

namespace LYU.WaterSystem.Data
{
    [CustomPropertyDrawer(typeof(ShadowSetting))]
    public class ShadowSettingEditor : BaseWaterSettingEditor
    {
        protected override string settingsText => "阴影设置";

        protected override void Draw(SerializedProperty property)
        {
            var shadowEnable = property.FindPropertyRelative("shadowEnable");
            var shadowJitter = property.FindPropertyRelative("shadowJitter");
            var shadowIntensity = property.FindPropertyRelative("shadowIntensity");
            var ditherTexture = property.FindPropertyRelative("ditherTexture");
            shadowEnable.boolValue = EditorGUILayout.Toggle("阴影开关", shadowEnable.boolValue);
            EditorGUILayout.Slider(shadowIntensity, 0, 1, "阴影强度");
            EditorGUILayout.Slider(shadowJitter, 0, 3, "阴影扰动");
            EditorGUILayout.PropertyField(ditherTexture, new GUIContent("扰动图", "用于扰动水面阴影"));
        }
    }
}