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
    [CustomPropertyDrawer(typeof(NetworkString), true)]
    public class NetworkStringProperty : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var valueProperty = property.FindPropertyRelative("value");
            var currentValue = valueProperty.stringValue;
            var contentPosition = EditorGUI.PrefixLabel(position, label);

            EditorGUI.BeginChangeCheck();

            var newValue = EditorGUI.DelayedTextField(contentPosition, GUIContent.none, currentValue);

            if (EditorGUI.EndChangeCheck())
            {
                valueProperty.stringValue = newValue;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

    }
}
