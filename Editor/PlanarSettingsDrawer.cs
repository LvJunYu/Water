// using UnityEngine;
// using UnityEditor;
//
// namespace WaterSystem
// {
//     [CustomPropertyDrawer(typeof(PlanarReflections.PlanarReflectionSettings))]
//     public class PlanarSettingsDrawer : PropertyDrawer
//     {
//         public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//         {
//             EditorGUI.BeginProperty(position, label, property);
//
//             var resMulti = property.FindPropertyRelative("m_ResolutionMultiplier");
//             var offset = property.FindPropertyRelative("m_ClipPlaneOffset");
//             var layerMask = property.FindPropertyRelative("m_ReflectLayers");
//             var shadows = property.FindPropertyRelative("m_ReflectShadows");
//             var targetCamera = property.FindPropertyRelative("targetCamera");
//             EditorGUILayout.PropertyField(resMulti, resMultiStr);
//             EditorGUILayout.Slider(offset, -0.500f, 0.500f);
//             EditorGUILayout.PropertyField(layerMask, layerMaskStr);
//             EditorGUILayout.PropertyField(shadows, shadowsStr);
//             EditorGUILayout.PropertyField(targetCamera, targetCameraStr);
//
//             EditorGUI.EndProperty();
//         }
//
//         private static readonly GUIContent resMultiStr = new GUIContent("反射分辨率");
//         private static readonly GUIContent layerMaskStr = new GUIContent("反射Layer");
//         private static readonly GUIContent shadowsStr = new GUIContent("反射阴影");
//         private static readonly GUIContent targetCameraStr = new GUIContent("场景相机", "不设置默认第一个相机");
//     }
// }