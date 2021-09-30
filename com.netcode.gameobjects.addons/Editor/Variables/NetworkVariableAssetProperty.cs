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
    [CustomPropertyDrawer(typeof(NetworkReferenceAsset<>), true)]
    public class NetworkVariableAssetProperty : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var assetProperty = property.FindPropertyRelative("assetGlobalId");
            var assetGlobalId = (uint)assetProperty.intValue;
            var contentPosition = EditorGUI.PrefixLabel(position, label);
            var genericType = GetGenericType();

            if (SafeToRead())
            {
                using var disabled = new EditorGUI.DisabledScope(IsReadonly());
                EditorGUI.BeginDisabledGroup(IsReadonly());

                NetworkAssetManager.Singleton.TryGetAsset(assetGlobalId, out var currentAsset);
                var newAsset = EditorGUI.ObjectField(contentPosition, GUIContent.none, currentAsset, genericType, true);

                if (currentAsset != newAsset)
                    if (newAsset != null)
                    {
                        assetGlobalId = default;
                        assetProperty.intValue = (int)assetGlobalId;
                    }
                    else
                    {
                        assetGlobalId = default;
                        assetProperty.intValue = (int)assetGlobalId;
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
            if (Application.isPlaying == false)
                return false;
            if (NetworkManager.Singleton == null)
                return false;
            if (NetworkAssetManager.Singleton == null)
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
    }
}
