using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Addons;

namespace Assets.Code
{
    class TestBehaviour : NetworkBehaviour
    {

        [RangeNetworkVariable(10, 100, slider:false)]
        public NetworkVariable<Vector3> aa = new NetworkVariable<Vector3>();

    }
}
