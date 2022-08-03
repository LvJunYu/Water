using System;
using UnityEditor;
using UnityEngine;

// ReSharper disable PossibleLossOfFraction

namespace LYU.WaterSystem.Data
{
    [Serializable]
    [CustomPropertyDrawer(typeof(WaveSetting))]
    public class WaveSettingEditor : BaseWaterSettingEditor
    {
        protected override string settingsText => "Wave Setting";

        protected override void Draw(SerializedProperty property)
        {
            var waveEnable = property.FindPropertyRelative("waveEnable");
            var basicSettings = property.FindPropertyRelative("_basicWaveSettings");
            var autoCount = basicSettings.FindPropertyRelative("numWaves");
            var avgHeight = basicSettings.FindPropertyRelative("amplitude");
            var avgWavelength = basicSettings.FindPropertyRelative("wavelength");
            var windDir = basicSettings.FindPropertyRelative("direction");
            var waveSpeed = property.FindPropertyRelative("waveSpeed");
            var speedRandom = property.FindPropertyRelative("speedRandom");
            var sharpness = property.FindPropertyRelative("sharpness");

            waveEnable.boolValue = EditorGUILayout.Toggle("Wave Enable", waveEnable.boolValue);
            // Wave count (display warning of on mobile platform and over 6) dropdown  1 > 10
            EditorGUILayout.IntSlider(autoCount, 1, WaveSetting.MaxWaveCount, waveCountStr, null);
            // if (autoCount.intValue > 7)
            //     EditorGUILayout.HelpBox("移动平台建议叠加浪的个数不要过多" , MessageType.Info);
            EditorGUILayout.Slider(avgHeight, 0.01f, 10.0f, waveHeightStr, null);
            EditorGUILayout.Slider(avgWavelength, 1.0f, 100.0f, waveLengthStr, null);
            EditorGUILayout.Slider(waveSpeed, 0f, 3f, "Wave Speed");
            EditorGUI.indentLevel++;
            EditorGUILayout.Slider(speedRandom, 0f, 1f, "Wave Speed Random");
            EditorGUI.indentLevel--;
            EditorGUILayout.Slider(sharpness, 0f, 10f, "Wave Sharpness");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Slider(windDir, -180.0f, 180.0f, windDirStr, null);
            if (GUILayout.Button(windButtonStr))
                windDir.floatValue = CameraRelativeDirection();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            var randSeed = property.FindPropertyRelative("randomSeed");
            randSeed.intValue = EditorGUILayout.IntField(randomSeedStr, randSeed.intValue);
            if (GUILayout.Button("Random"))
                randSeed.intValue = DateTime.Now.Millisecond * 100 - DateTime.Now.Millisecond;
            EditorGUILayout.EndHorizontal();
            if (waveEnable.boolValue)
                SubSurfaceDraw(property);
        }

        float CameraRelativeDirection()
        {
            float degrees;
            Vector3 camFwd = SceneView.lastActiveSceneView.camera.transform.forward;
            camFwd.y = 0f;
            camFwd.Normalize();
            float dot = Vector3.Dot(-Vector3.forward, camFwd);
            degrees = Mathf.LerpUnclamped(90.0f, 180.0f, dot);
            if (camFwd.x < 0)
                degrees *= -1f;
            return Mathf.RoundToInt(degrees * 1000) / 1000;
        }

        private void SubSurfaceDraw(SerializedProperty property)
        {
            EditorGUILayout.Space();

            var subSurfaceColor = property.FindPropertyRelative("subSurfaceColor");
            var subSurfaceSunFallOff = property.FindPropertyRelative("subSurfaceSunFallOff");
            var subSurfaceBase = property.FindPropertyRelative("subSurfaceBase");
            var subSurfaceSun = property.FindPropertyRelative("subSurfaceSun");
            var subSurfaceScale = property.FindPropertyRelative("subSurfaceScale");
            var subSurfaceEnable = property.FindPropertyRelative("subSurfaceEnable");
            subSurfaceEnable.boolValue = EditorGUILayout.Toggle(subSurfaceStr, subSurfaceEnable.boolValue);
            // EditorGUILayout.HelpBox("增强浪尖、向光方向的散射效果", MessageType.Info);
            if (subSurfaceEnable.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(subSurfaceColor, subSurfaceColorStr);
                EditorGUILayout.Slider(subSurfaceBase, 0f, 1f, "SSS Intensity");
                EditorGUILayout.Slider(subSurfaceSun, 0f, 5f, "SSS Reinforce Towards Sun");
                EditorGUILayout.Slider(subSurfaceSunFallOff, 1f, 10f, "SSS Reinforce Range Towards Sun");
                EditorGUILayout.Slider(subSurfaceScale, 0f, 1f, "SSS Transition");
                // var testUv = property.FindPropertyRelative("testUv");
                // EditorGUILayout.Slider(testUv, 0f, 1f, "测试");
                EditorGUI.indentLevel--;
            }
        }

        private static readonly GUIContent subSurfaceStr = new GUIContent("SSS Enable", "增强浪尖、向光方向的散射效果");
        private static readonly GUIContent subSurfaceColorStr = new GUIContent("SSS Color");

        private static readonly GUIContent windButtonStr = new GUIContent("Scene Dir", alignButtonTT);
        private static readonly GUIContent windDirStr = new GUIContent("Wind Direction", windDirTT);
        private static readonly GUIContent waveLengthStr = new GUIContent("Wave Length", avgWavelengthTT);
        private static readonly GUIContent waveHeightStr = new GUIContent("Wave Amplitude", avgHeightTT);
        private static readonly GUIContent waveCountStr = new GUIContent("Wave Overlay Counts", waveCountTT);
        private static readonly GUIContent randomSeedStr = new GUIContent("Random Seed", randSeedTT);

        private static string[] wavesTypeOptions =
        {
            "Automatic", "Customized"
        };

        private const string waveCountTT =
            "Number of waves the automatic setup creates, if aiming for mobile set to 6 or less";

        private const string avgHeightTT = "The average height of the waves. Units:Meters";
        private const string avgWavelengthTT = "The average wavelength of the waves. Units:Meters";
        private const string windDirTT = "The general wind direction, this is in degrees from Z+";

        private const string alignButtonTT =
            "This aligns the wave direction to the current scene view camera facing direction";

        private const string randSeedTT = "This seed controls the automatic wave generation";
    }
}