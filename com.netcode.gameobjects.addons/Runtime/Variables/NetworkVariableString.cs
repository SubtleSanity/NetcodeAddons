using System.Collections.Generic;
using UnityEngine;
using System;

namespace Unity.Netcode.Addons
{
    /// <summary>
    /// A variable that can be synchronized over the network.
    /// </summary>
    [Serializable]
    public class NetworkVariableString : NetworkVariableBase
    {
        private bool isDirty;

        [SerializeField]
        private NetworkString m_InternalValue;

        public string Value
        {
            get => m_InternalValue;
            set
            {
                if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
                {
                    throw new InvalidOperationException("Client can't write to NetworkVariables");
                }
                Set(value);
            }
        }

        public delegate void OnValueChangedDelegate(string previousValue, string newValue);      
        public OnValueChangedDelegate OnValueChanged;

        public NetworkVariableString()
        {
        }
        public NetworkVariableString(NetworkVariableReadPermission readPerm) : base(readPerm)
        {
        }
        public NetworkVariableString(NetworkVariableReadPermission readPerm, string value) : base(readPerm)
        {
            m_InternalValue = value;
        }
        public NetworkVariableString(string value)
        {
            m_InternalValue = value;
        }

        private protected void Set(NetworkString value)
        {
            if (m_InternalValue.Value == value.Value)
            {
                return;
            }

            isDirty = true;
            NetworkString previousValue = m_InternalValue;
            m_InternalValue = value;
            OnValueChanged?.Invoke(previousValue, m_InternalValue);
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

        public override void WriteDelta(FastBufferWriter writer)
        {
            WriteField(writer);
        }
        public override void WriteField(FastBufferWriter writer)
        {
            NetworkString.Write(ref m_InternalValue, writer);
            //writer.WriteValueSafe(m_InternalValue);            
        }

        public override void ReadDelta(FastBufferReader reader, bool keepDirtyDelta)
        {
            NetworkString previousValue = m_InternalValue;
            NetworkString.Read(ref m_InternalValue, reader);

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

    }
}
