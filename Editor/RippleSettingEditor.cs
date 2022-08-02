using UnityEditor;
using UnityEngine;

namespace LYU.WaterSystem.Data
{
    [CustomPropertyDrawer(typeof(RippleSetting))]
    public class RippleSettingEditor : BaseWaterSettingEditor
    {
        protected override string settingsText => "Ripple Setting";

        protected override void Draw(SerializedProperty property)
        {
            var noiseMap = property.FindPropertyRelative("noiseMap");
            var rippleEnable = property.FindPropertyRelative("rippleEnable");
            var maxRippleCount = property.FindPropertyRelative("maxRippleCount");
            
            var rippleType = property.FindPropertyRelative("rippleType");
            rippleEnable.boolValue = EditorGUILayout.Toggle("Ripple Enable", rippleEnable.boolValue);
            var intensity = property.FindPropertyRelative("intensity");
            EditorGUILayout.PropertyField(rippleType, rippleTypeStr);
            switch ((RippleSetting.RippleType) rippleType.intValue)
            {
                case RippleSetting.RippleType.Circle:
                    var waveShape = property.FindPropertyRelative("waveShape");
                    var speed = property.FindPropertyRelative("speed");
                    var frequency = property.FindPropertyRelative("frequency");
                    var lifeTime = property.FindPropertyRelative("lifeTime");
                    var param1 = property.FindPropertyRelative("param1");
                    var param2 = property.FindPropertyRelative("param2");
                    var param3 = property.FindPropertyRelative("param3");
                    var param4 = property.FindPropertyRelative("param4");
                    var param5 = property.FindPropertyRelative("param5");
                    var param6 = property.FindPropertyRelative("param6");
                    var param7 = property.FindPropertyRelative("param7");
                    var param8 = property.FindPropertyRelative("param8");
                    var waveform = property.FindPropertyRelative("waveform");
                    
                    EditorGUILayout.IntSlider(maxRippleCount, 1, RippleSetting.RippleCountLimit, "最大涟漪个数");
                    EditorGUILayout.PropertyField(waveShape, waveShapeStr);
                    if (waveShape.enumValueIndex == 2)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(waveform, waveformStr);
                        EditorGUI.indentLevel--;
                    }

                    EditorGUILayout.Slider(intensity, 0, 2, "强度");
                    EditorGUILayout.Slider(speed, 0.01f, 5, "速度");
                    EditorGUILayout.Slider(frequency, 0.01f, 3, "频率");
                    EditorGUILayout.Slider(lifeTime, 0, 10, "持续时间");
                    EditorGUILayout.Slider(param3, 0, 5, "波纹大小");
                    EditorGUILayout.Slider(param5, 0.01f, 5, "距离衰减");
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(noiseMap, noiseMapStr);
                    EditorGUILayout.Slider(param2, 0, 2, "噪声强度");
                    EditorGUILayout.Slider(param1, 0, 3, "噪声密度");
                    EditorGUILayout.Slider(param4, 0, 2, "噪声速度");
                    EditorGUILayout.Space();
                    EditorGUILayout.Slider(param6, 0f, 2, "涟漪初始时间偏移");
                    break;
                case RippleSetting.RippleType.WaveEquation:
                    EditorGUILayout.Slider(intensity, 0, 10, "Ripple Intensity");
                    var center = property.FindPropertyRelative("center");
                    var rippleRange = property.FindPropertyRelative("rippleRange");
                    var precision = property.FindPropertyRelative("precision");
                    var viscosity = property.FindPropertyRelative("viscosity");
                    var velocity = property.FindPropertyRelative("velocity");
                    EditorGUILayout.IntSlider(maxRippleCount, 1, RippleSetting.RippleCountLimit, "Max Ripple Count");
                    EditorGUILayout.PropertyField(center, centerStr);
                    EditorGUILayout.IntSlider(rippleRange, 1, 100, "Area Range (meter)");
                    EditorGUILayout.IntSlider(precision, 2, 10, "Precision (pixels per meter)");
                    EditorGUILayout.Slider(viscosity, 0, 10, "Viscosity");
                    EditorGUILayout.Slider(velocity, 0, 0.99f, "Velocity");
                    
                    // raindrop
                    var raindropCount = property.FindPropertyRelative("raindropCount");
                    var frameInternal = property.FindPropertyRelative("frameInternal");
                    var testRange = property.FindPropertyRelative("testRange");
                    var testDepth = property.FindPropertyRelative("testDepth");
                    var maxRadius = property.FindPropertyRelative("maxRadius");
                    var minRadius = property.FindPropertyRelative("minRadius");
                    EditorGUILayout.IntSlider(raindropCount, 0, RippleSetting.RippleCountLimit, "Raindrop Count");
                    EditorGUILayout.IntSlider(frameInternal, 1, 60, "Raindrop Internal (Frame)");
                    EditorGUILayout.Slider(testRange, 5, 100, "Raindrop Range");
                    EditorGUILayout.Slider(testDepth, 0, 1, "Raindrop Depth");
                    EditorGUILayout.Slider(maxRadius, 0.01f, 5f, "Raindrop Max Size");
                    EditorGUILayout.Slider(minRadius, 0.01f, 5f, "Raindrop Min Size");
                    break;
            }
        }

        private static readonly GUIContent centerStr = new GUIContent("Area Center");
        private static readonly GUIContent rippleTypeStr = new GUIContent("Ripple Type");
        private static readonly GUIContent waveShapeStr = new GUIContent("波浪类型");
        private static readonly GUIContent waveformStr = new GUIContent("自定义波浪");
        private static readonly GUIContent noiseMapStr = new GUIContent("噪声图(R)");
    }
}