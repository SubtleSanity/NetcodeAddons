using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityObject = UnityEngine.Object;
using UnityEngine;

namespace Unity.Netcode.Addons
{
    // custom networkvariable for holding my networked reference types.
    //  NetworkReferenceObject
    //  NetworkReferenceComponent7
    //  NetworkReferenceAsset
    //  NetworkString


    [Serializable]
    public abstract class NetworkReferenceVariable<T> : NetworkVariableBase where T : struct, INetworkReference<T>
    { 
        public delegate void OnValueChangedDelegate(T previousValue, T newValue);
        public OnValueChangedDelegate OnValueChanged;

        [SerializeField]
        protected T m_InternalValue;
        private bool isDirty;

        public T Value
        {
            get => m_InternalValue;
            set
            {
                if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
                {
                    throw new InvalidOperationException("Client can't write to NetworkVariables");
                }

                isDirty = true;
                var previousValue = m_InternalValue;
                m_InternalValue = value;
                OnValueChanged?.Invoke(previousValue, m_InternalValue);
            }
        }

        public NetworkReferenceVariable()
        {

        }
        public NetworkReferenceVariable(NetworkVariableReadPermission readPerm) : base(readPerm)
        {

        }
        public NetworkReferenceVariable(NetworkVariableReadPermission readPerm, T value) : base(readPerm)
        {
            m_InternalValue = value;
        }
        public NetworkReferenceVariable(T value)
        {
            m_InternalValue = value;
        }

        public override bool IsDirty()
        {
            return isDirty;
        }
        public override void SetDirty(bool isDirty)
        {
            this.isDirty = isDirty;
        }
        public override void ResetDirty()
        {
            isDirty = false;
        }

        public override void ReadDelta(FastBufferReader reader, bool keepDirtyDelta)
        {
            var previousValue = m_InternalValue;
            m_InternalValue.NetworkRead(reader);

            if (keepDirtyDelta)
            {
                isDirty = true;
            }

            OnValueChanged?.Invoke(previousValue, m_InternalValue);
        }
        public override void ReadField(FastBufferReader reader)
        {
            ReadDelta(reader, false);
        }

        public override void WriteDelta(FastBufferWriter writer)
        {
            WriteField(writer);
        }
        public override void WriteField(FastBufferWriter writer)
        {
            m_InternalValue.NetworkWrite(writer);
        }
    }
}
