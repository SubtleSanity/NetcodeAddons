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

namespace Unity.Netcode.Addons
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class RangeNetworkVariableAttribute : PropertyAttribute
    {
        public readonly float min;
        public readonly float max;

        // Attribute used to make a float or int variable in a script be restricted to a specific range.
        public RangeNetworkVariableAttribute(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
    }
}
