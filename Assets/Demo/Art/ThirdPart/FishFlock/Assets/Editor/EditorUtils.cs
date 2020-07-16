using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FishFlock
{
    [CustomPropertyDrawer(typeof(CustomTitle))]
    public class CustomTitleDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            CustomTitle bv = attribute as CustomTitle;

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField(bv.title, EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();

           
            {
                EditorGUILayout.PropertyField(property);
            }
        }
    }
}