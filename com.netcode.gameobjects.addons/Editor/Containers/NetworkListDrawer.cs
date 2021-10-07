using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Netcode;

namespace Unity.Netcode.Addons.Editor
{

    [CustomPropertyDrawer(typeof(NetworkReferenceList<>), true)]
    public class NetworkListDrawer : PropertyDrawer
    {
        private ReorderableList manifestList;
        private GUIContent label;
        private INetworkListNotify networkList;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var serializedObject = property.serializedObject;
            var internalProperty = property.FindPropertyRelative("m_List");

            if (manifestList == null)
            {
                networkList = GetNetworkList(property) as INetworkListNotify;
                
                this.label = label;
                this.manifestList = new ReorderableList(
                    serializedObject,
                    internalProperty,
                    true, true, true, true);

                manifestList.drawHeaderCallback = GUI_DrawHeaderCallback;
                manifestList.elementHeightCallback = GUI_ElementHeightCallback;
                manifestList.drawElementCallback = GUI_DrawElementCallback;
                manifestList.onAddCallback = GUI_OnAddCallback;
                manifestList.onRemoveCallback = GUI_OnRemoveCallback;
                manifestList.onReorderCallbackWithDetails = GUI_OnReorderCallbackWithDetails;
            }


            EditorGUI.BeginDisabledGroup(IsReadonly());
            manifestList.DoLayoutList();
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }

        protected bool IsReadonly()
        {
            // can edit if:
            //  app is not running
            //  network is not present/connected
            //  we are the server

            // can not edit if:
            //  we are a client only

            if (Application.isPlaying == false)
                return false;
            if (NetworkManager.Singleton == null)
                return false;
            if (NetworkManager.Singleton.IsServer)
                return false;
            if (NetworkManager.Singleton.IsClient)
                return true;
            return false;
        }
        protected bool IsNetworkRunning()
        {
            if (Application.isPlaying == false)
                return false;
            if (NetworkManager.Singleton == null)
                return false;
            if (NetworkManager.Singleton.IsHost)
                return true;
            if (NetworkManager.Singleton.IsServer)
                return true;
            if (NetworkManager.Singleton.IsClient)
                return true;
            return false;
        }
        protected object GetNetworkList(SerializedProperty property)
        {
            return fieldInfo.GetValue(property.serializedObject.targetObject);
        }

        private void GUI_DrawHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, label);
        }
        private float GUI_ElementHeightCallback(int index)
        {
            return 4 + EditorGUIUtility.singleLineHeight;
        }
        private void GUI_DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 3;
            var entry = manifestList.serializedProperty.GetArrayElementAtIndex(index);

            EditorGUI.BeginChangeCheck();

            EditorGUI.PropertyField(
                new Rect(rect.x + 10, rect.y, rect.width - 10, EditorGUIUtility.singleLineHeight), 
                entry, 
                GUIContent.none
                );

            if (EditorGUI.EndChangeCheck())
            {
                entry.serializedObject.ApplyModifiedProperties();

                if (IsNetworkRunning())
                {
                    // inform the list of the change
                    networkList.Notify_ElementAssigned(index);
                }
            }

        }
        private void GUI_OnAddCallback(ReorderableList list)
        {
            // increase the length of the list by one
            list.serializedProperty.arraySize += 1;
            list.serializedProperty.serializedObject.ApplyModifiedProperties();

            if (IsNetworkRunning())
            {
                // inform the list of the change
                networkList.Notify_ElementAdded(list.serializedProperty.arraySize - 1);
            }
        }
        private void GUI_OnRemoveCallback(ReorderableList list)
        {
            var removeAt = list.index;

            // move this element to the end of the list
            list.serializedProperty.MoveArrayElement(removeAt, list.serializedProperty.arraySize - 1);
            // shink the list by one
            list.serializedProperty.arraySize -= 1;
            // apply the changes to the serializedobject
            list.serializedProperty.serializedObject.ApplyModifiedProperties();

            if (IsNetworkRunning())
            {
                // inform the list of the change
                networkList.Notify_ElementRemovedAt(removeAt);
            }

            // we deleted the last element
            if (removeAt == list.serializedProperty.arraySize)
            {
                // reselect the last element
                list.index = removeAt - 1;
            }
        }
        private void GUI_OnReorderCallbackWithDetails(ReorderableList list, int oldIndex, int newIndex)
        {
            if (IsNetworkRunning())
            {
                // apply the changes to the serializedobject
                list.serializedProperty.serializedObject.ApplyModifiedProperties();
                // inform the list of the change
                networkList.Notify_ElementMoved(newIndex, oldIndex);
            }
        }


    }
}
