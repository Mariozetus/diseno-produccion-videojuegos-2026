#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Weasel.Utils
{
    public class GUIUtils
    {
        public static float GetArraySize(SerializedProperty array, string caption = null)
        {
            return EditorGUIUtility.singleLineHeight * (1 + array.arraySize);
        }
        public static void DisplayArray(SerializedProperty array, string caption = null)
        {
            EditorGUILayout.LabelField((caption == null) ? array.name : caption);
            EditorGUI.indentLevel++;
            if(array == null) { EditorGUI.indentLevel--; return; }
            for (int i = 0; i < array.arraySize; i++)
            {
                EditorGUILayout.PropertyField(array.GetArrayElementAtIndex(i));
            }
            EditorGUI.indentLevel--;
            //other possible methods:
            //array.InsertArrayElementAtIndex;
            //array.DeleteArrayElementAtIndex;
            //array.MoveArrayElement;
        }

    }
}
#endif
