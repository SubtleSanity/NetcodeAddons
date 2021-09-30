using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;

namespace Assets.Code
{
    class TestBehaviour : NetworkBehaviour
    {
        public int sdfdsfsdfds;

        [SerializeField]
        public NetworkVariable<int> testVar;

        private void Awake()
        {

        }
    }
}
