using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityObject = UnityEngine.Object;

namespace Unity.Netcode.Addons
{
    [Serializable]
    public class NetworkListAsset<T> : NetworkReferenceList<NetworkReferenceAsset<T>>
        where T : UnityObject
    {


    }
}
