using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityObject = UnityEngine.Object;
using Unity.Netcode;

namespace Unity.Netcode.Addons.Editor
{
    [CustomEditor(typeof(NetworkAssetManifest))]
    public class NetworkAssetManifestEditor : UnityEditor.Editor
    {
        public new NetworkAssetManifest target
        {
            get
            {
                return base.target as NetworkAssetManifest;
            }
        }

        private ReorderableList manifestList;
        private SerializedProperty overrideGlobalId;

        private void OnEnable()
        {
            overrideGlobalId = serializedObject.FindProperty(nameof(NetworkAssetManifest.overrideGlobalId));

            manifestList = new ReorderableList(serializedObject,
                serializedObject
                .FindProperty(nameof(NetworkAssetManifest.assetEntries)),
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

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(overrideGlobalId);

            if (EditorGUI.EndChangeCheck())
                if (overrideGlobalId.boolValue)
                    for (int i = 0; i < manifestList.serializedProperty.arraySize; i += 1)
                    {
                        var entry = manifestList.serializedProperty.GetArrayElementAtIndex(i);
                        var entryGlobalId = entry.FindPropertyRelative(nameof(NetworkAssetEntry.globalId));
                        var entryAsset = entry.FindPropertyRelative(nameof(NetworkAssetEntry.asset));

                        if (entryAsset.objectReferenceValue != null)
                        {
                            var globalObjectIdString = GlobalObjectId.GetGlobalObjectIdSlow(this).ToString();
                            entryGlobalId.longValue = XXHash.Hash32(globalObjectIdString);
                        }
                        else
                        {
                            entryGlobalId.longValue = 0;
                        }
                    }

            EditorGUILayout.Space();

            manifestList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        private void List_DrawHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "Assets");
        }
        private float List_ElementHeightCallback(int index)
        {
            return 4 + EditorGUIUtility.singleLineHeight;
        }
        private void List_DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 3;

            var entry = manifestList.serializedProperty.GetArrayElementAtIndex(index);
            var entryGlobalId = entry.FindPropertyRelative(nameof(NetworkAssetEntry.globalId));
            var entryAsset = entry.FindPropertyRelative(nameof(NetworkAssetEntry.asset));

            if (overrideGlobalId.boolValue)
            {
                EditorGUI.PropertyField(new Rect(rect.x + 10, rect.y, rect.width - 105, EditorGUIUtility.singleLineHeight), entryAsset, GUIContent.none);
                EditorGUI.PropertyField(new Rect(rect.x + (rect.width - 90), rect.y, 90, EditorGUIUtility.singleLineHeight), entryGlobalId, GUIContent.none);
            }
            else
            {
                EditorGUI.BeginChangeCheck();

                EditorGUI.PropertyField(new Rect(rect.x + 10, rect.y, rect.width - 10, EditorGUIUtility.singleLineHeight), entryAsset, GUIContent.none);
                
                if (EditorGUI.EndChangeCheck())
                {
                    var globalObjectIdString = GlobalObjectId.GetGlobalObjectIdSlow(this).ToString();
                    entryGlobalId.longValue = XXHash.Hash32(globalObjectIdString);
                }
            }
        }
        private void List_OnAddCallback(ReorderableList list)
        {
            list.serializedProperty.arraySize += 1;

            var newIndex = list.serializedProperty.arraySize - 1;
            var entry = list.serializedProperty.GetArrayElementAtIndex(newIndex);
            var entryGlobalId = entry.FindPropertyRelative(nameof(NetworkAssetEntry.globalId));
            var entryAsset = entry.FindPropertyRelative(nameof(NetworkAssetEntry.asset));

            entryGlobalId.intValue = 0;
            entryAsset.objectReferenceValue = null;            
        }
        private void List_OnRemoveCallback(ReorderableList list)
        {
            list.serializedProperty.arraySize -= 1;
        }
    }
}