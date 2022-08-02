using UnityEditor;
using UnityEngine;

namespace LYU.WaterSystem.Data
{
    [CustomPropertyDrawer(typeof(SurfaceSetting))]
    public class SurfaceSettingEditor : BaseWaterSettingEditor
    {
        protected override string settingsText => "Surface Setting";

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
            var dispersion = property.FindPropertyRelative("dispersion");
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
                    EditorGUILayout.Slider(bumpScale, 0f, 2f, "Bump 1 Intensity");
                    EditorGUILayout.Slider(surfaceSize, 0f, 1f, "Bump 1 Tiling");
                    speed.vector2Value = EditorGUILayout.Vector2Field("Bump 1 Speed", speed.vector2Value);
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.Slider(bumpScale2, 0f, 2f, "Bump 2 Intensity");
                    EditorGUILayout.Slider(surfaceSize2, 0f, 1f, "Bump 2 Tiling");
                    speed2.vector2Value = EditorGUILayout.Vector2Field("Bump 2 Speed", speed2.vector2Value);
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.PropertyField(tripleNormalMap, tripleNormalMapStr);
                    if (tripleNormalMap.boolValue)
                    {
                        EditorGUILayout.Slider(bumpScale3, 0f, 2f, "Bump 3 Intensity");
                        EditorGUILayout.Slider(surfaceSize3, 0f, 4f, "Bump 3 Tiling");
                        speed3.vector2Value = EditorGUILayout.Vector2Field("Bump 3 Speed", speed3.vector2Value);
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel--;
                    break;
                case EBumpType.Flowmap:
                    FlowmapDraw(property);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(surfaceMap, surfaceMapStr);
                    EditorGUILayout.Slider(bumpScale, 0f, 2f, "Bump Intensity");
                    EditorGUILayout.Slider(surfaceSize, 0f, 10f, "Bump Tiling");
                    var speedVal = EditorGUILayout.Slider("Bump Speed", speed.vector2Value.x, 0, 5);
                    speed.vector2Value = new Vector2(speedVal, speed.vector2Value.y);
                    EditorGUI.indentLevel--;
                    break;
            }

            EditorGUILayout.Slider(distort, 0f, 5f, "Refract Distortion");
            EditorGUILayout.Slider(dispersion, 0f, 5f, "Refract Dispersion");
            EditorGUILayout.Slider(edge, 0.01f, 2f, "Shore Transition");
            EditorGUILayout.Slider(specularIntensity, 0, 2, "Specular Intensity");
            EditorGUILayout.Slider(specularClamp, 0, 1024, "Specular Clamp");
            EditorGUILayout.Slider(specularRange, 0, 1, "Specular Range");
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(additionColor, additionColorStr);
            if (additionColor.boolValue)
            {
                EditorGUILayout.Slider(additionRange, 0, 10, "Additional Color Rage");
                EditorGUILayout.PropertyField(additionColor1, additionColor1Str);
                EditorGUILayout.PropertyField(additionColor2, additionColor2Str);
            }

            EditorGUILayout.EndHorizontal();
            scartterType.enumValueIndex = GUILayout.Toolbar(scartterType.enumValueIndex, scartterTypeEnum);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.Slider(waterMaxVisibility, 0, 300, waterMaxVisibilityStr);
            switch ((EScartterType) scartterType.enumValueIndex)
            {
                case EScartterType.Simple:
                    EditorGUILayout.PropertyField(shallowColor, shallowColorStr);
                    EditorGUILayout.PropertyField(deepColor, deepColorStr);
                    break;
                case EScartterType.Detail:
                    EditorGUILayout.PropertyField(absorptionRamp, absorptionRampStr, true, null);
                    EditorGUILayout.PropertyField(scatterRamp, scatterRampStr, true, null);
                    EditorGUILayout.HelpBox("Surface Color = Refraction * Absorbed Color + Scattering Color\n" +
                                            "The strip represents color change with water depth, and the right-most depth value is [maximum depth]", MessageType.Info);
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
                EditorGUILayout.Slider(flowMapScale, -10.0f, 10.0f, "Flow Intensity");
                EditorGUILayout.Slider(flowNormalSize, 0, 1, "Flow Bump Tiling");
                EditorGUILayout.Slider(flowSpeed, 0, 1, "Flow Speed");
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(foamMap, foamMapStr);
                EditorGUILayout.Slider(foamIntensity, 0, 100, "Foam Intensity");
                EditorGUILayout.Slider(foamMetallic, 0, 1, "Foam Metal");
                EditorGUILayout.Slider(foamSpecular, 0, 1, "Foam Specular");
                EditorGUILayout.Slider(foamSmoothness, 0, 1, "Foam Smoothness");
                EditorGUILayout.PropertyField(foamColor, foamColorStr);
                EditorGUI.indentLevel--;
            }
        }

        private static readonly GUIContent surfaceMapStr = new GUIContent("Bump Map(XY)", "Bump Map(XY)");

        private static readonly GUIContent flowMapStr = new GUIContent("Flow Map");
        private static readonly GUIContent flowNormalStr = new GUIContent("Flow Bump");
        private static readonly GUIContent foamMapStr = new GUIContent("Foam Map");
        private static readonly GUIContent foamColorStr = new GUIContent("Foam Color");

        private static readonly GUIContent waterMaxVisibilityStr = new GUIContent("Max Depth", maxDepthTT);
        private static readonly GUIContent absorptionRampStr = new GUIContent("Absorbed Color", absorpRampTT);
        private static readonly GUIContent scatterRampStr = new GUIContent("Scattering Color", scatterRampTT);

        private static readonly GUIContent tripleNormalMapStr = new GUIContent("Enable Bump 3");
        private static readonly GUIContent shallowColorStr = new GUIContent("Shallow Color");
        private static readonly GUIContent deepColorStr = new GUIContent("Deep Color");
        private static readonly GUIContent additionColorStr = new GUIContent("Additional Color Enable");
        private static readonly GUIContent additionColor1Str = new GUIContent("Near Color");
        private static readonly GUIContent additionColor2Str = new GUIContent("Far Color");
        private static readonly string[] bumpTypeEnum = {"Bump Map", "Flow Map"};
        private static readonly string[] scartterTypeEnum = {"Simply", "Advanced"};

        private const string maxDepthTT =
            "This controls the max depth of the waters transparency/visiblility, the absorption and scattering gradients map to this depth. Units:Meters";

        private const string absorpRampTT =
            "This gradient controls the color of the water as it gets deeper, darkening the surfaces under the water as they get deeper.";

        private const string scatterRampTT =
            "This gradient controls the 'scattering' of the water from shallow to deep, lighting the water as there becomes more of it.";
    }
}