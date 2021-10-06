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

    [CustomPropertyDrawer(typeof(ColourUsageNetworkAttribute), true)]
    public class ColourUsageNetworkProperty : NetworkVariableDrawer
    {

        protected override void DrawInternalProperty(Rect position, SerializedProperty property, SerializedProperty internalProperty, GUIContent label)
        {
            var colorUsageAttribute = (ColourUsageNetworkAttribute)attribute;

            if (internalProperty.propertyType == SerializedPropertyType.Color)
            {
                EditorGUI.BeginChangeCheck();
                var newColor = EditorGUI.ColorField(position, label, internalProperty.colorValue, true, colorUsageAttribute.showAlpha, colorUsageAttribute.hdr);
                internalProperty.colorValue = newColor;
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "[ColourUsage] requires Color type.");
            }
        }

    }
}
