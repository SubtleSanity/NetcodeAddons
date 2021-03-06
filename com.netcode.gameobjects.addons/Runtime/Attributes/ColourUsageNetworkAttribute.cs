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
    public sealed class ColourUsageNetworkAttribute : PropertyAttribute
    {
        public readonly bool showAlpha = true;
        public readonly bool hdr = false;

        public ColourUsageNetworkAttribute(bool showAlpha)
        {
            this.showAlpha = showAlpha;
        }
        public ColourUsageNetworkAttribute(bool showAlpha, bool hdr)
        {
            this.showAlpha = showAlpha;
            this.hdr = hdr;
        }
    }
}
