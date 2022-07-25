using UnityEditor;
using UnityEngine;

namespace LYU.WaterSystem.Data
{
    public abstract class BaseWaterSettingEditor : PropertyDrawer
    {
        protected abstract string settingsText { get; }
        protected abstract void Draw(SerializedProperty property);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var fold = property.FindPropertyRelative("fold");
            fold.boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(fold.boolValue, settingsText);
            if (fold.boolValue)
            {
                EditorGUI.indentLevel++;
                Draw(property);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
  
            // EditorGUILayout.BeginVertical("box");
            // EditorGUILayout.LabelField(settingsText, EditorStyles.boldLabel);
            // Draw(property);
            // EditorGUILayout.EndVertical();
            EditorGUI.EndProperty();
        }
    }
}