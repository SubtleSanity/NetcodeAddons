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
  
    [CustomPropertyDrawer(typeof(RangeNetworkVariableAttribute), true)]
    public class RangeNetworkVariableProperty : NetworkVariableProperty
    {
        protected override void DrawProperty(Rect position, SerializedProperty property, SerializedProperty internalProperty, GUIContent label)
        {
            var range = (RangeNetworkVariableAttribute)attribute;

            if (internalProperty.propertyType == SerializedPropertyType.Integer)
                EditorGUI.IntSlider(position, internalProperty, (int)range.min, (int)range.max, label);
            else if (internalProperty.propertyType == SerializedPropertyType.Float)
                EditorGUI.Slider(position, internalProperty, range.min, range.max, label);
            else
                EditorGUI.LabelField(position, label.text, "[Range] requires field of Integer or Float types.");

        }
    }
}
