using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Netcode.Addons
{
    [Serializable]
    public struct NetworkReferenceObject
        : INetworkSerializable, IEquatable<NetworkReferenceObject>
    {
        [SerializeField]
        private ulong networkObjectId;

        public ulong NetworkObjectId
        {
            get
            {
                return networkObjectId;
            }
        }

        public NetworkReferenceObject(NetworkObject networkObject)
        {
            networkObjectId = networkObject.NetworkObjectId;
        }
        public NetworkReferenceObject(ulong networkObjectId)
        {
            this.networkObjectId = networkObjectId;
        }

        public NetworkObject Get()
        {
            NetworkManager.Singleton.SpawnManager.SpawnedObjects
                .TryGetValue(networkObjectId, out NetworkObject networkObject);

            return networkObject;
        }
        public bool TryGet(out NetworkObject networkObject)
        {
            return NetworkManager.Singleton.SpawnManager.SpawnedObjects
                .TryGetValue(networkObjectId, out networkObject);
        }

        public static implicit operator NetworkObject(NetworkReferenceObject reference)
        {
            return reference.Get();
        }
        public static implicit operator NetworkReferenceObject(NetworkObject networkObject)
        {
            return new NetworkReferenceObject
            {
                networkObjectId = networkObject.NetworkObjectId
            };
        }

        void INetworkSerializable.NetworkSerialize<T1>(BufferSerializer<T1> serializer)
        {
            serializer.SerializeValue(ref networkObjectId);
        }
        bool IEquatable<NetworkReferenceObject>.Equals(NetworkReferenceObject other)
        {
            return networkObjectId == other.networkObjectId;
        }

        public bool Equals(NetworkReferenceObject other)
        {
            return networkObjectId == other.networkObjectId;
        }
        public override bool Equals(object obj)
        {
            return obj is NetworkReferenceObject other && networkObjectId == other.networkObjectId;
        }
        public override int GetHashCode()
        {
            return networkObjectId.GetHashCode();
        }
        public override string ToString()
        {
            return string.Format("NetworkReferenceObject: {0}", networkObjectId);
        }
    }
}
