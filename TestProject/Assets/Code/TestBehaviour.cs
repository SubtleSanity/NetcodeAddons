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

        public NetworkVariable<int> aa;
        public NetworkVariable<int> a;

        [RangeNetworkVariable(0f, 100f)]
        public NetworkVariable<bool> bbb = new NetworkVariable<bool>();

    }
}
