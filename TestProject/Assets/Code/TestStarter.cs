using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;

namespace Assets.Code
{
    class TestStarter : MonoBehaviour
    {

        private void Start()
        {
            NetworkManager.Singleton.StartHost();
        }

    }
}
