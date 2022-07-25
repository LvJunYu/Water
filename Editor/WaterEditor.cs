using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using LYU.WaterSystem.Data;

namespace LYU.WaterSystem
{
    [CustomEditor(typeof(WaterSettingsData))]
    public class WaterSettingsDataEditor : Editor
    {
    }

    [CustomEditor(typeof(Water))]
    public class WaterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Water w = (Water) target;
            EditorGUI.BeginChangeCheck();
            var waterMaterial = serializedObject.FindProperty("waterMaterial");
            EditorGUILayout.PropertyField(waterMaterial);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                w.SetMaterial();
            }

            var seaSettingsData = serializedObject.FindProperty("settingsData");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(seaSettingsData, true);
            if (seaSettingsData.objectReferenceValue != null)
            {
                EditorGUILayout.EndHorizontal();
                CreateEditor((WaterSettingsData) seaSettingsData.objectReferenceValue).OnInspectorGUI();
            }
            else
            {
                if (GUI.Button(GUILayoutUtility.GetRect(20, 20), "New"))
                {
                    Water actualTarget = (Water) target;
                    seaSettingsData.objectReferenceValue =
                        CreateWaterSettingData(actualTarget.gameObject.scene, actualTarget.name);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            // var seaMaterial = serializedObject.FindProperty("_seaMaterial");
            // EditorGUILayout.PropertyField(seaMaterial, new GUIContent("Water Material"));

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                w.Refresh();
            }
        }

        static WaterSettingsData CreateWaterSettingData(Scene scene, string targetName)
        {
            string path;

            if (string.IsNullOrEmpty(scene.path))
            {
                path = "Assets/";
            }
            else
            {
                var scenePath = Path.GetDirectoryName(scene.path);
                var extPath = scene.name;
                var profilePath = scenePath + "/" + extPath;

                if (!AssetDatabase.IsValidFolder(profilePath))
                    AssetDatabase.CreateFolder(scenePath, extPath);
                path = profilePath + "/";
            }

            path += targetName + "_waterSetting.asset";
            path = AssetDatabase.GenerateUniqueAssetPath(path);

            var setting = CreateInstance<WaterSettingsData>();
            AssetDatabase.CreateAsset(setting, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return setting;
        }
    }
}