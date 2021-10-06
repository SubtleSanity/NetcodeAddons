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

    [CustomPropertyDrawer(typeof(MinNetworkAttribute), true)]
    public class MinNetworkDrawer : NetworkVariableDrawer
    {
        protected override void OnPropertyChanged(Rect position, SerializedProperty property, SerializedProperty internalProperty, GUIContent label)
        {
            var minAttribute = (MinNetworkAttribute)attribute;

            if (internalProperty.propertyType == SerializedPropertyType.Float)
            {
                var value = internalProperty.floatValue;
                internalProperty.floatValue = Mathf.Max(minAttribute.min, value);
            }
            else if (internalProperty.propertyType == SerializedPropertyType.Integer)
            {
                var value = internalProperty.intValue;
                internalProperty.intValue = Mathf.Max((int)minAttribute.min, value);
            }
            else if (internalProperty.propertyType == SerializedPropertyType.Vector2)
            {
                var value = internalProperty.vector2Value;
                internalProperty.vector2Value = new Vector2
                { 
                    x = Mathf.Max(minAttribute.min, value.x), 
                    y = Mathf.Max(minAttribute.min, value.y)
                };
            }
            else if (internalProperty.propertyType == SerializedPropertyType.Vector2Int)
            {
                var value = internalProperty.vector2IntValue;
                internalProperty.vector2IntValue = new Vector2Int
                {
                    x = Mathf.Max((int)minAttribute.min, value.x), 
                    y = Mathf.Max((int)minAttribute.min, value.y)
                };
            }
            else if (internalProperty.propertyType == SerializedPropertyType.Vector3)
            {
                var value = internalProperty.vector3Value;
                internalProperty.vector3Value = new Vector3
                {
                    x = Mathf.Max(minAttribute.min, value.x), 
                    y = Mathf.Max(minAttribute.min, value.y), 
                    z = Mathf.Max(minAttribute.min, value.z) 
                };
            }
            else if (internalProperty.propertyType == SerializedPropertyType.Vector3Int)
            {
                var value = internalProperty.vector3IntValue;
                internalProperty.vector3IntValue = new Vector3Int
                {
                    x = Mathf.Max((int)minAttribute.min, value.x),
                    y = Mathf.Max((int)minAttribute.min, value.y),
                    z = Mathf.Max((int)minAttribute.min, value.z)
                };
            }
            else if (internalProperty.propertyType == SerializedPropertyType.Vector4)
            {
                var value = internalProperty.vector4Value;
                internalProperty.vector4Value = new Vector4
                {
                    x = Mathf.Max(minAttribute.min, value.x),
                    y = Mathf.Max(minAttribute.min, value.y),
                    z = Mathf.Max(minAttribute.min, value.z),
                    w = Mathf.Max(minAttribute.min, value.w)
                };
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "[Min] requires float, int or Vector.");
            }
        }
    }
}
