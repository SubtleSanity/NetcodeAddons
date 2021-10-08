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
        private ReorderableList drawList;
        private GUIContent label;
        private INetworkListNotify networkList;
        private Type elementType;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var serializedObject = property.serializedObject;
            var internalProperty = property.FindPropertyRelative("m_List");

            if (drawList == null)
            {
                networkList = GetNetworkList(property) as INetworkListNotify;
                elementType = GetElementType(GetNetworkList(property));
                
                this.label = label;
                this.drawList = new ReorderableList(
                    serializedObject,
                    internalProperty,
                    true, true, true, true);

                drawList.drawHeaderCallback = GUI_DrawHeaderCallback;
                drawList.elementHeightCallback = GUI_ElementHeightCallback;
                drawList.drawElementCallback = GUI_DrawElementCallback;
                drawList.onAddCallback = GUI_OnAddCallback;
                drawList.onRemoveCallback = GUI_OnRemoveCallback;
                drawList.onReorderCallbackWithDetails = GUI_OnReorderCallbackWithDetails;
            }


            EditorGUI.BeginDisabledGroup(IsReadonly());
            drawList.DoLayoutList();
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
        protected Type GetElementType(object networkList)
        {
            var baseType = typeof(NetworkReferenceList<>);
            var listType = networkList.GetType();

            // go up the chain of inheritance to find the generic base class NetworkReferenceList<>
            // if we hit object it means the drawer has been mis-applied to an unsupported type
            while (listType != typeof(object))
            {
                listType = listType.BaseType;

                // this is a generic class
                if (listType.IsGenericType)
                {
                    // this is the generic base class NetworkReferenceList<>
                    if (listType.GetGenericTypeDefinition() == baseType)
                    {
                        // return the generic type
                        return listType.GetGenericArguments()[0];
                    }
                }
            }

            throw new Exception(
                "Couldn't get the type of the lists elements" +
                "Did you apply this drawer to something that doesn't inherit NetworkReferenceType<>?");

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
            var entry = drawList.serializedProperty.GetArrayElementAtIndex(index);

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
