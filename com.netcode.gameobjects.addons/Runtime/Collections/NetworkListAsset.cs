using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityObject = UnityEngine.Object;

namespace Unity.Netcode.Addons
{
    public class NetworkListAsset<T> : NetworkList<NetworkReferenceAsset<T>>
        where T : UnityObject
    {
        



    }
}
