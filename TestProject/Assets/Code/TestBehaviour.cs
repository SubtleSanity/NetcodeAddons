﻿using System;
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

        [ColourUsageNetworkVariable(true, true)]
        public NetworkVariable<Color> aa = new NetworkVariable<Color>();

    }
}
