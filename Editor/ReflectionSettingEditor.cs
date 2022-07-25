using UnityEditor;
using UnityEngine;

namespace LYU.WaterSystem.Data
{
    [CustomPropertyDrawer(typeof(ReflectionSetting))]
    public class ReflectionSettingEditor : BaseWaterSettingEditor
    {
        protected override string settingsText => "反射设置";

        protected override void Draw(SerializedProperty property)
        {
            var reflectionEnable = property.FindPropertyRelative("reflectionEnable");
            var refType = property.FindPropertyRelative("refType");
            var fresnelPower = property.FindPropertyRelative("fresnelPower");
            var reflectDistort = property.FindPropertyRelative("reflectDistort");
            var reflectIntensity = property.FindPropertyRelative("reflectIntensity");

            reflectionEnable.boolValue = EditorGUILayout.Toggle("反射开关", reflectionEnable.boolValue);
            EditorGUILayout.Slider(reflectIntensity, 0, 2, "反射强度");
            EditorGUILayout.IntSlider(fresnelPower, 0, 20, "菲涅尔强度");
            EditorGUILayout.Slider(reflectDistort, 0, 1, "反射扰动");
            refType.enumValueIndex = GUILayout.Toolbar(refType.enumValueIndex, relfectEnum);
            switch ((ReflectionType) refType.enumValueIndex)
            {
                case ReflectionType.Cubemap:
                    var cube = property.FindPropertyRelative("cubemapTexture");
                    EditorGUILayout.PropertyField(cube, new GUIContent("反射贴图"));
                    break;
                case ReflectionType.ReflectionProbe:
                    EditorGUILayout.HelpBox("自动使用场景设置的环境反射，或反射探针", MessageType.Info);
                    break;
                case ReflectionType.PlanarReflection:
                    EditorGUILayout.HelpBox("镜面反射开销较高，建议过滤不必要的Layer，降低反射分辨率", MessageType.Info);
                    var planarSettings = property.FindPropertyRelative("planarSettings");
                    EditorGUILayout.PropertyField(planarSettings, true);
                    break;
                case ReflectionType.SSPR:
                    var ssprResolution = property.FindPropertyRelative("ssprResolution");
                    EditorGUILayout.HelpBox("需要添加SSPlanarReflectionFeature\n降低反射分辨率可提高性能", MessageType.Info);
                    EditorGUILayout.PropertyField(ssprResolution, resolutionStr);
                    break;
                case ReflectionType.TD_SSPR:
                    SerializedProperty MarchParam = property.FindPropertyRelative("MarchParam");
                    EditorGUILayout.HelpBox("需要添加Copy Color Pass", MessageType.Info);
                    EditorGUILayout.PropertyField(MarchParam, MarchParamStr);
                    break;
            }
        }

        private static GUIContent resolutionStr = new GUIContent("反射分辨率", "降低分辨率可提高性能");
        private static GUIContent MarchParamStr = new GUIContent("反射经验参数", "增加反射真实度，减少穿帮");
        private static readonly string[] relfectEnum = {"反射贴图", "环境反射", "镜面反射", "屏幕空间镜面反射", "天刀SSPR"};
    }
}