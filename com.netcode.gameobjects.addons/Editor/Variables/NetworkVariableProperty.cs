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

namespace Unity.Netcode.Addons.Editor
{
    [CustomPropertyDrawer(typeof(NetworkVariable<>), true)]
    public class NetworkVariableProperty : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            label.text += " (NetVar)";


            var control = EditorGUI.PrefixLabel(position, label);

            EditorGUI.BeginDisabledGroup(IsReadonly());

            var internalProperty = property.FindPropertyRelative("m_InternalValue");

            EditorGUI.PropertyField(control, internalProperty, GUIContent.none, true);

            EditorGUI.EndDisabledGroup();
            EditorGUI.EndProperty();
                   
        }
     
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var internalProperty = property.FindPropertyRelative("m_InternalValue");
            return EditorGUI.GetPropertyHeight(internalProperty);
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

    }
}
