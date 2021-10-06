using System.Collections.Generic;
using UnityEngine;
using System;

namespace Unity.Netcode.Addons
{
    public interface INetworkReference<T> : IEquatable<T>
    {
        void NetworkRead(FastBufferReader reader);
        void NetworkWrite(FastBufferWriter writer);
    }
}
