using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace AutoDiscoveryLib
{
    public enum SandboxSerializeDataType
    {
        StartMarker,
        UInt32,
        String,
        EndMarker
    }

    public interface ISandboxSerialize
    {
        byte[] Serialize();
        void Deserialize(byte[] data);
        void Deserialize(byte[] data, int numBytes);
    }
}