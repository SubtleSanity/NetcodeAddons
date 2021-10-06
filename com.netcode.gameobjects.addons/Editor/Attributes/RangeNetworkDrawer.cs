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
  
    [CustomPropertyDrawer(typeof(RangeNetworkAttribute), true)]
    public class RangeNetworkDrawer : NetworkVariableDrawer
    {
        protected override void DrawInternalProperty(Rect position, SerializedProperty property, SerializedProperty internalProperty, GUIContent label)
        {
            var rangeAttribute = (RangeNetworkAttribute)attribute;

            if (rangeAttribute.slider)
            {
                if (internalProperty.propertyType == SerializedPropertyType.Integer)
                    EditorGUI.IntSlider(position, internalProperty, (int)rangeAttribute.min, (int)rangeAttribute.max, label);
                else if (internalProperty.propertyType == SerializedPropertyType.Float)
                    EditorGUI.Slider(position, internalProperty, rangeAttribute.min, rangeAttribute.max, label);
                else
                    EditorGUI.LabelField(position, label.text, "[Range] requires field of Integer or Float types.");
            }
            else
            {
                base.DrawInternalProperty(position, property, internalProperty, label);
            }
        }

        protected override void OnPropertyChanged(Rect position, SerializedProperty property, SerializedProperty internalProperty, GUIContent label)
        {
            var rangeAttribute = (RangeNetworkAttribute)attribute;
            if (internalProperty.propertyType == SerializedPropertyType.Float)
            {
                var value = internalProperty.floatValue;
                internalProperty.floatValue = Mathf.Clamp(value, rangeAttribute.min, rangeAttribute.max);
            }
            else if (internalProperty.propertyType == SerializedPropertyType.Integer)
            {
                var value = internalProperty.intValue;
                internalProperty.intValue = Mathf.Clamp(value, (int)rangeAttribute.min, (int)rangeAttribute.max);
            }
            else if (internalProperty.propertyType == SerializedPropertyType.Vector2)
            {
                var value = internalProperty.vector2Value;
                internalProperty.vector2Value = new Vector2
                {
                    x = Mathf.Clamp(value.x, rangeAttribute.min, rangeAttribute.max),
                    y = Mathf.Clamp(value.y, rangeAttribute.min, rangeAttribute.max)
                };
            }
            else if (internalProperty.propertyType == SerializedPropertyType.Vector2Int)
            {
                var value = internalProperty.vector2IntValue;
                internalProperty.vector2IntValue = new Vector2Int
                {
                    x = Mathf.Clamp(value.x, (int)rangeAttribute.min, (int)rangeAttribute.max),
                    y = Mathf.Clamp(value.y, (int)rangeAttribute.min, (int)rangeAttribute.max)
                };
            }
            else if (internalProperty.propertyType == SerializedPropertyType.Vector3)
            {
                var value = internalProperty.vector3Value;
                internalProperty.vector3Value = new Vector3
                {
                    x = Mathf.Clamp(value.x, rangeAttribute.min, rangeAttribute.max),
                    y = Mathf.Clamp(value.y, rangeAttribute.min, rangeAttribute.max),
                    z = Mathf.Clamp(value.z, rangeAttribute.min, rangeAttribute.max)
                };
            }
            else if (internalProperty.propertyType == SerializedPropertyType.Vector3Int)
            {
                var value = internalProperty.vector3IntValue;
                internalProperty.vector3IntValue = new Vector3Int
                {
                    x = Mathf.Clamp(value.x, (int)rangeAttribute.min, (int)rangeAttribute.max),
                    y = Mathf.Clamp(value.y, (int)rangeAttribute.min, (int)rangeAttribute.max),
                    z = Mathf.Clamp(value.z, (int)rangeAttribute.min, (int)rangeAttribute.max)
                };
            }
            else if (internalProperty.propertyType == SerializedPropertyType.Vector4)
            {
                var value = internalProperty.vector4Value;
                internalProperty.vector4Value = new Vector4
                {
                    x = Mathf.Clamp(value.x, rangeAttribute.min, rangeAttribute.max),
                    y = Mathf.Clamp(value.y, rangeAttribute.min, rangeAttribute.max),
                    z = Mathf.Clamp(value.z, rangeAttribute.min, rangeAttribute.max),
                    w = Mathf.Clamp(value.w, rangeAttribute.min, rangeAttribute.max)
                };
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "[Range] requires float, int or Vector.");
            }
        }
    }
}
