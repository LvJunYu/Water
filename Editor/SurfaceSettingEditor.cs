using UnityEditor;
using UnityEngine;

namespace LYU.WaterSystem.Data
{
    [CustomPropertyDrawer(typeof(SurfaceSetting))]
    public class SurfaceSettingEditor : BaseWaterSettingEditor
    {
        protected override string settingsText => "水面设置";

        protected override void Draw(SerializedProperty property)
        {
            var bumpType = property.FindPropertyRelative("bumpType");
            var scartterType = property.FindPropertyRelative("scartterType");
            var surfaceMap = property.FindPropertyRelative("surfaceMap");

            var bumpScale = property.FindPropertyRelative("bumpScale");
            var surfaceSize = property.FindPropertyRelative("surfaceSize");
            var speed = property.FindPropertyRelative("speed");
            var bumpScale2 = property.FindPropertyRelative("bumpScale2");
            var surfaceSize2 = property.FindPropertyRelative("surfaceSize2");
            var speed2 = property.FindPropertyRelative("speed2");

            var distort = property.FindPropertyRelative("distort");
            var edge = property.FindPropertyRelative("edge");
            var specularClamp = property.FindPropertyRelative("specularClamp");
            var specularIntensity = property.FindPropertyRelative("specularIntensity");
            var waterMaxVisibility = property.FindPropertyRelative("waterMaxVisibility");
            var absorptionRamp = property.FindPropertyRelative("absorptionRamp");
            var scatterRamp = property.FindPropertyRelative("scatterRamp");
            var specularRange = property.FindPropertyRelative("specularRange");

            SerializedProperty tripleNormalMap = property.FindPropertyRelative("tripleNormalMap");
            SerializedProperty bumpScale3 = property.FindPropertyRelative("bumpScale3");
            SerializedProperty surfaceSize3 = property.FindPropertyRelative("surfaceSize3");
            SerializedProperty speed3 = property.FindPropertyRelative("speed3");
            SerializedProperty shallowColor = property.FindPropertyRelative("shallowColor");
            SerializedProperty deepColor = property.FindPropertyRelative("deepColor");
            SerializedProperty additionColor = property.FindPropertyRelative("additionColor");
            SerializedProperty additionColor1 = property.FindPropertyRelative("additionColor1");
            SerializedProperty additionColor2 = property.FindPropertyRelative("additionColor2");
            SerializedProperty additionRange = property.FindPropertyRelative("additionRange");

            bumpType.enumValueIndex = GUILayout.Toolbar(bumpType.enumValueIndex, bumpTypeEnum);
            switch ((EBumpType) bumpType.enumValueIndex)
            {
                case EBumpType.Bumpmap:
                    EditorGUILayout.PropertyField(surfaceMap, surfaceMapStr);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.Slider(bumpScale, 0f, 2f, "法线强度1");
                    EditorGUILayout.Slider(surfaceSize, 0f, 10f, "波纹大小1");
                    speed.vector2Value = EditorGUILayout.Vector2Field("波纹速度2", speed.vector2Value);
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.Slider(bumpScale2, 0f, 2f, "法线强度2");
                    EditorGUILayout.Slider(surfaceSize2, 0f, 10f, "波纹大小2");
                    speed2.vector2Value = EditorGUILayout.Vector2Field("波纹速度2", speed2.vector2Value);
                    EditorGUILayout.EndVertical();
                    
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.PropertyField(tripleNormalMap, tripleNormalMapStr);
                    if(tripleNormalMap.boolValue)
                    {
                        EditorGUILayout.Slider(bumpScale3, 0f, 2f, "法线强度3");
                        EditorGUILayout.Slider(surfaceSize3, 0f, 10f, "波纹大小3");
                        speed3.vector2Value = EditorGUILayout.Vector2Field("波纹速度3", speed3.vector2Value);
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel--;
                    break;
                case EBumpType.Flowmap:
                    FlowmapDraw(property);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(surfaceMap, surfaceMapStr);
                    EditorGUILayout.Slider(bumpScale, 0f, 2f, "法线强度");
                    EditorGUILayout.Slider(surfaceSize, 0f, 10f, "波纹大小");
                    var speedVal = EditorGUILayout.Slider("波纹速度", speed.vector2Value.x, 0, 5);
                    speed.vector2Value = new Vector2(speedVal, speed.vector2Value.y);
                    EditorGUI.indentLevel--;
                    break;
            }

            EditorGUILayout.Slider(distort, 0f, 2f, "折射扭曲");
            EditorGUILayout.Slider(edge, 0.5f, 3f, "边缘过度");
            EditorGUILayout.Slider(specularIntensity, 0, 2, "高光强度");
            EditorGUILayout.Slider(specularClamp, 0, 1024, "高光Clamp");
            EditorGUILayout.Slider(specularRange, 0, 1, "高光范围修正");
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(additionColor, additionColorStr);
            if(additionColor.boolValue)
            {
                EditorGUILayout.Slider(additionRange, 0, 10, "补色远近范围");
                EditorGUILayout.PropertyField(additionColor1, additionColor1Str);
                EditorGUILayout.PropertyField(additionColor2, additionColor2Str);
            }
            EditorGUILayout.EndHorizontal();
            scartterType.enumValueIndex = GUILayout.Toolbar(scartterType.enumValueIndex, scartterTypeEnum);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.Slider(waterMaxVisibility, 0, 300, waterMaxVisibilityStr);
            switch ((EScartterType)scartterType.enumValueIndex)
            {
                case EScartterType.Simple:
                    EditorGUILayout.PropertyField(shallowColor, shallowColorStr);
                    EditorGUILayout.PropertyField(deepColor, deepColorStr);
                    break;
                case EScartterType.Detail:
                    EditorGUILayout.PropertyField(absorptionRamp, absorptionRampStr, true, null);
                    EditorGUILayout.PropertyField(scatterRamp, scatterRampStr, true, null);
                    EditorGUILayout.HelpBox("水面颜色 = 水底折射 * 吸收颜色 + 散射颜色\n" +
                                            "颜色条带代表深度从浅到深的颜色变化，最右的深度值对应参数【最大深度】", MessageType.Info);
                    break;
            }
            EditorGUILayout.EndVertical();

        }

        private void FlowmapDraw(SerializedProperty property)
        {
            var flowMap = property.FindPropertyRelative("flowMap");
            var flowNormal = property.FindPropertyRelative("flowNormal");
            //var flowMapTiling = property.FindPropertyRelative("flowMapTiling");
            var flowMapScale = property.FindPropertyRelative("flowMapScale");
            var flowNormalSize = property.FindPropertyRelative("flowNormalSize");
            var flowSpeed = property.FindPropertyRelative("flowSpeed");
            var foamMap = property.FindPropertyRelative("foamMap");
            var foamMetallic = property.FindPropertyRelative("foamMetallic");
            var foamSpecular = property.FindPropertyRelative("foamSpecular");
            var foamSmoothness = property.FindPropertyRelative("foamSmoothness");
            var foamColor = property.FindPropertyRelative("foamColor");
            var foamIntensity = property.FindPropertyRelative("foamIntensity");
            EditorGUILayout.PropertyField(flowMap, flowMapStr);
            if (flowMap.objectReferenceValue != null)
            {
                EditorGUI.indentLevel++;
                //EditorGUILayout.Slider(flowMapTiling, 0f, 100f, "流向图Tiling");
                EditorGUILayout.PropertyField(flowNormal, flowNormalStr);
                EditorGUILayout.Slider(flowMapScale, -10.0f, 10.0f, "流向图强度");
                EditorGUILayout.Slider(flowNormalSize, 0, 1, "流向法线大小");
                EditorGUILayout.Slider(flowSpeed, 0, 1, "流向图速度");
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(foamMap, foamMapStr);
                EditorGUILayout.Slider(foamIntensity, 0, 100, "白沫Intensity");
                EditorGUILayout.Slider(foamMetallic, 0, 1, "白沫金属度");
                EditorGUILayout.Slider(foamSpecular, 0, 1, "白沫高光");
                EditorGUILayout.Slider(foamSmoothness, 0, 1, "白沫粗糙度");
                EditorGUILayout.PropertyField(foamColor, foamColorStr);
                EditorGUI.indentLevel--;
            }
        }

        private static readonly GUIContent surfaceMapStr = new GUIContent("法线贴图", "RG通道");

        private static readonly GUIContent flowMapStr = new GUIContent("流向图");
        private static readonly GUIContent flowNormalStr = new GUIContent("流向法线");
        private static readonly GUIContent foamMapStr = new GUIContent("白沫图");
        private static readonly GUIContent foamColorStr = new GUIContent("白沫颜色");

        private static readonly GUIContent waterMaxVisibilityStr = new GUIContent("最大深度", maxDepthTT);
        private static readonly GUIContent absorptionRampStr = new GUIContent("吸收颜色", absorpRampTT);
        private static readonly GUIContent scatterRampStr = new GUIContent("散射颜色", scatterRampTT);

        private static readonly GUIContent tripleNormalMapStr = new GUIContent("是否使用3法线设置");
        private static readonly GUIContent shallowColorStr = new GUIContent("浅水颜色");
        private static readonly GUIContent deepColorStr = new GUIContent("深水颜色");
        private static readonly GUIContent additionColorStr = new GUIContent("是否使用补色");
        private static readonly GUIContent additionColor1Str = new GUIContent("近处颜色");
        private static readonly GUIContent additionColor2Str = new GUIContent("远处颜色");
        private static readonly string[] bumpTypeEnum = {"法线贴图", "流向图"};
        private static readonly string[] scartterTypeEnum = {"简单散射", "细节散射"};

        private const string maxDepthTT =
            "This controls the max depth of the waters transparency/visiblility, the absorption and scattering gradients map to this depth. Units:Meters";

        private const string absorpRampTT =
            "This gradient controls the color of the water as it gets deeper, darkening the surfaces under the water as they get deeper.";

        private const string scatterRampTT =
            "This gradient controls the 'scattering' of the water from shallow to deep, lighting the water as there becomes more of it.";
    }
}