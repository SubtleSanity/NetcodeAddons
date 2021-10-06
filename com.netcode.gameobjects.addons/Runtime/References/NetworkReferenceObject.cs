using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Netcode.Addons
{
    [Serializable]
    public struct NetworkReferenceObject : INetworkSerializable, INetworkReference<NetworkReferenceObject>
    {
        [SerializeField]
        private NetworkObject internalValue;
        
        public NetworkObject Value
        {
            get
            {
                return internalValue;
            }
        }

        public NetworkReferenceObject(NetworkObject networkObject)
        {
            internalValue = networkObject;
        }

        void INetworkSerializable.NetworkSerialize<T1>(BufferSerializer<T1> serializer)
        {
            var networkObjectId = default(ulong);

            if (serializer.IsWriter)
            {
                if (internalValue == null)
                {
                    networkObjectId = ulong.MaxValue;
                }
                else
                {
                    networkObjectId = internalValue.NetworkObjectId;
                }
            }

            serializer.SerializeValue(ref networkObjectId);

            if (serializer.IsReader)
            {
                if (networkObjectId == ulong.MaxValue)
                {
                    internalValue = null;
                }
                else
                {
                    // get the network object
                    NetworkManager.Singleton.SpawnManager.SpawnedObjects
                        .TryGetValue(networkObjectId, out internalValue);
                }
            }
        }
        void INetworkReference<NetworkReferenceObject>.NetworkRead(FastBufferReader reader)
        {
            reader.ReadValueSafe(out ulong networkObjectId);
            
            if (networkObjectId == ulong.MaxValue)
            {
                internalValue = null;
            }
            else
            {
                // get the network object
                NetworkManager.Singleton.SpawnManager.SpawnedObjects
                    .TryGetValue(networkObjectId, out internalValue);
            }
        }
        void INetworkReference<NetworkReferenceObject>.NetworkWrite(FastBufferWriter writer)
        {
            if (internalValue == null)
            {
                writer.WriteValueSafe(ulong.MaxValue);
            }
            else
            {
                writer.WriteValueSafe(internalValue.NetworkObjectId);
            }
        }
        bool IEquatable<NetworkReferenceObject>.Equals(NetworkReferenceObject other)
        {
            return internalValue == other.internalValue;
        }

        public static implicit operator NetworkObject(NetworkReferenceObject reference)
        {
            return reference.internalValue;
        }
        public static implicit operator NetworkReferenceObject(NetworkObject networkObject)
        {
            return new NetworkReferenceObject
            {
                internalValue = networkObject
            };
        }

        public bool Equals(NetworkReferenceObject other)
        {
            return internalValue == other.internalValue;
        }
        public override bool Equals(object obj)
        {
            return obj is NetworkReferenceObject other && internalValue == other.internalValue;
        }
        public override int GetHashCode()
        {
            return internalValue.GetHashCode();
        }
        public override string ToString()
        {
            return string.Format("NetworkReferenceObject: {0}", internalValue.ToString());
        }

    }
}
