using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Netcode.Addons
{
    [Serializable]
    public struct NetworkString : INetworkSerializable, INetworkReference<NetworkString>
    {
        internal const int DEFAULT_BUFFER_SIZE = 4096;
        internal static byte[] buffer = new byte[DEFAULT_BUFFER_SIZE];
        public static int BUFFER_SIZE
        {
            get
            {
                return buffer.Length;
            }
        }
        public static void SetBufferSize(int bufferSize)
        {
            buffer = new byte[bufferSize];
        }

        [SerializeField]
        private string internalValue;
        
        public string Value
        {
            get
            {
                return internalValue;
            }
        }

        public NetworkString(string value)
        {
            this.internalValue = value;
        }

        void INetworkSerializable.NetworkSerialize<T1>(BufferSerializer<T1> serializer)
        {
            var bytesWritten = default(int);
            
            if (serializer.IsWriter)
            {
                bytesWritten = PushToBuffer();
                serializer.PreCheck(sizeof(int) + bytesWritten);
            }

            serializer.SerializeValue(ref bytesWritten);

            for (int i = 0; i < bytesWritten; i += 1)
                serializer.SerializeValue(ref buffer[i]);

            if (serializer.IsReader)
            {
                internalValue = Encoding.UTF8.GetString(buffer, 0, bytesWritten);
            }
        }
        void INetworkReference<NetworkString>.NetworkRead(FastBufferReader reader)
        {
            reader.ReadValueSafe(out int bytesWritten);

            if (reader.TryBeginRead(bytesWritten))
            {
                reader.ReadBytes(ref buffer, bytesWritten, 0);
                internalValue = Encoding.UTF8.GetString(buffer, 0, bytesWritten);
            }
            else
            {
                Debug.LogError("Failed to read");
            }
        }
        void INetworkReference<NetworkString>.NetworkWrite(FastBufferWriter writer)
        {
            int bytesWritten = PushToBuffer();

            if (writer.TryBeginWrite(bytesWritten + sizeof(int)))
            {
                writer.WriteValue(bytesWritten);
                writer.WriteBytes(buffer, bytesWritten, 0);
            }
            else
            {
                Debug.LogError("Failed to write");
            }
        }
 
        unsafe int PushToBuffer()
        {
            if (internalValue == null)
                return 0;

            var bytesWritten = default(int);
            var encoder = Encoding.UTF7.GetEncoder();
            var charStep = 50;
            var byteStep = Encoding.UTF7.GetMaxByteCount(charStep);

            var remainingChars = Value.Length;
            var remainingBytes = buffer.Length;

            fixed (byte* bytes = buffer)
            fixed (char* chars = internalValue)
            {
                while (remainingChars > 0 && remainingBytes > byteStep)
                {
                    var result = encoder.GetBytes(
                        chars + (internalValue.Length - remainingChars), // start of char array
                        Math.Min(charStep, remainingChars),         // num of chars to write
                        bytes + (buffer.Length - remainingBytes),   // start of byte array          
                        remainingBytes,                             // max bytes to write
                        true
                        );

                    bytesWritten += result;
                    remainingBytes -= result;
                    remainingChars -= charStep;
                }
            }

            return bytesWritten;
        }

        public static implicit operator string(NetworkString networkString)
        {
            return networkString.internalValue;
        }
        public static implicit operator NetworkString(string value)
        {
            return new NetworkString
            (
                value
            );
        }

        public bool Equals(NetworkString other)
        {
            return internalValue == other.internalValue;
        }
        public override bool Equals(object obj)
        {
            return obj is NetworkString other && internalValue == other.internalValue;
        }
        public override int GetHashCode()
        {
            return internalValue.GetHashCode();
        }
        public override string ToString()
        {
            return internalValue;
        }

    }
}
