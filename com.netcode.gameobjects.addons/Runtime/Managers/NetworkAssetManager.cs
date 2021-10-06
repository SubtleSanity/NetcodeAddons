using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Unity.Netcode.Addons
{
    [RequireComponent(typeof(NetworkManager))]
    public class NetworkAssetManager : MonoBehaviour
    {
        [SerializeField] [HideInInspector]
        public NetworkAssetManifest[] initialManifests;
        private Dictionary<uint, UnityObject> globalToAsset;
        private Dictionary<UnityObject, uint> assetToGlobal;

        private void Awake()
        {
            globalToAsset = new Dictionary<uint, UnityObject>();
            assetToGlobal = new Dictionary<UnityObject, uint>();

            foreach (var manifest in initialManifests)
            {
                AddAssetsFromManifest(manifest);
            }

            Singleton = this;
        }

        public static NetworkAssetManager Singleton
        {
            get;
            private set;
        }

        public bool ContainsGlobalId(uint globalId)
        {
            return globalToAsset.ContainsKey(globalId);
        }
        public bool ContainsAsset(UnityObject asset)
        {
            return assetToGlobal.ContainsKey(asset);
        }

        public uint GetGlobalId(UnityObject asset)
        {
            assetToGlobal.TryGetValue(asset, out var result);
            return result;
        }

        public UnityObject GetAsset(uint globalId)
        {
            globalToAsset.TryGetValue(globalId, out var result);
            return result;
        }
        public T GetAsset<T>(uint globalId) where T : UnityObject
        {
            globalToAsset.TryGetValue(globalId, out var result);
            return result as T;
        }        
        public bool TryGetAsset(uint globalId, out UnityObject asset)
        {
            return globalToAsset.TryGetValue(globalId, out asset);
        }       
        public bool TryGetAsset<T>(uint globalId, out T asset) where T : UnityObject
        {
            var success = globalToAsset.TryGetValue(globalId, out var result);
            asset = result as T;
            return success;
        }


        private void AddAssetsFromManifest(NetworkAssetManifest manifest)
        {
            // add each entry in the manifest
            foreach (var assetEntry in manifest.assetEntries)
            {
                // check if that globalId is already in use
                if (globalToAsset.TryGetValue(assetEntry.globalId, out var conflict))
                {
                    // log an error
                    Debug.LogError(string.Format(
                        "Failed to add asset from manifest named '{0}'.\n" +
                        "Tried to add asset named '{1}' with globalId {2}.\n" +
                        "Already added an asset with that globalId named '{3}'.",
                        manifest.name, assetEntry.asset.name, assetEntry.globalId, conflict.name));
                }
                else
                {
                    // add the asset
                    globalToAsset.Add(assetEntry.globalId, assetEntry.asset);
                    assetToGlobal.Add(assetEntry.asset, assetEntry.globalId);
                }
            }
        }
        private void RemoveAssetsFromManifest(NetworkAssetManifest manifest)
        {
            // remove each entry in the manifest
            foreach (var assetEntry in manifest.assetEntries)
            {
                globalToAsset.Remove(assetEntry.globalId);
                assetToGlobal.Remove(assetEntry.asset);
            }
        }
    }
}
