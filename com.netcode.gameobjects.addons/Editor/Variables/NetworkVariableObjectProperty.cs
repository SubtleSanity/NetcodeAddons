using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Netcode;
using UnityObject = UnityEngine.Object;

namespace Unity.Netcode.Addons.Editor
{
    [CustomPropertyDrawer(typeof(NetworkReferenceObject), true)]
    public class NetworkVariableObjectProperty : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var objectProperty = property.FindPropertyRelative("networkObjectId");
            var objectGlobalId = (ulong)objectProperty.longValue;
            var contentPosition = EditorGUI.PrefixLabel(position, label);

            if (SafeToRead())
            {
                EditorGUI.BeginDisabledGroup(IsReadonly());

                var currentObject = GetObject(objectGlobalId);
                var newObject = EditorGUI.ObjectField(contentPosition, GUIContent.none, currentObject, typeof(NetworkObject), true) as NetworkObject;

                if (currentObject != newObject)
                    if (newObject != null)
                    {
                        objectGlobalId = newObject.NetworkObjectId;
                        objectProperty.longValue = (long)objectGlobalId;
                    }
                    else
                    {
                        objectGlobalId = default;
                        objectProperty.longValue = (long)objectGlobalId;
                    }

                EditorGUI.EndDisabledGroup();
                EditorGUI.EndProperty();
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);

                EditorGUI.ObjectField(contentPosition, GUIContent.none, null, typeof(NetworkObject), true);

                EditorGUI.EndDisabledGroup();
                EditorGUI.EndProperty();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        private bool SafeToRead()
        {
            // we can't read when the game isn't running
            // we can't read when the networkmanager isn't present
            // we can't read when network isn't started

            if (Application.isPlaying == false)
                return false;
            if (NetworkManager.Singleton == null)
                return false;
            if (NetworkManager.Singleton.IsClient == false &&
                NetworkManager.Singleton.IsServer == false &&
                NetworkManager.Singleton.IsHost == false)
                return false;
            return true;
        }
        private bool IsReadonly()
        {
            if (Application.isPlaying == false)
                return true;
            if (NetworkManager.Singleton == null)
                return true;
            if (NetworkManager.Singleton.IsHost)
                return false;
            if (NetworkManager.Singleton.IsServer)
                return false;
            return true;
        }
        private NetworkObject GetObject(ulong networkObjectId)
        {
            NetworkManager.Singleton.SpawnManager.SpawnedObjects
                .TryGetValue(networkObjectId, out NetworkObject networkObject);

            return networkObject;
        }

    }
}
