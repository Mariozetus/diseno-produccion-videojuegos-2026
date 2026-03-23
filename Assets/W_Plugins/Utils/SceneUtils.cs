using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Weasel.Utils
{
    public class SceneUtils
    {
#if UNITY_EDITOR
        public static T[] FindObjectsOfTypeAll<T>() where T : Object
        {
#if UNITY_2020_1_OR_NEWER
            return Object.FindObjectsOfType<T>(true);
#else
        List<T> results = new List<T>();
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            var s = EditorSceneManager.GetSceneAt(i);
            if (s.isLoaded)
            {
                var allGameObjects = s.GetRootGameObjects();
                for (int j = 0; j < allGameObjects.Length; j++)
                {
                    var go = allGameObjects[j];
                    results.AddRange(go.GetComponentsInChildren<T>(true));
                }
            }
        }
        return results.ToArray();
#endif
        }

        public static T[] GetAllInstances<T>() where T : ScriptableObject
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);  //FindAssets uses tags check documentation for more info
            T[] a = new T[guids.Length];
            for (int i = 0; i < guids.Length; i++)         //probably could get optimized 
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                a[i] = AssetDatabase.LoadAssetAtPath<T>(path);
            }

            return a;
        }
#endif

        public static GameObject GetFurthestParent(GameObject go)
        {
            if (go.transform.parent == null)
            {
                return go;
            }
            Transform parent = go.transform.parent;
            bool running = true;
            while (running)
            {
                if (parent.parent == null || !parent.parent.gameObject)
                {
                    running = false;
                }
                else
                {
                    parent = parent.parent;
                }
            }
            return parent.gameObject;
        }
    }
}