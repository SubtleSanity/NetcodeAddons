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
    [CustomPropertyDrawer(typeof(NetworkVariableString), true)]
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

            DrawInternalProperty(position, property, internalProperty, label);

            // detect and process any changes
            if (EditorGUI.EndChangeCheck())
            {
                OnPropertyChanged(position, property, internalProperty, label);

                if (Application.isPlaying)
                {
                    // apply the changes so we can read the new value
                    property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    // hold the new value the user just provided
                    var newValue = GetValueDirectly(variableObject);
                    // restore the old value so we can call Set() on the networkvariable
                    // otherwise it will think the new value doesn't represent a change
                    SetValueDirectly(variableObject, currentValue);
                    // call the proper function to set the new value
                    SetValueByMethod(variableObject, newValue);
                }
                else
                {
                    // apply the change
                    property.serializedObject.ApplyModifiedProperties();
                }
            }

            EditorGUI.EndDisabledGroup();
            EditorGUI.EndProperty();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var internalProperty = property.FindPropertyRelative("m_InternalValue");
            return GetInternalPropertyHeight(property, internalProperty, label);
        }

        protected virtual bool IsReadonly()
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
        protected virtual void DrawInternalProperty(Rect position, SerializedProperty property, SerializedProperty internalProperty, GUIContent label)
        {
            EditorGUI.PropertyField(position, internalProperty, label, true);
        }
        protected virtual void OnPropertyChanged(Rect position, SerializedProperty property, SerializedProperty internalProperty, GUIContent label)
        {

        }
        protected virtual float GetInternalPropertyHeight(SerializedProperty property, SerializedProperty internalProperty, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(internalProperty);
        }

        // reflected access to NetworkVariable<> and its internal value
        protected object GetNetworkVariable(SerializedProperty property)
        {
            return fieldInfo.GetValue(property.serializedObject.targetObject);
        }
        protected object GetValueDirectly(object networkVariable)
        {
            var field = networkVariable.GetType().GetField("m_InternalValue", BindingFlags.Instance | BindingFlags.NonPublic);

            if (field == null)
            {
                throw new Exception("NetworkVariable<>.m_InternalValue is missing. Did NetworkVariable<> implementation change?");
            }

            return field.GetValue(networkVariable);
        }
        protected void SetValueDirectly(object networkVariable, object value)
        {
            var field = networkVariable.GetType().GetField("m_InternalValue", BindingFlags.Instance | BindingFlags.NonPublic);

            if (field == null)
            {
                throw new Exception("NetworkVariable<>.m_InternalValue is missing. Did NetworkVariable<> implementation change?");
            }

            field.SetValue(networkVariable, value);
        }
        protected void SetValueByMethod(object networkVariable, object value)
        {
            var method = networkVariable.GetType().GetMethod("Set", BindingFlags.Instance | BindingFlags.NonPublic);

            if (method == null)
            {
                throw new Exception("NetworkVariable<>.Set is missing. Did NetworkVariable<> implementation change?");
            }

            method.Invoke(networkVariable, new object[] { value });
        }
    }
}
