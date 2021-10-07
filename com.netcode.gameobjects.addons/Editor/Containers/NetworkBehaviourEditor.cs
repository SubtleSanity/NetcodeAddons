using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityObject = UnityEngine.Object;

namespace Unity.Netcode.Addons.Editor
{
    [CustomEditor(typeof(NetworkBehaviour), true)]
    public class NetworkBehaviourEditor : UnityEditor.Editor
    {

    }
}
