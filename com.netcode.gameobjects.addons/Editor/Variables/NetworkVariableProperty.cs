using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
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

            EditorGUI.BeginDisabledGroup(IsReadonly());

            // get the information we need from the property
            var internalProperty = property.FindPropertyRelative("m_InternalValue");
            var variableObject = GetNetworkVariable(property);
            var currentValue = GetValueDirectly(variableObject);            

            // show the property field
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, internalProperty, label, true);
            
            // detect and process any changes
            if (EditorGUI.EndChangeCheck())
            {
                // apply the changes so we can read the new value
                // can't support undo because it won't notify the NetVar of the change
                property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                // hold the new value the user just provided
                var newValue = GetValueDirectly(variableObject);
                // restore the old value so we can call Set() on the networkvariable
                // otherwise it will think the new value doesn't represent a change
                SetValueDirectly(variableObject, currentValue);
                // call the proper function to set the new value
                SetValueByMethod(variableObject, newValue);
            }

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
            // can write if app is not running
            if (Application.isPlaying == false)
                return false;
            // can write if networkmanager doesn't exist
            if (NetworkManager.Singleton == null)
                return false;
            // can write if we're the server
            if (NetworkManager.Singleton.IsServer)
                return false;
            return true;
        }

        // reflected access to NetworkVariable<>
        private object GetNetworkVariable(SerializedProperty property)
        {
            return fieldInfo.GetValue(property.serializedObject.targetObject);
        }
        private object GetValueDirectly(object netVar)
        {
            var field = netVar.GetType().GetField("m_InternalValue", BindingFlags.Instance | BindingFlags.NonPublic);
            
            if (field == null)
            {
                throw new Exception("NetworkVariable<>.m_InternalValue is missing. Did NetworkVariable<> implementation change?");
            }

            return field.GetValue(netVar);
        }
        private void SetValueDirectly(object netVar, object value)
        {
            var field = netVar.GetType().GetField("m_InternalValue", BindingFlags.Instance | BindingFlags.NonPublic);

            if (field == null)
            {
                throw new Exception("NetworkVariable<>.m_InternalValue is missing. Did NetworkVariable<> implementation change?");
            }

            field.SetValue(netVar, value);
        }
        private void SetValueByMethod(object netVar, object value)
        {
            var field = netVar.GetType().GetProperty("Value", BindingFlags.Instance | BindingFlags.Public);

            if (field == null)
            {
                throw new Exception("NetworkVariable<>.Value is missing. Did NetworkVariable<> implementation change?");
            }

            field.SetValue(netVar, value);
        }
    }
}
