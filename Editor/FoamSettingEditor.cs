using UnityEditor;
using UnityEngine;

namespace LYU.WaterSystem.Data
{
    [CustomPropertyDrawer(typeof(FoamSetting))]
    public class FoamSettingEditor : BaseWaterSettingEditor
    {
        protected override string settingsText => "泡沫设置";

        protected override void Draw(SerializedProperty property)
        {
            var foamType = property.FindPropertyRelative("foamType");
            var foamEnable = property.FindPropertyRelative("foamEnable");
            var foamColor = property.FindPropertyRelative("foamColor");
            var foamIntensity = property.FindPropertyRelative("foamIntensity");
            var foamParam1 = property.FindPropertyRelative("foamParam1");
            var foamParam2 = property.FindPropertyRelative("foamParam2");
            var foamParam3 = property.FindPropertyRelative("foamParam3");
            var foamParam4 = property.FindPropertyRelative("foamParam4");
            var foamParam5 = property.FindPropertyRelative("foamParam5");
            var foamParam6 = property.FindPropertyRelative("foamParam6");
            
            foamEnable.boolValue = EditorGUILayout.Toggle("泡沫开关", foamEnable.boolValue);
            EditorGUILayout.Slider(foamIntensity, 0, 2, "泡沫强度");
            EditorGUILayout.PropertyField(foamColor, foamColorStr);
            foamType.intValue = GUILayout.Toolbar(foamType.intValue, foamTypeEnum);
            switch ((EFoamType) foamType.intValue)
            {
                case EFoamType.SeaFoam:
                    var shallowsHeight = property.FindPropertyRelative("shallowsHeight");
                    var foamMap = property.FindPropertyRelative("foamMap");
                    var bakedDepthTex = property.FindPropertyRelative("bakedDepthTex");
                    var defaultFoamRamp = property.FindPropertyRelative("defaultFoamRamp");
                    var basicFoam = property.FindPropertyRelative("basicFoam");
                    EditorGUILayout.Slider(shallowsHeight, 0, 1, "Shore Range Expansion");
                    EditorGUILayout.PropertyField(foamMap, foamMapStr);
                    EditorGUILayout.Slider(foamParam1, 0, 2f, "Foam Map Tiling");
                    EditorGUILayout.Slider(foamParam2, 0, 2f, "Shore Wave Tiling");
                    EditorGUILayout.Slider(foamParam3, 0, 2f, "Shore Wave Speed");
                    EditorGUILayout.Slider(foamParam4, 0, 2f, "Shore Wave Gradient");
                    EditorGUILayout.Slider(foamParam5, 0, 2f, "Shore Wave Intensity");
                    EditorGUILayout.Slider(foamParam6, 0, 10f, "Shore Wave Power");
                    EditorGUILayout.PropertyField(bakedDepthTex, bakedDepthTexStr);
                    if (!bakedDepthTex.objectReferenceValue)
                        EditorGUILayout.HelpBox("点击Water脚本右上角【获取高度图】进行Bake\nBake前需要把海底物体的Layer设置成SeaFloor",
                            MessageType.Info);
                    EditorGUILayout.PropertyField(defaultFoamRamp, defaultFoamRampStr);
                    ShowCurve(basicFoam, "泡沫稠密曲线");
                    break;
                case EFoamType.RiverFoam:
                    var noiseMap = property.FindPropertyRelative("noiseMap");
                    var foamParam7 = property.FindPropertyRelative("foamParam7");
                    var foamParam8 = property.FindPropertyRelative("foamParam8");
                    var foamParam9 = property.FindPropertyRelative("foamParam9");
                    var foamParam10 = property.FindPropertyRelative("foamParam10");

                    EditorGUILayout.PropertyField(noiseMap, noiseMapStr);
                    EditorGUILayout.Slider(foamParam2, 0, 2, "噪声Size");
                    EditorGUILayout.Slider(foamParam1, 0, 2, "泡沫频率");
                    EditorGUILayout.Slider(foamParam4, 0.01f, 1, "泡沫厚度");
                    EditorGUILayout.Slider(foamParam3, 0.01f, 10f, "泡沫范围");
                    EditorGUILayout.Slider(foamParam8, 0, 2, "泡沫速度");

                    EditorGUILayout.Space();
                    EditorGUILayout.Slider(foamParam7, 0, 2, "噪声强度");
                    EditorGUILayout.Slider(foamParam5, 0.01f, 10f, "泡沫边缘范围");
                    EditorGUILayout.Slider(foamParam6, 0, 2, "泡沫边缘亮度");
                    EditorGUILayout.Slider(foamParam10, 0, 2, "泡沫边缘阈值");
                    break;
            }
        }

        void ShowCurve(SerializedProperty property, string name, string tips = null)
        {
            EditorGUILayout.BeginHorizontal();
            DoInlineLabel(name, tips, 50f);
            property.animationCurveValue = EditorGUILayout.CurveField(property.animationCurveValue,
                Color.white, new Rect(Vector2.zero, Vector2.one));
            EditorGUILayout.EndHorizontal();
        }

        void DoInlineLabel(string label, string tooltip, float width)
        {
            var preWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = width;
            EditorGUILayout.LabelField(new GUIContent(label, tooltip));
            EditorGUIUtility.labelWidth = preWidth;
        }

        private static readonly string[] foamTypeEnum = {"海水泡沫", "河水泡沫"};
        private static readonly GUIContent foamColorStr = new GUIContent("泡沫颜色");
        private static readonly GUIContent foamMapStr2 = new GUIContent("泡沫贴图(RG)");
        private static readonly GUIContent foamMapStr = new GUIContent("Foam Map(RGB)");
        private static readonly GUIContent bakedDepthTexStr = new GUIContent("Height Map", "Editor下点击Water脚本右上角获取高度图");
        private static readonly GUIContent defaultFoamRampStr = new GUIContent("Foam Ramp Map");
        private static readonly GUIContent noiseMapStr = new GUIContent("噪声图");
    }
}