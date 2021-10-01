using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityObject = UnityEngine.Object;

namespace Unity.Netcode.Addons.Editor
{
    [CustomEditor(typeof(NetworkAssetManager))]
    public class NetworkAssetManagerEditor : UnityEditor.Editor
    {
        private const string buttonText = "Find and add all Manifests in Project";
        private const string buttonTip = "Search the projects assets to find all NetworkAssetManifest scriptable objects and add them to the NetworkAssetManager.";

        public new NetworkAssetManager target
        {
            get
            {
                return base.target as NetworkAssetManager;
            }
        }

        private ReorderableList manifestList;

        private void OnEnable()
        {
            manifestList = new ReorderableList(serializedObject,
                serializedObject
                .FindProperty(nameof(NetworkAssetManager.initialManifests)),
                true, true, true, true);

            manifestList.drawHeaderCallback = List_DrawHeaderCallback;
            manifestList.elementHeightCallback = List_ElementHeightCallback;
            manifestList.drawElementCallback = List_DrawElementCallback;
            manifestList.onAddCallback = List_OnAddCallback;
            manifestList.onRemoveCallback = List_OnRemoveCallback;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();

            EditorGUILayout.Space(10);
            manifestList.DoLayoutList();

            EditorGUILayout.Space(10);
            if (GUILayout.Button(new GUIContent(buttonText, buttonTip)))
            {
                // clear the array
                manifestList.serializedProperty.arraySize = 0;

                foreach(var manifest in GetAllInstances<NetworkAssetManifest>())
                {
                    manifestList.serializedProperty.arraySize += 1;

                    var newIndex = manifestList.serializedProperty.arraySize - 1;
                    var newEntry = manifestList.serializedProperty.GetArrayElementAtIndex(newIndex);

                    newEntry.objectReferenceValue = manifest;
                }
            }

            EditorGUILayout.Space(10);
            serializedObject.ApplyModifiedProperties();
        }

        private void List_DrawHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "Initial Manifests");
        }
        private float List_ElementHeightCallback(int index)
        {
            return 4 + EditorGUIUtility.singleLineHeight;
        }
        private void List_DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 3;

            var manifest = manifestList.serializedProperty.GetArrayElementAtIndex(index);

            EditorGUI.PropertyField(new Rect(rect.x + 10, rect.y, rect.width - 10, EditorGUIUtility.singleLineHeight), manifest, GUIContent.none);

        }
        private void List_OnAddCallback(ReorderableList list)
        {
            list.serializedProperty.arraySize += 1;

            var newIndex = list.serializedProperty.arraySize - 1;
            var manifest = list.serializedProperty.GetArrayElementAtIndex(newIndex);

            manifest.objectReferenceValue = null;
        }
        private void List_OnRemoveCallback(ReorderableList list)
        {
            list.serializedProperty.arraySize -= 1;
        }
        private T[] GetAllInstances<T>() where T : ScriptableObject
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
    }
}
