using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Unity.Netcode.Addons
{
    [Serializable]
    public struct NetworkReferenceAsset<T> : INetworkSerializable, INetworkReference<NetworkReferenceAsset<T>>
        where T : UnityObject
    {
        [SerializeField]
        private T internalValue;

        public T Value
        {
            get
            {
                return internalValue;
            }
        }

        public NetworkReferenceAsset(T asset)
        {
            internalValue = asset;
        }

        void INetworkSerializable.NetworkSerialize<T1>(BufferSerializer<T1> serializer)
        {
            var assetGlobalId = default(uint);

            if (serializer.IsWriter)
            {
                if (internalValue == null)
                {
                    assetGlobalId = uint.MaxValue;
                }
                else
                {
                    assetGlobalId = NetworkAssetManager.Singleton.GetGlobalId(internalValue);
                }
            }

            serializer.SerializeValue(ref assetGlobalId);

            if (serializer.IsReader)
            {
                if (assetGlobalId == uint.MaxValue)
                {
                    internalValue = null;
                }
                else
                {
                    internalValue = NetworkAssetManager.Singleton.GetAsset(assetGlobalId) as T;
                }
            }
        }
        void INetworkReference<NetworkReferenceAsset<T>>.NetworkRead(FastBufferReader reader)
        {
            reader.ReadValueSafe(out uint assetGlobalId);

            if (assetGlobalId == uint.MaxValue)
            {
                internalValue = null;
            }
            else
            { 
                internalValue = NetworkAssetManager.Singleton.GetAsset(assetGlobalId) as T;
            }
        }
        void INetworkReference<NetworkReferenceAsset<T>>.NetworkWrite(FastBufferWriter writer)
        {
            var assetGlobalId = NetworkAssetManager.Singleton.GetGlobalId(internalValue);
 
            if (internalValue == null)
            {
                writer.WriteValueSafe(uint.MaxValue);
            }
            else
            {
                writer.WriteValueSafe(assetGlobalId);
            }
        }
        bool IEquatable<NetworkReferenceAsset<T>>.Equals(NetworkReferenceAsset<T> other)
        {
            return internalValue == other.internalValue;
        }

        public static implicit operator T(NetworkReferenceAsset<T> reference)
        {
            return reference.internalValue;
        }
        public static implicit operator NetworkReferenceAsset<T>(T asset)
        {
            return new NetworkReferenceAsset<T>
            {
                internalValue = asset
            };
        }

        public bool Equals(NetworkReferenceAsset<T> other)
        {
            return internalValue == other.internalValue;
        }
        public override bool Equals(object obj)
        {
            return obj is NetworkReferenceAsset<T> other && internalValue == other.internalValue;
        }
        public override int GetHashCode()
        {
            return internalValue.GetHashCode();
        }
        public override string ToString()
        {
            return string.Format("NetworkReferenceComponent: {0}", internalValue.ToString());
        }

    }
}
