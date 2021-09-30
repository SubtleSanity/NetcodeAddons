using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Netcode.Addons
{
    [Serializable]
    public struct NetworkReferenceBehaviour<T>
        : INetworkSerializable, IEquatable<NetworkReferenceBehaviour<T>>
        where T : NetworkBehaviour
    {
        [SerializeField]
        private ulong networkObjectId;
        [SerializeField]
        private ushort networkBehaviourId;

        public ulong NetworkObjectId
        {
            get
            {
                return networkObjectId;
            }
        }
        public ushort NetworkBehaviourId
        {
            get
            {
                return networkBehaviourId;
            }
        }

        public NetworkReferenceBehaviour(T networkBehaviour)
        {
            networkObjectId = networkBehaviour.NetworkObjectId;
            networkBehaviourId = networkBehaviour.NetworkBehaviourId;
        }
        public NetworkReferenceBehaviour(ulong networkObjectId, ushort networkBehaviourId)
        {
            this.networkObjectId = networkObjectId;
            this.networkBehaviourId = networkBehaviourId;
        }

        public T Get()
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject))
            {
                return networkObject.GetNetworkBehaviourAtOrderIndex(NetworkBehaviourId) as T;
            }
            return null;
        }
        public bool TryGet(out T networkBehaviour)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject))
            {
                networkBehaviour = networkObject.GetNetworkBehaviourAtOrderIndex(NetworkBehaviourId) as T;
                return true;
            }

            networkBehaviour = null;
            return false;
        }

        public static implicit operator T(NetworkReferenceBehaviour<T> reference)
        {
            return reference.Get();
        }
        public static implicit operator NetworkReferenceBehaviour<T>(T networkBehaviour)
        {
            return new NetworkReferenceBehaviour<T>
            {
                networkObjectId = networkBehaviour.NetworkObjectId,
                networkBehaviourId = networkBehaviour.NetworkBehaviourId
            };
        }

        void INetworkSerializable.NetworkSerialize<T1>(BufferSerializer<T1> serializer)
        {
            serializer.SerializeValue(ref networkObjectId);
            serializer.SerializeValue(ref networkBehaviourId);
        }
        bool IEquatable<NetworkReferenceBehaviour<T>>.Equals(NetworkReferenceBehaviour<T> other)
        {
            return networkObjectId == other.networkObjectId &&
                networkBehaviourId == other.networkBehaviourId;
        }

        public bool Equals(NetworkReferenceBehaviour<T> other)
        {
            return networkObjectId == other.networkObjectId 
                && networkBehaviourId == other.networkBehaviourId;
        }
        public override bool Equals(object obj)
        {
            return obj is NetworkReferenceBehaviour<T> other 
                && networkObjectId == other.networkObjectId 
                && networkBehaviourId == other.networkBehaviourId;
        }
        public override int GetHashCode()
        {
            int hashCode = 949727528;
            hashCode = hashCode * -1521134295 + networkObjectId.GetHashCode();
            hashCode = hashCode * -1521134295 + networkBehaviourId.GetHashCode();
            return hashCode;
        }
        public override string ToString()
        {
            return string.Format("NetworkReferenceBehaviour: {0}, {1}", networkObjectId, networkBehaviourId);
        }
    }
}
