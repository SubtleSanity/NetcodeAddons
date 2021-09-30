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

        // reflected access to NetworkVariable<>
        private object GetNetworkVariable(SerializedProperty property)
        {
            return fieldInfo.GetValue(property.serializedObject.targetObject);
        }
        private object GetValueDirectly(object netVar)
        {
            foreach (var field in netVar.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                if (field.Name == "m_InternalValue")
                {
                    return field.GetValue(netVar);
                }
            throw new Exception("m_InternalValue is missing. Did NetworkVariable<> implementation change?");
        }
        private void SetValueDirectly(object netVar, object value)
        {
            foreach (var field in netVar.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                if (field.Name == "m_InternalValue")
                {
                    field.SetValue(netVar, value);
                    return;
                }
            throw new Exception("m_InternalValue is missing. Did NetworkVariable<> implementation change?");

        }
        private void SetValueByMethod(object netVar, object value)
        {
            // var method = typeof(NetworkVariableProperty).GetMethod("ApplyChange", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic);
            // method.Invoke(this, new[] { variable });

            foreach (var field in netVar.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                if (field.Name == "Value")
                {
                    field.SetValue(netVar, value);
                    return;
                }
        }
    }
}
