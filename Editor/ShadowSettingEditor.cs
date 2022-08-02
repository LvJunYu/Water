using UnityEditor;
using UnityEngine;

namespace LYU.WaterSystem.Data
{
    [CustomPropertyDrawer(typeof(ShadowSetting))]
    public class ShadowSettingEditor : BaseWaterSettingEditor
    {
        protected override string settingsText => "Shadow Setting";

        protected override void Draw(SerializedProperty property)
        {
            var shadowEnable = property.FindPropertyRelative("shadowEnable");
            var shadowJitter = property.FindPropertyRelative("shadowJitter");
            var shadowIntensity = property.FindPropertyRelative("shadowIntensity");
            var ditherTexture = property.FindPropertyRelative("ditherTexture");
            shadowEnable.boolValue = EditorGUILayout.Toggle("Shadow Enable", shadowEnable.boolValue);
            EditorGUILayout.Slider(shadowIntensity, 0, 1, "Shadow Intensity");
            EditorGUILayout.Slider(shadowJitter, 0, 3, "Shadow Disturb");
            EditorGUILayout.PropertyField(ditherTexture, new GUIContent("Disturb Map", "Disturb Map"));
        }
    }
}