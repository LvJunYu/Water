using UnityEditor;
using UnityEngine;

namespace LYU.WaterSystem.Data
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
            var causticsDispersion = property.FindPropertyRelative("causticsDispersion");
            var causticsSpeed = property.FindPropertyRelative("causticsSpeed");
            causticsEnable.boolValue = EditorGUILayout.Toggle("Caustics Enable", causticsEnable.boolValue);
            EditorGUILayout.PropertyField(causticsTexture, new GUIContent("Caustics Map(BA)"));
            EditorGUILayout.Slider(causticsIntensity, 0, 2, "Caustics Intensity");
            EditorGUILayout.Slider(causticsSize, 0, 2, "Caustics Size");
            EditorGUILayout.Slider(causticsOffset, -5, 5, "Caustics Offset");
            EditorGUILayout.Slider(causticsBlendDistance, 0, 10, "Caustics Transition");
            EditorGUILayout.Slider(causticsDispersion, 0, 10, "Caustics Dispersion");
            causticsSpeed.vector2Value = EditorGUILayout.Vector2Field("Caustics Speed", causticsSpeed.vector2Value);
        }
    }
}