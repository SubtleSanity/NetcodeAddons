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
        public NetworkVariableObject aaa = new NetworkVariableObject();
        public NetworkVariableComponent<TestBehaviour> bbb = new NetworkVariableComponent<TestBehaviour>();
        public NetworkVariable<int> ccc = new NetworkVariable<int>();
        public NetworkVariableString ddd = new NetworkVariableString();
    }
}
