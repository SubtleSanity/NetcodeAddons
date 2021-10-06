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
    [CustomPropertyDrawer(typeof(NetworkReferenceComponent<>), true)]
    [CustomPropertyDrawer(typeof(NetworkReferenceObject), true)]
    [CustomPropertyDrawer(typeof(NetworkString), true)]
    public class NetworkReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var internalProperty = property.FindPropertyRelative("internalValue");
           
            EditorGUI.PropertyField(position, internalProperty, label, true);

            EditorGUI.EndProperty();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var internalProperty = property.FindPropertyRelative("internalValue");
            return EditorGUI.GetPropertyHeight(internalProperty);
        }
    }
}
