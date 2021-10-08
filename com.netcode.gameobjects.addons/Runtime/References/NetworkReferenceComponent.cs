using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Netcode.Addons
{
    [Serializable]
    public struct NetworkReferenceComponent<T> : INetworkSerializable, INetworkReference<NetworkReferenceComponent<T>>
        where T : NetworkBehaviour
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

        public NetworkReferenceComponent(T component)
        {
            internalValue = component;
        }

        void INetworkSerializable.NetworkSerialize<T1>(BufferSerializer<T1> serializer)
        {
            var networkObjectId = default(ulong);
            var componentIndex = default(ushort);

            serializer.PreCheck(sizeof(ulong) + sizeof(ushort));

            if (serializer.IsWriter)
            {
                if (internalValue == null)
                {
                    networkObjectId = ulong.MaxValue;
                    componentIndex = ushort.MaxValue;
                }
                else
                {
                    networkObjectId = internalValue.NetworkObjectId;
                    componentIndex = internalValue.NetworkBehaviourId;
                }
            }

            serializer.SerializeValuePreChecked(ref networkObjectId);
            serializer.SerializeValuePreChecked(ref componentIndex);

            if (serializer.IsReader)
            {
                if (networkObjectId == ulong.MaxValue && componentIndex == ushort.MaxValue)
                {
                    internalValue = null;
                }
                else
                {
                    // get the network object
                    NetworkManager.Singleton.SpawnManager.SpawnedObjects
                        .TryGetValue(networkObjectId, out var networkObject);
                    // get the component
                    internalValue = networkObject.GetNetworkBehaviourAtOrderIndex(componentIndex) as T;
                }
            }
        }
        void INetworkReference<NetworkReferenceComponent<T>>.NetworkRead(FastBufferReader reader)
        {
            reader.TryBeginRead(sizeof(ulong) + sizeof(ushort));
            reader.ReadValue(out ulong networkObjectId);
            reader.ReadValue(out ushort componentIndex);

            if (networkObjectId == ulong.MaxValue && componentIndex == ushort.MaxValue)
            {
                internalValue = null;
            }
            else
            {
                // get the network object
                NetworkManager.Singleton.SpawnManager.SpawnedObjects
                    .TryGetValue(networkObjectId, out var networkObject);
                // get the component
                internalValue = networkObject.GetNetworkBehaviourAtOrderIndex(componentIndex) as T;
            }
        }
        void INetworkReference<NetworkReferenceComponent<T>>.NetworkWrite(FastBufferWriter writer)
        {
            writer.TryBeginWrite(sizeof(ulong) + sizeof(ushort));

            if (internalValue == null)
            {
                writer.WriteValue(ulong.MaxValue);
                writer.WriteValue(ushort.MaxValue);
            }
            else
            {
                writer.WriteValue(internalValue.NetworkObjectId);
                writer.WriteValue(internalValue.NetworkBehaviourId);
            }
        }

        public static implicit operator T(NetworkReferenceComponent<T> reference)
        {
            return reference.internalValue;
        }
        public static implicit operator NetworkReferenceComponent<T>(T component)
        {
            return new NetworkReferenceComponent<T>
            {
                internalValue = component
            };
        }

        public bool Equals(NetworkReferenceComponent<T> other)
        {
            return internalValue == other.internalValue;
        }
        public override bool Equals(object obj)
        {
            return obj is NetworkReferenceComponent<T> other && internalValue == other.internalValue;
        }
        public override int GetHashCode()
        {
            return internalValue.GetHashCode();
        }
        public override string ToString()
        {
            return string.Format("NetworkReferenceComponent: {0}", internalValue?.ToString() ?? "NULL");
        }

    }
}
