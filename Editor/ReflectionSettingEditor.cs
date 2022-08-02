using UnityEditor;
using UnityEngine;

namespace LYU.WaterSystem.Data
{
    [CustomPropertyDrawer(typeof(ReflectionSetting))]
    public class ReflectionSettingEditor : BaseWaterSettingEditor
    {
        protected override string settingsText => "Refection Setting";

        protected override void Draw(SerializedProperty property)
        {
            var reflectionEnable = property.FindPropertyRelative("reflectionEnable");
            var refType = property.FindPropertyRelative("refType");
            var fresnelPower = property.FindPropertyRelative("fresnelPower");
            var reflectDistort = property.FindPropertyRelative("reflectDistort");
            var reflectIntensity = property.FindPropertyRelative("reflectIntensity");

            reflectionEnable.boolValue = EditorGUILayout.Toggle("Refection Enable", reflectionEnable.boolValue);
            EditorGUILayout.Slider(reflectIntensity, 0, 2, "Refection Intensity");
            EditorGUILayout.IntSlider(fresnelPower, 0, 20, "Fresnel Power");
            EditorGUILayout.Slider(reflectDistort, 0, 1, "Reflect Distort");
            refType.enumValueIndex = GUILayout.Toolbar(refType.enumValueIndex, relfectEnum);
            switch ((ReflectionType) refType.enumValueIndex)
            {
                case ReflectionType.Cubemap:
                    var cube = property.FindPropertyRelative("cubemapTexture");
                    EditorGUILayout.PropertyField(cube, new GUIContent("Cubemap"));
                    break;
                case ReflectionType.ReflectionProbe:
                    EditorGUILayout.HelpBox("Use scene Settings for reflection, or reflection probes", MessageType.Info);
                    break;
                case ReflectionType.PlanarReflection:
                    EditorGUILayout.HelpBox(
                        "The overhead of planar reflection is high. It is recommended to filter unnecessary layers and decrease resolution",
                        MessageType.Info);
                    var planarSettings = property.FindPropertyRelative("planarSettings");
                    EditorGUILayout.PropertyField(planarSettings, true);
                    break;
                case ReflectionType.SSPR:
                    var ssprResolution = property.FindPropertyRelative("ssprResolution");
                    EditorGUILayout.HelpBox(
                        "Need to Add [SSPlanarReflectionFeature]\nReducing the reflection resolution improves performance",
                        MessageType.Info);
                    EditorGUILayout.PropertyField(ssprResolution, resolutionStr);
                    break;
                case ReflectionType.TD_SSPR:
                    SerializedProperty MarchParam = property.FindPropertyRelative("MarchParam");
                    EditorGUILayout.HelpBox("Need to Add Copy Color Pass", MessageType.Info);
                    EditorGUILayout.PropertyField(MarchParam, MarchParamStr);
                    break;
            }
        }

        private static GUIContent resolutionStr =
            new GUIContent("Reflection Resolution", "Reducing the resolution improves performance");

        private static GUIContent MarchParamStr = new GUIContent("Reflection Parameters", "Reflection Parameters");

        private static readonly string[] relfectEnum =
            {"Cubemap", "Reflection Probe", "Planar Reflection", "SSPR", "Faked SSPR"};
    }
}