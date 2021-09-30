using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityObject = UnityEngine.Object;
using UnityEngine;

namespace Unity.Netcode.Addons
{
    [Serializable]
    public struct NetworkReferenceAsset<T> 
        : INetworkSerializable, IEquatable<NetworkReferenceAsset<T>> 
        where T : UnityObject
    {
        [SerializeField]
        private uint assetGlobalId;

        public uint AssetGlobalId
        {
            get
            {
                return assetGlobalId;
            }
        }

        public NetworkReferenceAsset(T asset)
        {
            assetGlobalId = NetworkAssetManager.Singleton.GetGlobalId(asset);
        }
        public NetworkReferenceAsset(uint globalId)
        {
            this.assetGlobalId = globalId;
        }

        public T Get()
        {
            return NetworkAssetManager.Singleton.GetAsset<T>(assetGlobalId);
        }
        public bool TryGet(out T asset)
        {
            return NetworkAssetManager.Singleton.TryGetAsset<T>(assetGlobalId, out asset);
        }

        public static implicit operator T(NetworkReferenceAsset<T> reference)
        {
            return reference.Get();
        }
        public static implicit operator NetworkReferenceAsset<T>(T asset)
        {
            return new NetworkReferenceAsset<T>
            {
                assetGlobalId = NetworkAssetManager.Singleton.GetGlobalId(asset)
            };
        }

        void INetworkSerializable.NetworkSerialize<T1>(BufferSerializer<T1> serializer)
        {
            serializer.SerializeValue(ref assetGlobalId);
        }
        bool IEquatable<NetworkReferenceAsset<T>>.Equals(NetworkReferenceAsset<T> other)
        {
            return assetGlobalId == other.assetGlobalId;
        }

        public bool Equals(NetworkReferenceAsset<T> other)
        {
            return assetGlobalId == other.assetGlobalId;
        }
        public override bool Equals(object obj)
        {
            return obj is NetworkReferenceAsset<T> other && assetGlobalId == other.assetGlobalId;
        }
        public override int GetHashCode()
        {
            return assetGlobalId.GetHashCode();
        }
        public override string ToString()
        {
            return string.Format("NetworkReferenceAsset: {0}", assetGlobalId);
        }
    }

}
