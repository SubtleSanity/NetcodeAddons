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
    [CustomPropertyDrawer(typeof(NetworkReferenceVariable<>), true)]
    public class NetworkVariableDrawer : PropertyDrawer
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

                if (IsNetworkRunning())
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
        protected virtual bool IsNetworkRunning()
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
                throw new Exception(string.Format("{0}.m_internalValue is missing. Did {0} implementation change?", networkVariable.GetType().Name));
            }

            return field.GetValue(networkVariable);
        }
        protected void SetValueDirectly(object networkVariable, object value)
        {
            var field = networkVariable.GetType().GetField("m_InternalValue", BindingFlags.Instance | BindingFlags.NonPublic);

            if (field == null)
            {
                throw new Exception(string.Format("{0}.m_internalValue is missing. Did {0} implementation change?", networkVariable.GetType().Name));
            }

            field.SetValue(networkVariable, value);
        }
        protected void SetValueByMethod(object networkVariable, object value)
        {
            var property = networkVariable.GetType().GetProperty("Value", BindingFlags.Instance | BindingFlags.Public);

            if (property == null)
            {
                throw new Exception(string.Format("{0}.Value is missing. Did {0} implementation change?", networkVariable.GetType().Name));
            }
            property.SetValue(networkVariable, value);
        }

    }
}
