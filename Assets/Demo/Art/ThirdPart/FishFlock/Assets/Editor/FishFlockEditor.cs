using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FishFlock
{
    static class FishFlockEditorUtil
    {
        public static readonly string[] _dontIncludeMe = new string[] { "m_Script" };
        public static GUIStyle label_style = new GUIStyle(EditorStyles.boldLabel);
    }

    [CustomEditor(typeof(FishFlockControllerGPU))]
    public class FishFlockGPUEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            FishFlockEditorUtil.label_style.fontSize = 20;

            GUILayout.Label("Fish Flock GPU", FishFlockEditorUtil.label_style);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();

            DrawPropertiesExcluding(serializedObject, FishFlockEditorUtil._dontIncludeMe);

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(FishFlockController))]
    public class FishFlockEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            FishFlockEditorUtil.label_style.fontSize = 20;
            GUILayout.Label("Fish Flock", FishFlockEditorUtil.label_style);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();

            DrawPropertiesExcluding(serializedObject, FishFlockEditorUtil._dontIncludeMe);

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(FishFlockController2))]
    public class FishFlockV2Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            FishFlockEditorUtil.label_style.fontSize = 20;
            GUILayout.Label("Fish Flock V2", FishFlockEditorUtil.label_style);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();

            DrawPropertiesExcluding(serializedObject, FishFlockEditorUtil._dontIncludeMe);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
