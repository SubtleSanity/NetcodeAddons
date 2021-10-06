using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Unity.Netcode.Addons
{
    [CreateAssetMenu(fileName = "NewNetworkAssetManifest", menuName = "Netcode/Network Asset Manifest")]
    public class NetworkAssetManifest : ScriptableObject
    {        
        /// <summary>
        /// List of assets contained in this manifest
        /// </summary>
        [SerializeField] [HideInInspector]
        public NetworkAssetEntry[] assetEntries;
    }

    [Serializable]
    public struct NetworkAssetEntry
    {
        /// <summary>
        /// Id to identify assets across the network
        /// </summary>
        public uint globalId;
        /// <summary>
        /// reference to the local asset
        /// </summary>
        public UnityObject asset;
    }
}
