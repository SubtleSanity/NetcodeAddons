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
        public NetworkVariableString aaaa = new NetworkVariableString();

        public NetworkString testString = "sdfdfds";

        private void Update()
        {
            Debug.Log(testString);
        }

        //private string blah = "0987654321";
        //public int sdfdsfd;
        //public bool DoTheThing;

        //[ColourUsageNetworkVariable(true, true)]
        //public NetworkVariable<Color> aa = new NetworkVariable<Color>();

        //private void Awake()
        //{
        //    var temp = blah;
        //    blah = string.Empty;
        //    for (int i = 0; i < 1000; i += 1)
        //        blah += temp;
        //}
        //private void Update()
        //{
        //    //if (DoTheThing)
        //    {
        //        DoTheThing = false;
        //        TestClientRPC(blah);
        //    }
        //}

        //[ClientRpc]
        //public void TestClientRPC(NetworkString val)
        //{
        //    Debug.Log(val.Value);
        //    sdfdsfd = val.Value.Length;
        //}
    }
}
