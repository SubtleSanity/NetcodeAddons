using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityObject = UnityEngine.Object;
using System.Reflection;
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

        private void OnEnable()
        {
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

            EditorGUI.BeginChangeCheck();

            EditorGUI.PropertyField(new Rect(rect.x + 10, rect.y, rect.width - 10, EditorGUIUtility.singleLineHeight), entryAsset, GUIContent.none);

            if (EditorGUI.EndChangeCheck())
            {
                var globalObjectIdString = GlobalObjectId.GetGlobalObjectIdSlow(this).ToString();
                entryGlobalId.longValue = GetHash32(globalObjectIdString);
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

        private uint GetHash32(string seed)
        {
            var assembly = typeof(NetworkObject).Assembly;
            var hashType = assembly?.GetType("Unity.Netcode.XXHash");
            var field = hashType?.GetMethod("Hash32", new Type[] { typeof(string) });
           
            if (field == null)
            {
                throw new Exception("Unity.Netcode.XXHash.Hash32() is missing. Did XXHash get refactored?");
            }

            return (uint)field.Invoke(null, new object[] { seed });
        }
    }
}
