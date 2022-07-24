using UnityEditor;
using UnityEngine;

namespace WaterSystem.Data
{
    [CustomPropertyDrawer(typeof(CausticsSetting))]
    public class CausticsSettingEditor : BaseWaterSettingEditor
    {
        protected override string settingsText => "焦散设置";

        protected override void Draw(SerializedProperty property)
        {
            var causticsEnable = property.FindPropertyRelative("causticsEnable");
            var causticsTexture = property.FindPropertyRelative("causticsTexture");
            var causticsIntensity = property.FindPropertyRelative("causticsIntensity");
            var causticsSize = property.FindPropertyRelative("causticsSize");
            var causticsOffset = property.FindPropertyRelative("causticsOffset");
            var causticsBlendDistance = property.FindPropertyRelative("causticsBlendDistance");
            causticsEnable.boolValue = EditorGUILayout.Toggle("焦散开关", causticsEnable.boolValue);
            EditorGUILayout.PropertyField(causticsTexture, new GUIContent("焦散贴图(BA)"));
            EditorGUILayout.Slider(causticsIntensity, 0, 2, "强度");
            EditorGUILayout.Slider(causticsSize, 0, 2, "大小");
            EditorGUILayout.Slider(causticsOffset, -5, 5, "起始位置偏移");
            EditorGUILayout.Slider(causticsBlendDistance, 0, 10, "混合距离");
        }
    }
}