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

    [CustomPropertyDrawer(typeof(MaxNetworkVariableAttribute), true)]
    public class MaxNetworkVariableProperty : NetworkVariableProperty
    {
        protected override void OnPropertyChanged(Rect position, SerializedProperty property, SerializedProperty internalProperty, GUIContent label)
        {
            Debug.Log("fsdfdsfdsfds");

            var maxAttribute = (MaxNetworkVariableAttribute)attribute;

            if (internalProperty.propertyType == SerializedPropertyType.Float)
            {
                var value = internalProperty.floatValue;
                internalProperty.floatValue = Mathf.Min(maxAttribute.max, value);
            }
            else if (internalProperty.propertyType == SerializedPropertyType.Integer)
            {
                var value = internalProperty.intValue;
                internalProperty.intValue = Mathf.Min((int)maxAttribute.max, value);
            }
            else if (internalProperty.propertyType == SerializedPropertyType.Vector2)
            {
                var value = internalProperty.vector2Value;
                internalProperty.vector2Value = new Vector2
                {
                    x = Mathf.Min(maxAttribute.max, value.x),
                    y = Mathf.Min(maxAttribute.max, value.y)
                };
            }
            else if (internalProperty.propertyType == SerializedPropertyType.Vector2Int)
            {
                var value = internalProperty.vector2IntValue;
                internalProperty.vector2IntValue = new Vector2Int
                {
                    x = Mathf.Min((int)maxAttribute.max, value.x),
                    y = Mathf.Min((int)maxAttribute.max, value.y)
                };
            }
            else if (internalProperty.propertyType == SerializedPropertyType.Vector3)
            {
                var value = internalProperty.vector3Value;
                internalProperty.vector3Value = new Vector3
                {
                    x = Mathf.Min(maxAttribute.max, value.x),
                    y = Mathf.Min(maxAttribute.max, value.y),
                    z = Mathf.Min(maxAttribute.max, value.z)
                };
            }
            else if (internalProperty.propertyType == SerializedPropertyType.Vector3Int)
            {
                var value = internalProperty.vector3IntValue;
                internalProperty.vector3IntValue = new Vector3Int
                {
                    x = Mathf.Min((int)maxAttribute.max, value.x),
                    y = Mathf.Min((int)maxAttribute.max, value.y),
                    z = Mathf.Min((int)maxAttribute.max, value.z)
                };
            }
            else if (internalProperty.propertyType == SerializedPropertyType.Vector4)
            {
                var value = internalProperty.vector4Value;
                internalProperty.vector4Value = new Vector4
                {
                    x = Mathf.Min(maxAttribute.max, value.x),
                    y = Mathf.Min(maxAttribute.max, value.y),
                    z = Mathf.Min(maxAttribute.max, value.z),
                    w = Mathf.Min(maxAttribute.max, value.w)
                };
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "[Max] requires float, int or Vector.");
            }
        }
    }
}
