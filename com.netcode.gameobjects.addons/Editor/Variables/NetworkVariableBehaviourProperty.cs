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
    [CustomPropertyDrawer(typeof(NetworkReferenceBehaviour<>), true)]
    public class NetworkVariableBehaviourProperty : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var objectProperty = property.FindPropertyRelative("networkObjectId");
            var objectGlobalId = (ulong)objectProperty.longValue;
            var behaviourProperty = property.FindPropertyRelative("networkBehaviourId");
            var behaviourOrderId = (ushort)objectProperty.intValue;
            var contentPosition = EditorGUI.PrefixLabel(position, label);
            var genericType = GetGenericType();

            if (SafeToRead())
            {
                EditorGUI.BeginDisabledGroup(IsReadonly());

                var currentObject = GetObject(objectGlobalId);
                var currentBehaviour = currentObject.GetNetworkBehaviourAtOrderIndex(behaviourOrderId);
                var newBehaviour = EditorGUI.ObjectField(contentPosition, GUIContent.none, currentBehaviour, genericType, true) as NetworkBehaviour;

                if (currentObject != newBehaviour)
                {
                    objectGlobalId = newBehaviour.NetworkObjectId;
                    objectProperty.longValue = (long)objectGlobalId;
                    behaviourOrderId = newBehaviour.NetworkBehaviourId;
                    behaviourProperty.intValue = (ushort)behaviourOrderId;
                }

                EditorGUI.EndDisabledGroup();
                EditorGUI.EndProperty();
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);

                EditorGUI.ObjectField(contentPosition, GUIContent.none, null, genericType, true);

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
            if (NetworkAssetManager.Singleton == null)
                return true;
            if (NetworkManager.Singleton.IsHost)
                return false;
            if (NetworkManager.Singleton.IsServer)
                return false;
            return true;
        }
        private Type GetGenericType()
        {
            return fieldInfo.FieldType.GetGenericArguments()[0];
        }
        private NetworkObject GetObject(ulong networkObjectId)
        {
            NetworkManager.Singleton.SpawnManager.SpawnedObjects
                .TryGetValue(networkObjectId, out NetworkObject networkObject);

            return networkObject;
        }

    }
}
